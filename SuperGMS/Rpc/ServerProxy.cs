/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS
 文件名：Publisher
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 11:43:12

 功能描述：

----------------------------------------------------------------*/
using Microsoft.Extensions.Logging;

using SuperGMS.Config;
using SuperGMS.Log;
using SuperGMS.Rpc.Grpc.Server;
using SuperGMS.Rpc.HttpWebApi;
using SuperGMS.Rpc.Thrift.Server;

using System;
using System.Net;
using System.Threading;

namespace SuperGMS.Rpc
{
    public class ServerProxy
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<ServerProxy>();

        /// <summary>
        /// 一般在Program里面注册
        /// 如果是NetCore3.1用TypeOf(Program).Namespace
        /// 如果是Net5,Net6用TypeOf(Program).Module.Name
        /// 如果你觉得麻烦，请直接用另外一个构造吧
        /// 注意这一句是Program最后一句代码，任何放到这句后的代码都不会被执行
        /// </summary>
        /// <param name="programNamespace"></param>
       // [Obsolete("建议用更简单的构造：Register(Type typeOfClass)")]
        public static void Register(string programNamespace, string[] args=null)
        {
            if (string.IsNullOrEmpty(programNamespace))
                throw new Exception("argument programNamespace is Null");
            ServicePointManager.SetTcpKeepAlive(true, 5*60*1000, 5*60*1000); // 设置系统socket如果5分钟没有交互，需要发送心跳包，进行保活

            //初始化线程池最小大小 为 1000
            ThreadPool.GetMinThreads(out var wt, out var ct);
            if (wt < 1000 || ct < 1000)
            {
                ThreadPool.SetMinThreads(Math.Max(wt, 1000), Math.Max(ct, 1000));
            }
            var serviceName = AssemblyTools.AssemblyToolProxy.GetCurrentAppName(programNamespace.TrimEnd(".dll".ToCharArray()));
            var rpcServer = ServerSetting.GetRpcServer(serviceName);
            string assPath = string.IsNullOrEmpty(programNamespace)
                ? rpcServer.AssemblyPath
                : programNamespace.EndsWith(".dll")
                    ? programNamespace
                    : programNamespace + ".dll";
            if (rpcServer != null)
            {
                SuperGMSServerConfig s = new SuperGMSServerConfig()
                {
                    Port = rpcServer.Port,
                    ServerType = rpcServer.ServerType,
                    AssemblyPath = assPath,
                    Pool = rpcServer.Pool,
                    Ip = rpcServer.Ip,
                    TimeOut = rpcServer.TimeOut,
                    Enable = rpcServer.Enable,
                    PortList = rpcServer.PortList,
                    ServerName=serviceName
                };
                Register(s,args);
            }
            logger.LogInformation($"ThreadPool.GetMinThreads: worker-{Math.Max(wt, 1000)}; io-{Math.Max(ct, 1000)}");

            // 阻止主线程，防止退出
            Thread.Sleep(int.MaxValue);
        }

        /// <summary>
        /// 一般在Program里面注册时用TypeOf(Program)
        /// 注意这一句是Program最后一句代码，任何放到这句后的代码都不会被执行
        /// </summary>
        /// <param name="typeOfClass">TypeOf(Program)</param>
        public static void Register(Type typeOfClass, string[] args=null)
        {
            if (typeOfClass == null)
                throw new Exception("argument typeOfClass is Null");
            Register(typeOfClass.Module.Name,args);
        }
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            server.QtDispose();
        }

        private static SuperGMSBaseServer server = null;

        /// <summary>
        /// 通过配置直接注册使用
        /// </summary>
        /// <param name="config">配置文件</param>
        internal static void Register(SuperGMSServerConfig config, string[] args)
        {
            switch (config.ServerType)
            {
                default:
                case ServerType.Thrift:
                    server = new ThriftRpcServer();
                    break;
                case ServerType.Grpc:
                    server=new GrpcServer();
                    break;
                case ServerType.HttpWebApi:
                    server=new WebApiServer();
                    break;
            }

            if (server == null)
            {
            }
            else
            {
                // 开启线程去监听，要不当前主线程会被hang住
                Thread th = new Thread(new ThreadStart(() =>
                {
                    server.RpcServerRegister(config,args);
                }));
                th.IsBackground = true;
                th.Name = string.Format("rpc_thread_{0}", config.AssemblyPath);
                th.Start();
                Console.CancelKeyPress += Console_CancelKeyPress;
            }
        }

        /// <summary>
        /// httpProxy做完业务处理时的调用方式，这些参数直接从前端来的，不用做转换
        /// </summary>
        /// <param name="args"></param>
        /// <param name="appContext"></param>
        /// <returns></returns>
        internal static string HttpSend(string args,object appContext)
        {
            if (server == null)
            {
                throw new Exception("请先Register服务");
            }
            return server.Send(args, appContext).Result;
        }
    }
}
