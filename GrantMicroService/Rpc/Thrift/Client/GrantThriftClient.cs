/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Thrift.Client
 文件名：GrantClient
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:08:57

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using GrantMicroService.Log;
using GrantMicroService.Rpc.Client;
using GrantMicroService.Rpc.Thrift.Server;
using Thrift.Protocol;
using Thrift.Transport;

namespace GrantMicroService.Rpc.Thrift.Client
{
    /// <summary>
    /// 客户端
    /// </summary>
    public class GrantThriftClient : IGrantRpcClient
    {
        private GrantService.Client client;
        private TTransport transport;
        private ClientItem clientItem;
        //标记客户端到服务端的连接是否可用,发送异常时将标记为fasle
        private bool isConnected = false;
        private readonly static ILogger logger = LogFactory.CreateLogger<GrantThriftClient>();

        /// <inheritdoc />
        public ClientItem Item
        {
            get { return clientItem; }
        }
        /// <summary>
        /// 打开一个Thrift 连接
        /// </summary>
        /// <param name="client">连接配置</param>
        public GrantThriftClient(ClientItem client)
        {
            try
            {
                clientItem = client;
                transport = new TSocket(client.Ip, client.Port, client.TimeOut);
                transport.Open();
                this.client = new GrantService.Client(new TBinaryProtocol(transport));
                isConnected = true;
                logger.LogDebug($"创建链接:{transport.GetHashCode()},{client.Ip}:{client.Port},");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GrantMicroService.GrantRpc.Thrift.Client.RpcClientRegister.Error");
            }
        }

        /// <inheritdoc/>
        public bool Send(string args,string m, out string result)
        {
            result = null;
            try
            {
                result = client.Send(args, null);
                logger.LogDebug($"使用链接:{transport.GetHashCode()}发送数据");
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
                    logger.LogDebug($"关闭链接:{transport.GetHashCode()},{clientItem.Ip}:{clientItem.Port}");
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
                logger.LogError(ex, "GrantMicroService.GrantRpc.Thrift.Client.Dispose.Error");
            }
        }
    }
}