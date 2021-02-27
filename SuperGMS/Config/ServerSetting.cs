/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Config
 文件名：  ServerSetting
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/11/13 18:03:45

 功能描述：全局配置管理类，可以进行热更新配置

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using org.apache.zookeeper;
using SuperGMS.Cache;
using SuperGMS.Config.RemoteJsonFile;
using SuperGMS.HttpProxy;
using SuperGMS.Log;
using SuperGMS.MQ;
using SuperGMS.Router;
using SuperGMS.Tools;
using SuperGMS.Zookeeper;

namespace SuperGMS.Config
{
    /// <summary>
    /// ServerSetting
    /// </summary>
    public class ServerSetting
    {
        private static ConfigCenter configCenter;
        private static Configuration config;

        private static string _appName;

        private static int _pool;

        private static Action<string, Configuration> updateConfigurationAction;

        private static Dictionary<string,Action<Configuration>> callBackList = new Dictionary<string, Action<Configuration>>();

        public readonly string ServerInfo = ServiceEnvironment.EnvironmentInfo;

        private static ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Gets appName
        /// </summary>
        public static string AppName
        {
            get
            {
                return _appName;
            }
        }

        /// <summary>
        /// 初始化AppName, 此方法仅给单元测试提供调用, 只有_appName没有值时,才能设置
        /// </summary>
        /// <param name="value">Name</param>
        public static void SetAppNameForTest(string value)
        {
            if (string.IsNullOrEmpty(_appName))
            {
                _appName = value;
            }
        }

        /// <summary>
        /// 配置类型
        /// </summary>
        public static ConfigCenter ConfigCenter
        {
            get { return configCenter; }
        }

        /// <summary>
        /// Gets config
        /// </summary>
        public static Configuration Config
        {
            get { return config; }
        }

        /// <summary>
        /// Gets pool
        /// </summary>
        public static int Pool
        {
            get { return _pool; }
        }

        private static object _objLock = new object();

        /// <summary>
        /// 获取ConstValue配置信息
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>ConstKeyValue</returns>
        public static ConstItem GetConstValue(string key)
        {
            return ConfigManager.GetConstKeyValue(key);
        }

        /// <summary>
        /// 获取File服务器的地址列表
        /// </summary>
        /// <returns>FileServer[]</returns>
        public static FileServerItem GetFileServers()
        {
            var servers = FileServerManager.GetFileServers();
            Random r = new Random(DateTime.Now.Millisecond);
            int idx = r.Next(0, servers.Length);
            return servers[idx];
        }

        /// <summary>
        /// 外部关心的系统变更新，目前只有log在使用，其他的变更ServerSetting会帮着处理，不用关心变更
        /// </summary>
        /// <param name="updateAction">updateAction回调方法</param>
        /// <returns>Configuration</returns>
        public static Configuration GetConfiguration(Action<string, Configuration> updateAction = null)
        {
            updateConfigurationAction = updateAction;
            return config;
        }
        /// <summary>
        /// 注册zookeeper使用
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="enable"></param>
        /// <param name="timeout"></param>
        public static void RegisterRouter(string serverName, string ip, int port, bool enable,int timeout)
        {
            if (configCenter.ConfigType != ConfigType.Zookeeper)
            {
                return; // 本地配置不走zookeeper;
            } 
           string path = ZookeeperManager.SetRouter(serverName, ip, port, enable, timeout);
            var cfgWatcher = new ConfigWatcher();
            cfgWatcher.OnChange += (string p) =>
            {
                var changeData = ZookeeperManager.GetNodeData(path, cfgWatcher);
                if (!string.IsNullOrEmpty(changeData))
                {
                    try
                    {
                        var clientItem = Newtonsoft.Json.JsonConvert.DeserializeObject<ClientItem>(changeData);
                        logger.LogWarning($"我的配置被修改为：{changeData}");
                        if (clientItem != null && !clientItem.Enable)
                        {
                            // 我被下线了，更新本地配置"Enable": false，下一次重启需要带上这个状态
                            logger.LogInformation($"我被管理员下线了，哼。。。。serverName={serverName},ip={ip},port = {port}");
                        }
                        else
                        {
                            logger.LogInformation($"我被管理员上线了,哈哈哈哈。。。。serverName={serverName},ip={ip},port = {port}");
                        }
                        // 在这里修改本地配置快照
                        if(clientItem==null||string.IsNullOrEmpty(clientItem.Ip)||clientItem.Port<1) return;
                        config.ServerConfig.RpcService.Ip = clientItem.Ip;
                        config.ServerConfig.RpcService.Port = clientItem.Port;
                        config.ServerConfig.RpcService.Pool = clientItem.Pool;
                        config.ServerConfig.RpcService.Enable = clientItem.Enable;
                        config.ServerConfig.RpcService.TimeOut = clientItem.TimeOut;
                        //copyConfig(); 暂时注掉，这里还没思考好
                    }
                    catch (Exception e)
                    {
                        logger.LogCritical(e, $"RegisterRouter.cfgWatcher.OnChange.Error,serverName={serverName},ip={ip},port = {port}");
                    }
                }
               
            };
            ZookeeperManager.GetNodeData(path, cfgWatcher); // 监控自己router节点的内容，有可能被置为下线；
        }

        /// <summary>
        /// HttpProxy 配置,主要配置那些微服务可以提供外部api服务
        /// </summary>
        /// <param name="proxyName">proxyName</param>
        /// <param name="updateAction">updateAction</param>
        /// <returns>HttpProxy</returns>
        public static Configuration GetHttpProxy(string proxyName, Action<Configuration> updateAction)
        {
            switch (configCenter.ConfigType)
            {
                case ConfigType.Local:
                case ConfigType.HttpFile:
                    return config; // 直接返回本地配置
                case ConfigType.Zookeeper:
                    Action<Configuration> callBack = (Configuration cfgValue) =>
                    {
                        GetHttpProxy(proxyName, updateAction); // 调用一次方法，挂载回调
                        updateAction(cfgValue); // 重连之后要执行回调，做变更
                    };
                    callBackList[proxyName] = callBack; // 断线重连之后，要把当前方法封装起来，作为回调

                    var router = new ConfigWatcher();
                    router.OnChange += (string path) =>
                    {
                        if (updateAction != null && !string.IsNullOrEmpty(path))
                        {
                            var proxyStr = ZookeeperManager.GetNodeData(path, router);
                            if (string.IsNullOrEmpty(proxyStr))
                            {
                                return;
                            }
                            var httpProxy = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(proxyStr);
                            config.HttpProxy = httpProxy.HttpProxy;
                            updateAction(config);
                        }
                    };
                    string p = ZookeeperManager.getConfigPath(proxyName);
                    var cfg = ZookeeperManager.GetNodeData(p + "/" + SuperHttpProxy.HttpProxy, router);
                    if (string.IsNullOrEmpty(cfg))
                    {
                        logger.LogWarning("获取代理层配置为空，这个代理层将无法提供代理服务 ServerSetting.GetHttpProxy is Null");
                    }
                    else
                    {
                        var proxy = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(cfg);
                        config.HttpProxy = proxy.HttpProxy;
                    }
                    return config;
            }

            return null;
        }

        /// <summary>
        /// RpcClinet 客户端初始化信息，路由
        /// </summary>
        /// <param name="appName">appName</param>
        /// <returns>XElement</returns>
        public static Configuration GetAppClient(string appName, Action<Configuration> updateAction)
        {
            switch (configCenter.ConfigType)
            {
                case ConfigType.Local:
                case ConfigType.HttpFile:
                    return config; // 直接返回本地配置
                case ConfigType.Zookeeper:
                    // 去远程拉取，因为初始化的时候，是初始化的当前服务的配置，
                    // 引用客户端的配置需要分别拉取
                    // 因为zk没有提供拉节点数据和子节点的接口，只能先拉到Node然后在依次拉子节点的数据
                    var serviceRouterWatcher = new ServiceRouterWatcher();
                    serviceRouterWatcher.OnChange += (string path) =>
                    {
                        if (updateAction != null && !string.IsNullOrEmpty(path))
                        {
                            config.RpcClients = getRouters(appName, serviceRouterWatcher);
                            updateAction(config);
                        }
                    };

                    Action<Configuration> callBack = (Configuration cfgValue) =>
                    {
                        GetAppClient(appName, updateAction); // 调用一次方法，挂载回调
                        updateAction(config); // 重连之后要执行回调，做变更
                    };

                    callBackList[appName] = callBack; // 断线重连之后，要把当前方法封装起来，作为回调

                    config.RpcClients = getRouters(appName, serviceRouterWatcher);

                    ZookeeperManager.SetRelation(appName, _appName); // 设置调用关系

                    return config;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 获取某个服务在集群中的所有负载地址，包括自己,注意必须是基于zk部署，或者集中配置中心才行，单个super.json的配置是无法知道负载地址的
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns></returns>
        public static Client GetAllClientsByServiceName(string serviceName)
        {
           return  config?.RpcClients?.Clients?.FirstOrDefault(x => x.ServerName.ToLower() == serviceName.ToLower());
        }

        private static RpcClients getRouters(string appName,Watcher serviceRouterWatcher)
        {
            List<string> nodeList = ZookeeperManager.GetRouterChildren(appName,serviceRouterWatcher); // 路由是整个获取节点整个跟节点监控，因为子节点是虚拟的
            if (nodeList == null || nodeList.Count < 1)
            {
                string msg = $"你代码里面调用了 {appName} ,但是从zookeeper中取不到这个服务的路由信息，GetAppClient.appName={appName}.GetChildrenNode==null";
                logger.LogWarning(msg);
                return null;
            }

            RpcClients rpcClients = new RpcClients();
            rpcClients.Clients = new List<Client>();
            Client client = new Client() { RouterType = RouterType.Random, ServerName = appName };
            client.Items = new List<ClientItem>();
            rpcClients.Clients.Add(client);
            string p = ZookeeperManager.getRouterPath(appName);
            if (string.IsNullOrEmpty(p))
            {
                return null;
            }
            foreach (var item in nodeList)
            {
                string nodeData = ZookeeperManager.GetNodeData(p + "/" + item,serviceRouterWatcher);
                if (string.IsNullOrEmpty(nodeData))
                {
                    continue;
                }
                ClientItem clientItem = Newtonsoft.Json.JsonConvert.DeserializeObject<ClientItem>(nodeData);
                client.Items.Add(clientItem);
            }
            config.RpcClients = rpcClients;
            return rpcClients;
        }

        /// <summary>
        /// RpcServer 服务端
        /// 根据配置加载配置信息，凡是后端微服务不用自己初始化，
        /// 直接注册RpcServer就可以（RpcServer里面已经初始化了），如果也要注册RpcClient，RpcClient的注册要放在后面
        /// 在前端代理层，需要先初始化Initlize()
        /// </summary>
        /// <param name="appName">appName</param>
        /// <param name="pool">pool</param>
        public static void Initlize(string appName, int pool)
        {
            try
            {
                checkInitlize();
                
                ServerSetting._appName = appName;
                ServerSetting._pool = pool;

                switch (configCenter.ConfigType)
                {
                    default:
                    case ConfigType.Local:
                    case ConfigType.HttpFile:
                        UpdateLocal(config); // 本地的配置，一次就加载完整了
                        break;

                    case ConfigType.Zookeeper:
                        // 根据appName拉取信息，注册zk，这里需要注意热更新的问题
                        // 初始化连接，并注册对连接的监听
                        ZKConnectionWatcher connectionWatcher = new ZKConnectionWatcher();
                        connectionWatcher.OnChange += (string path) =>
                        {
                            ZookeeperManager.reConnection(new KeeperException.SessionExpiredException(),
                                () =>
                                {
                                    try
                                    {
                                        // Initlize(appName, pool); // 更新了config 
                                       initlizeData(appName);
                                    }
                                    catch (Exception e) // 重连的就不能抛异常了
                                    {
                                        logger.LogError(e, $"reConnection.Initlize.Error");
                                    }

                                    var list = callBackList.Values.ToArray();
                                    for (int i = 0; i < list.Length; i++)
                                    {
                                        try
                                        {
                                            list[i]?.Invoke(config);
                                            // callBackList[callBackList.Keys[i]]?.Invoke(config); // 依次执行回调链，保证更新及时
                                        }
                                        catch (Exception e)
                                        {
                                            logger.LogError(e, $"reConnection.callBack.Error");
                                        }
                                    }


                                    // 断线重连，注册自己
                                    RegisterRouter(ServerSetting.AppName,
                                        ServerSetting.Config.ServerConfig.RpcService.Ip,
                                        ServerSetting.Config.ServerConfig.RpcService.Port,
                                        ServerSetting.Config.ServerConfig.RpcService.Enable,
                                        ServerSetting.Config.ServerConfig.RpcService.TimeOut);

                                });
                        };
                        ZookeeperManager.Initlize(
                            configCenter.Ip,
                            configCenter.SessionTimeout, connectionWatcher);

                        initlizeData(appName);

                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "ServerSetting.Initlize.Error");
                throw ex;
            }
        }

        private static void initlizeData(string appName)
        {
            // 检查标准配置，第一次可能zk是空
            // 检查标准配置节点，帮助初始化
            var standConfig = getStandConfig();
            ZookeeperManager.CheckConfig(appName, standConfig);


            // 拉取当前AppName的配置，需要注册watcher
            var dataWatcher = new ConfigWatcher();

            dataWatcher.OnChange += (string path) =>
            {
                string configData = ZookeeperManager.GetNodeData(path, dataWatcher);
                if (string.IsNullOrEmpty(configData))
                {
                    return;
                }
                UpdateZookeeper(path, configData);
            };

            List<string>
                childrens = ZookeeperManager.GetConfigChildren(appName,
                    null); // 配置是整个获取节点，分别获取配置和分别增加watcher
            if (childrens != null && childrens.Count > 0)
            {
                string root = ZookeeperManager.getConfigPath(appName);
                if (string.IsNullOrEmpty(root))
                {
                    return;
                }
                foreach (var item in childrens)
                {
                    // 需要根据节点路径来判断是哪个节点变化了
                    string path = root + "/" + item;
                    string configData = ZookeeperManager.GetNodeData(path, dataWatcher);
                    UpdateZookeeper(path, configData);
                }
            }
        }

        private static Dictionary<string, string> getStandConfig()
        {
            Dictionary<string, string> standConfig = new Dictionary<string, string>();
            try
            {
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                standConfig.Add(SuperGMS.HttpProxy.SuperHttpProxy.HttpProxy, config.HttpProxy == null ? string.Empty : Newtonsoft.Json.JsonConvert.SerializeObject(new Configuration(){ HttpProxy = config.HttpProxy}, jsonSerializerSettings));
                string dbStr = string.Empty;
                if (config.DataBase != null && !string.IsNullOrEmpty(config.DataBase.DbFile))
                {
                    var db = DbModelContextManager.GetDataBase(config.DataBase.DbFile);
                    var map = DbModelContextManager.GetSqlMap();
                    dbStr = $"<DataBase>\r\n{db?.ToString()}\r\n{map?.ToString()}\r\n</DataBase>";
                }

                standConfig.Add(DbModelContextManager.DATABASE, dbStr);
                standConfig.Add(ConfigManager.CONSTKEYVALUE, config.ConstKeyValue == null ? string.Empty : Newtonsoft.Json.JsonConvert.SerializeObject(new Configuration(){ ConstKeyValue = config.ConstKeyValue}, jsonSerializerSettings));
                standConfig.Add(CacheManager.RedisConfig, config.RedisConfig == null ? string.Empty : Newtonsoft.Json.JsonConvert.SerializeObject(new Configuration(){RedisConfig = config.RedisConfig},jsonSerializerSettings));
                standConfig.Add(MQHostConfigManager.RabbitMQ, config.RabbitMQ == null ? string.Empty : Newtonsoft.Json.JsonConvert.SerializeObject(new Configuration(){ RabbitMQ = config.RabbitMQ},jsonSerializerSettings));
                standConfig.Add(FileServerManager.FileServerName, config.FileServer == null ? string.Empty : Newtonsoft.Json.JsonConvert.SerializeObject(new Configuration(){ FileServer = config.FileServer},jsonSerializerSettings));
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "初始化标准配置失败");
            }
            return standConfig;
        }


        /// <summary>
        /// 找到这个节点,并且替换
        /// </summary>
        /// <param name="xml">xml</param>
        // public static void ZookeeperUpdate(XElement xml)
        // {
        //    Update(xml);
        // }
        private static string dbXMl = "<DataBase RefFile=\"{0}\" DataBaseInfo=\"{1}\" />";

        /// <summary>
        /// Local的配置
        /// 系统的所有的基础性配置都会在这里初始化
        /// </summary>
        /// <param name="configuration">本地配置json</param>
        private static void UpdateLocal(Configuration configuration)
        {
            // ServerSetting的初始化依赖配置驱动的，只有配置了才会被初始化，如果某个微服务未使用到某个配置，不配置即可，这样可以做到按需初始化
            try
            {
                if (configuration.DataBase != null)
                {
                    var dbstr = string.Format(dbXMl, configuration.DataBase.RefFile, configuration.DataBase.DbFile);
                    if (!string.IsNullOrEmpty(dbstr))
                    {
                        XElement xml = XElement.Parse(dbstr);
                        DbModelContextManager.Initlize(xml); // 初始化数据库配置和sql脚本
                    }

                }

                ConfigManager.Initlize(configuration.ConstKeyValue); // 初始化常量

                CacheManager.Initlize(configuration.RedisConfig); // 初始化redis
                MQHostConfigManager.Initlize(configuration.RabbitMQ); // 是时候MQ
                FileServerManager.Initlize(configuration.FileServer); // 初始化文件服务器
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ServerSetting.UpdateLocal.Error");
                throw ex;
            }
        }

        /// <summary>
        /// 按推送节点更新
        /// </summary>
        /// <param name="path">变更路径</param>
        private static void UpdateZookeeper(string path, string configData)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (string.IsNullOrEmpty(configData))
            {
                return;
            }

            logger.LogDebug($"更新路径:{path},更新内容：{configData}");

            try
            {
                lock (_objLock)
                {
                    string[] ps = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                    switch (ps[ps.Length - 1])
                    {
                        case DbModelContextManager.DATABASE: // 数据库,从zookeeper推过来的直接就是数据库和脚本的xml内容，这个跟文件有区别，文件是通过 ref引到外部文件的
                                                             // DataBase db = Newtonsoft.Json.JsonConvert.DeserializeObject<DataBase>(configData);
                            var xmlStr = XElement.Parse(configData);
                            DbModelContextManager.Initlize(xmlStr);

                            // Config.DataBase = db; // 把本地的完整配置更新,数据库配置不需要更新全局
                            break;
                        case ConfigManager.CONSTKEYVALUE: // 常量
                            var keyValue =
                                Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(configData);
                            ConfigManager.Initlize(keyValue.ConstKeyValue);
                            Config.ConstKeyValue = keyValue.ConstKeyValue;
                            break;
                        case CacheManager.RedisConfig: // redis
                            var redisConfig =
                                Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(configData);
                            CacheManager.Initlize(redisConfig.RedisConfig);
                            Config.RedisConfig = redisConfig.RedisConfig;
                            break;
                        case MQHostConfigManager.RabbitMQ: // rabbitMQ
                            var mq = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(configData);
                            MQHostConfigManager.Initlize(mq.RabbitMQ);
                            Config.RabbitMQ = mq.RabbitMQ;
                            break;
                        case FileServerManager.FileServerName: // fileserver
                            var fs = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(configData);
                            FileServerManager.Initlize(fs.FileServer);
                            Config.FileServer = fs.FileServer;
                            break;
                        case SuperGMS.HttpProxy.SuperHttpProxy.HttpProxy: //HttpProxy
                            var pxyName = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(configData);
                            Config.HttpProxy = pxyName.HttpProxy;
                            break;
                    }

                    updateConfigurationAction?.Invoke(ps[ps.Length - 1], Config); // 执行用户自定义的回调，目前只有log在用，其他不用
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"ServerSetting.UpdateZookeeper.Error.path={path},configData={configData}");
            }
        }

        /// <summary>
        /// 这个是当前微服务第一个要调用的，在ServerProxy的Register()里面用
        /// 这个是获取本地配置，因为rpcServer的配置只在本地配置文件中
        /// 这里只设置了 configCenter，rpcServer
        /// 这里还没有设置config,只有经过Initlize才设置
        /// </summary>
        /// <returns>XElement</returns>
        public static RpcService GetRpcServer(string servserName="")
        {
            checkInitlize();
            if (string.IsNullOrEmpty(servserName))
            {
                return config.ServerConfig.RpcService;
            }
            else
            {
                var ports = config.ServerConfig.RpcService.PortList;
                if (ports!=null&&ports.Count>0)
                {
                    if (ports.ContainsKey(servserName))
                    {
                        config.ServerConfig.RpcService.Port = ports[servserName];
                    }
                }
            }
            return config.ServerConfig.RpcService;
        }

        private static void checkInitlize()
        {
            if (config?.ServerConfig == null)
            {
                lock (_objLock)
                {
                    if (config?.ServerConfig == null)
                    {
                        InitConfiguration();
                    }
                }
            }
        }

        /// <summary>
        /// 根据dbContextName 获取对应得物理数据库的信息
        /// </summary>
        /// <param name="dbContextName">dbContextName</param>
        /// <returns>DbModelContext</returns>
        internal static DbModelContext GetDbModelContext(string dbContextName)
        {
            return DbModelContextManager.GetDbModelContext(dbContextName);
        }

        /// <summary>
        /// 根据dbContext的名称和sqlKey获取sql语句
        /// </summary>
        /// <param name="dbContextName">dbContextName</param>
        /// <param name="sqlKey">sqlKey</param>
        /// <returns>string</returns>
        internal static string GetSql(string dbContextName, string sqlKey)
        {
            return SqlMapManager.GetSql(dbContextName, sqlKey);
        }
        private static string configFile = "config{0}.json";
        /// <summary>
        /// 初始化服务配置(本地super.json中必须指明读取的配置源)
        /// </summary>
        /// <returns></returns>
        private static void InitConfiguration()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(env)) // 不配置就走config.json
                configFile = string.Format(configFile, "");
            else
            {
                env = env.ToLower().Trim();
                //switch (env)
                //{
                //    case "dev":
                //        configFile = string.Format(configFile, ".dev");
                //        break;
                //    case "test":
                //        configFile = string.Format(configFile, ".test");
                //        break;
                //    case "prod":
                //        configFile = string.Format(configFile, ".prod");
                //        break;
                //    default:
                //        configFile = string.Format(configFile, "");
                //        break;
                //}
                configFile = string.Format(configFile, "."+env);
            }
            var configJson = Path.Combine(AppContext.BaseDirectory, configFile);
            // 在NLog配置没有初始化之前，只能靠Console输出日志
            Console.WriteLine($"当前的环境变量是:{env},加载的指向配置文件是:{configJson}\r\n");
            if (!File.Exists(configJson))
                throw new Exception($"在当前服务运行目录中找不到配置文件{configFile}");
            //配置源优先从服务本地根目录下的config.json获取,
            IConfiguration configSource = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
              .AddJsonFile(configFile, optional: true, reloadOnChange: false).Build();
            ServerSetting.configCenter = configSource.GetSection("ServerConfig:ConfigCenter").Get<ConfigCenter>();
            //如未获取到,则配置源默认为ConfigType.Local
            if (ServerSetting.configCenter == null)
            {
                ServerSetting.configCenter = new ConfigCenter() { ConfigType = ConfigType.Local};
            }
            var settingConfigBuilder = new ConfigurationBuilder();
            string configPath = string.Empty;
            switch (ServerSetting.configCenter.ConfigType)
            {
                //配置源为ConfigType.Local时,优先读取内部config.json文件,不存在则读取上级目录Conf下的config.json
                case ConfigType.Local:
                    configPath = Path.Combine(AppContext.BaseDirectory, configFile);
                    if (File.Exists(configPath))
                    {
                        settingConfigBuilder.SetBasePath(AppContext.BaseDirectory).AddJsonFile(configFile, optional: false, reloadOnChange: false);
                    }
                    else
                    {
                        var dirInfo = new DirectoryInfo(AppContext.BaseDirectory);
                        var conf = Path.Combine(dirInfo.Parent.Parent.FullName, "conf");
                        bool isExist = false;
                        if (Directory.Exists(conf))
                        {
                            configPath = Path.Combine(conf, configFile);
                            if (File.Exists(configPath))
                            {
                                settingConfigBuilder.SetBasePath(conf).AddJsonFile(configFile, optional: false, reloadOnChange: false);
                                isExist = true;
                            }
                        }
                        if (!isExist)
                            throw new Exception($"请检查配置文件的路径是否存在{conf}/{configFile}");
                    }
                    break;
                case ConfigType.HttpFile:
                    settingConfigBuilder.Sources.Add(new RemoteJsonFileConfigurationSource() { Uri = ServerSetting.configCenter.Ip, Optional = false });
                    configPath = ServerSetting.configCenter.Ip;
                    break;
                default:
                    throw new Exception($"The setting 'ConfigCenter.ConfigType':'{(int)ServerSetting.configCenter.ConfigType}' not support now!");
            }
            Console.WriteLine($"最终的配置类型是:{ServerSetting.configCenter.ConfigType}, 最终的配置文件路径是:{configPath}\r\n");
            var settingsConfig = settingConfigBuilder.Build();
            //加载Nlog配置
            NLog.LogManager.Configuration = new NLogLoggingConfiguration(settingsConfig.GetSection("NLog"));
            logger = LogFactory.CreateLogger<ServerSetting>();
            //加载服务配置项
            ServerSetting.config = settingsConfig.Get<Configuration>();
            ServerSetting.configCenter = config.ServerConfig.ConfigCenter;
            if (config.ServerConfig.RpcService == null)
            {
                string msg = "请检查配置中super.json的配置是否正确，保证子节点RpcService的配置完整....";
                logger.LogCritical(msg);
                Thread.Sleep(1000);
                throw new Exception(msg);
            }
            config.ConfigPath = ServerSetting.configCenter.Ip;
            getLocalIp();
        }
        /// <summary>
        /// 通过通配符来匹配本地Ip，主要为了适应多台机器共享配置，多台机器肯定是同一个局域网，Ip规则如：192.168.100.121  192.168.100.126  所以通配符配置如：192.168.100.*  因为对于多网卡的机器来说，多个网卡肯定是不同网段的，否则就没有意义了，所以使用通配符既可以适应多台机器，又能过滤出来正确的Ip
        /// </summary>
        private static void getLocalIp()
        {
            if (string.IsNullOrEmpty(config.ServerConfig.RpcService.Ip))
                config.ServerConfig.RpcService.Ip = ServiceEnvironment.ComputerAddress;
            else
            {
                string configIp = config.ServerConfig.RpcService.Ip.Trim().Replace(" ", "");
                logger.LogInformation($"获取到的本机Ip列表是:{string.Join(",", ServiceEnvironment.IpList)},配置文件中的IP是：{configIp}");
                int endPos = configIp.IndexOf("*");
                if (endPos > 0) // 通配符
                {
                    bool isFind = false;
                    if (ServiceEnvironment.IpList != null && ServiceEnvironment.IpList.Count > 0)
                    {
                        string ipPart = configIp.Substring(0, endPos);
                        foreach (var ip in ServiceEnvironment.IpList)
                        {
                            var orgIp = ip.Trim().Replace(" ", "");
                            if (orgIp.StartsWith(ipPart))
                            {
                                ServiceEnvironment.ComputerAddress = orgIp;
                                config.ServerConfig.RpcService.Ip = orgIp;
                                isFind = true;
                                break;
                            }
                        }
                    }
                    if(!isFind)
                    {
                        throw new Exception("当前机器的Ip无法和RpcServer配置的Ip规则匹配到结果，请检查配置是否正确");
                    }
                }
                else // 说明是完整Ip
                {
                    ServiceEnvironment.ComputerAddress = config.ServerConfig.RpcService.Ip;
                }
            }
            logger.LogInformation($"最终使用的Ip是：{config.ServerConfig.RpcService.Ip}");
        }
        internal static void UpdateRpcPort(int port)
        {
            config.ServerConfig.RpcService.Port = port;
        }
    }
}