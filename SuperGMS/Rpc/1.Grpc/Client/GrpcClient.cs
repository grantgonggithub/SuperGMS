/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Rpc.Grpc
 文件名：GrpcClient
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2025/1/14 15:11:38

 功能描述：

----------------------------------------------------------------*/
using Grpc.Net.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SuperGMS.Log;
using SuperGMS.Rpc.Client;
using SuperGMS.Rpc.Thrift.Client;

using System;

namespace SuperGMS.Rpc.Grpc.Client
{
    internal class GrpcClient : ISuperGMSRpcClient
    {
        private GrpcService.GrpcServiceClient client;
        private GrpcChannel channel;
        private ClientItem clientItem;
        
        //标记客户端到服务端的连接是否可用,发送异常时将标记为fasle

        private readonly static ILogger logger = LogFactory.CreateLogger<ThriftClient>();
        public ClientItem Item
        {
            get { return clientItem; }
        }

        public bool IsConnected => channel.State== ConnectivityState.Ready;

        public GrpcClient(ClientItem _client)
        {
            try
            {
                channel = GrpcChannel.ForAddress($"http://{_client.Ip}:{_client.Port}");
                //if(channel.State!=ConnectivityState.Ready)
                //   channel.ConnectAsync().Wait();
                client = new GrpcService.GrpcServiceClient(channel);
                clientItem = _client;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SuperGMS.GrantRpc.Thrift.Client.RpcClientRegister.Error");
            }
        }


        /// <summary>
        /// 连接可用时,把用完的连接放入连接池
        /// 否则释放掉
        /// </summary>
        public void Dispose()
        {
            if (channel.State== ConnectivityState.Ready)
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
            try
            {
                if (channel != null)
                {
                    channel.ShutdownAsync().Wait();
                    channel.Dispose();
                    channel = null;
                    client = null;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SuperGMS.Rpc.Grpc.Client.Close.Error");
            }
        }

        public bool Send(string args, string m, out string result)
        {
            var rsp = client.SendAsync(new GRpcRequest { MyArgs = args }).GetAwaiter().GetResult();
            result = rsp.MyResult;
            return true;
        }
    }
}
