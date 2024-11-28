/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantRpc.Thrift.Server
 文件名：ThriftServer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/5/16 18:07:38

 功能描述：

----------------------------------------------------------------*/

using Microsoft.Extensions.Logging;
using System;
using Thrift;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Processor;
using Thrift.Transport;
using Thrift.Transport.Server;

using static SuperGMS.Rpc.Thrift.Server.ThriftService;
using System.Threading;

namespace SuperGMS.Rpc.Thrift.Server
{
    /// <summary>
    ///
    /// </summary>
    public class ThriftRpcServer : SuperGMSBaseServer
    {
        private TThreadPoolAsyncServer ts;
        private TServerSocketTransport serverTransport;

        protected override void Dispose()
        {
            ts.Stop();
            serverTransport.Close();
        }

        protected override void ServerRegister(SuperGMSServerConfig server)
        {
            try
            {
                serverTransport = new TServerSocketTransport(server.Port,null);

                // 传输协议
                TBinaryProtocol.Factory factory = new TBinaryProtocol.Factory();
                AsyncProcessor processor = new ThriftService.AsyncProcessor(this);
                ts = new TThreadPoolAsyncServer(processor, serverTransport, new TTransportFactory(), factory);
                ts.ServeAsync(CancellationToken.None).Wait();
            }
            catch (TTransportException tex)
            {
                logger.LogError(tex, "ThriftServer.ServerRegister.TTransportException.Error");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ThriftServer.ServerRegister.Exception.Error");
                throw;
            }
        }
    }
}