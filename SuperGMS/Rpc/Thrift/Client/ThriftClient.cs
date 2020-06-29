/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Thrift.Client
 文件名：GrantClient
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:08:57

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using SuperGMS.Log;
using SuperGMS.Rpc.Client;
using SuperGMS.Rpc.Thrift.Server;
using Thrift.Protocol;
using Thrift.Transport;

namespace SuperGMS.Rpc.Thrift.Client
{
    /// <summary>
    /// 客户端
    /// </summary>
    public class ThriftClient : ISuperGMSRpcClient
    {
        private ThriftService.Client client;
        private TTransport transport;
        private ClientItem clientItem;
        //标记客户端到服务端的连接是否可用,发送异常时将标记为fasle
        private bool isConnected = false;
        private readonly static ILogger logger = LogFactory.CreateLogger<ThriftClient>();

        /// <inheritdoc />
        public ClientItem Item
        {
            get { return clientItem; }
        }
        /// <summary>
        /// 打开一个Thrift 连接
        /// </summary>
        /// <param name="client">连接配置</param>
        public ThriftClient(ClientItem client)
        {
            try
            {
                clientItem = client;
                transport = new TSocket(client.Ip, client.Port, client.TimeOut);
                transport.Open();
                this.client = new ThriftService.Client(new TBinaryProtocol(transport));
                isConnected = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SuperGMS.GrantRpc.Thrift.Client.RpcClientRegister.Error");
            }
        }

        /// <inheritdoc/>
        public bool Send(string args,string m, out string result)
        {
            result = null;
            try
            {
                result = client.Send(args, null);
                return true;
            }
            catch (Exception ex)
            {
                isConnected = false;
                logger.LogError(ex, $"Thrift.Client.Send.Error,链接:{transport.GetHashCode()}");
                return false;
            }
        }

        /// <summary>
        /// 连接可用时,把用完的连接放入连接池
        /// 否则释放掉
        /// </summary>
        public void Dispose()
        {
            if (isConnected)
            {
                //客户端连接可用时,释放回连接池
                ClientConnectionManager.ReleaseClient(this);
            }
            else
            {
                // 释放掉
                Close();
            }
        }

        /// <summary>
        /// 关闭连接，请慎用，这个主要是提供给连接池管理用来释放连接的
        /// 业务千万不能用这个
        /// </summary>
        public void Close()
        {
            isConnected = false;
            try
            {
                if (transport != null)
                {
                    if (transport.IsOpen)
                    {
                        transport.Close();
                    }
                    transport.Dispose();
                    transport = null;
                }
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SuperGMS.GrantRpc.Thrift.Client.Dispose.Error");
            }
        }
    }
}