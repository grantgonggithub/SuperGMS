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
        public static void Register(string programNamespace)
        {
            ServicePointManager.SetTcpKeepAlive(true, 5*60*1000, 5*60*1000); // 设置系统socket如果5分钟没有交互，需要发送心跳包，进行保活

            //初始化线程池最小大小 为 1000
            ThreadPool.GetMinThreads(out var wt, out var ct);
            if (wt < 1000 || ct < 1000)
            {
                ThreadPool.SetMinThreads(Math.Max(wt, 1000), Math.Max(ct, 1000));
            }
            var rpcServer = ServerSetting.GetRpcServer();
            string assPath = string.IsNullOrEmpty(programNamespace)
                ? rpcServer.AssemblyPath
                : programNamespace.EndsWith(".dll")
                    ? programNamespace
                    : programNamespace + ".dll";
            if (rpcServer != null)
            {
                GrantServerConfig s = new GrantServerConfig()
                {
                    Port = rpcServer.Port,
                    ServerType = rpcServer.ServerType,
                    AssemblyPath = assPath,
                    Pool = rpcServer.Pool,
                    Ip = rpcServer.Ip,
                    TimeOut = rpcServer.TimeOut,
                    Enable = rpcServer.Enable,
                    PortList = rpcServer.PortList,
                };
                Register(s);
            }
            logger.LogInformation($"ThreadPool.GetMinThreads: worker-{Math.Max(wt, 1000)}; io-{Math.Max(ct, 1000)}");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            server.QtDispose();
        }

        private static GrantBaseServer server = null;

        public static void Register(GrantServerConfig config)
        {
            switch (config.ServerType)
            {
                default:
                case ServerType.Thrift:
                    server = new GrantThriftServer();
                    break;
                case ServerType.Grpc:
                    break;
                case ServerType.WCF:
                    break;
                case ServerType.TaskWorker:
                    server = new TaskWorker.TaskWorker();
                    break;
                case ServerType.HttpWebApi:
                    server=new GrantWebApiServer();
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
                    server.RpcServerRegister(config);
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
            return server.Send(args, appContext);
        }
    }
}
