/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Thrift.Server
 文件名：ConnectionManager
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 14:13:20

 功能描述：Rpc服务器的管理器

----------------------------------------------------------------*/

using System;
using SuperGMS.Rpc.Thrift.Server;
using Thrift;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Transport;

namespace SuperGMS.Rpc.Manage
{
    /// <summary>
    /// Rpc服务器的管理器
    /// </summary>
    public class ServerManager
    {
        private static ServerManager _instance = null;
        private static object _objLock = new object();

        /// <summary>
        /// 要注册的服务器列表，注意一个服务只能注册一次，否则会失败（port不能重复注册）
        /// </summary>
        /// <param name="servers"></param>
        public void Register(GrantServer[] servers)
        {
            try
            {
                if (servers == null || servers.Length < 1) throw new Exception("no Server can Register");
                foreach (GrantServer s in servers)
                {
                    TServerSocket serverTransport = new TServerSocket(s.Config.Port);
                    TBinaryProtocol.Factory factory = new TBinaryProtocol.Factory();//传输协议
                    TProcessor processor = new GrantService.Processor(s.Server);
                    TServer server = new TThreadPoolServer(processor, serverTransport, new TTransportFactory(), factory);
                }
            }
            catch (TTransportException tex)
            {
                //等log组件改造完之后再记录
            }
            catch (Exception ex)
            {
                //等log组件改造完之后再记录
            }
        }

        /// <summary>
        /// 主从RpcServer
        /// </summary>
        /// <param name="servers"></param>
        public static void RegisterServer(GrantServer[] servers)
        {
            if (_instance == null)
            {
                lock (_objLock)
                {
                    if (_instance == null)
                    {
                        _instance = new ServerManager();
                        _instance.Register(servers);
                    }
                }
            }
        }
    }
}