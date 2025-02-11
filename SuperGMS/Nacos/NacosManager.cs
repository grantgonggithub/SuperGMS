/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Nacos
 文件名：  NacosManager
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2025/1/20 18:10:27

 功能描述：

----------------------------------------------------------------*/
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Nacos.OpenApi;
using Nacos.V2;
using Nacos.V2.Config;
using Nacos.V2.Naming;
using Nacos.V2.Naming.Dtos;
using Nacos.V2.Naming.Event;

using SuperGMS.Config;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SuperGMS.Nacos
{
    public class NacosManager
    {
        private static  NacosConfigService _configService;
        private static NacosNamingService _namingService;
        private const int timeOutMs = 5 * 1000;
        public const string NACOS_SERVICE_INFO = "metadata_service_Info";

        /// <summary>
        /// 初始化Nacos
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="configCenter"></param>
        /// <param name="environmentName">在nacos页面配置的Namespace值，对应的系统的环境变量名</param>
        public static void Initlize(LoggerFactory loggerFactory, ConfigCenter configCenter,string environmentName)
        {
            environmentName = (string.IsNullOrEmpty(environmentName) ? "dev" : environmentName);// 如果取不到环境变量就默认dev
            NacosSdkOptions sdkOptions = new NacosSdkOptions()
            {
                ConfigUseRpc = true,
                DefaultTimeOut = configCenter.SessionTimeout,
                Namespace = environmentName,
                NamingUseRpc = true,
                Password = configCenter.Password,
                UserName = configCenter.UserName,
                ServerAddresses = configCenter.Ip.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
                NamingCacheRegistryDir = AppDomain.CurrentDomain.BaseDirectory
            };
            var options = Options.Create(sdkOptions);
            var httpClientFactory = new ServiceCollection().AddHttpClient().BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            DefaultNacosOpenApi defaultNacosOpenApi=new DefaultNacosOpenApi(httpClientFactory, options);

            // 自动创建NameSpaces
            var nameSpaces = defaultNacosOpenApi.GetNamespacesAsync().Result;
            if (!nameSpaces?.Any((NacosNamespace x) => x.Namespace == environmentName)??true)
            {
                bool isOk = defaultNacosOpenApi.CreateNamespaceAsync(environmentName, environmentName, environmentName).Result;
            }

            _configService = new NacosConfigService(loggerFactory, options);
            
            _namingService = new NacosNamingService(loggerFactory, options, httpClientFactory);
        }


        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="dataId">配置节的名称</param>
        /// <param name="group">微服务的名称作为group</param>
        /// <returns></returns>
        public static string GetConfig(string dataId, string group)
        {
           return _configService.GetConfig(dataId, group, timeOutMs).Result;
        }


        public static void AddConfigListener(string dataId,string group,IListener listener)
        { 
            _configService.AddListener(dataId,group, listener).Wait(timeOutMs);
        }

        /// <summary>
        /// 添加配置
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="group"></param>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool PublishConfig(string dataId,string group,string content,string type)
        {
           return _configService.PublishConfig(dataId, group, content, type).Result;
        }

        public static List<Instance> GetAllNamingServices(string serviceName, string group,List<string> clusters,bool healthy,bool subscribe) { 
           return _namingService.SelectInstances(serviceName, group,clusters, healthy, subscribe).Result;
        }

        public static Instance GetOneNamingServices(string serviceName, string group, List<string> clusters, bool healthy, bool subscribe)
        {
            return _namingService.SelectOneHealthyInstance(serviceName, group, clusters, subscribe).Result;
        }

        public static void RegisterInstance(string serviceName,string ip,int port,string clusterName)
        { 
            _namingService.RegisterInstance(serviceName,ip,port, clusterName).Wait(timeOutMs);
        }

        public static void RegisterInstance(string serviceName, string groupName, Instance instance)
        {
            _namingService.RegisterInstance(serviceName, groupName, instance).Wait(timeOutMs);
        }

        public static bool Subscribe(string serviceName,string group,List<string> clusters, IEventListener listener)
        {
           return _namingService.Subscribe(serviceName,group,clusters,listener).Wait(timeOutMs);
        }

        public static bool UnSubscribe(string serviceName, string group, List<string> clusters, IEventListener listener)
        {
           return _namingService.Unsubscribe(serviceName, group, clusters, listener).Wait(timeOutMs);
        }
       

        public static void ShutDown()
        { 
            _namingService.ShutDown().Wait();
        }

        public class Listener : IListener
        {
            private Action<string, string> callBack;
            private string listenKey;
            public Listener(Action<string,string> _callBack,string _listenKey) {
                callBack = _callBack;
                listenKey = _listenKey;
            }
            public void ReceiveConfigInfo(string configInfo)
            {
                callBack(listenKey,configInfo);
            }
        }

        public class EventListener : IEventListener
        {
            private Action<string, InstancesChangeEvent> callBack;
            private string listenKey;

            public EventListener(Action<string, InstancesChangeEvent> _callBack, string _listenKey)
            {
                this.callBack = _callBack;
                this.listenKey = _listenKey;
            }

            public Task OnEvent(IEvent @event)
            {
                if (@event is InstancesChangeEvent changeEvent)
                {
                    return Task.Run(() => callBack(listenKey, changeEvent));
                }
                return Task.CompletedTask;
            }
        }

        public static Stream GetStream(string jsonConfig) {
            var byteValue = Encoding.UTF8.GetBytes(jsonConfig);
            return  new MemoryStream(byteValue, 0, byteValue.Length);
        }

    }
}
