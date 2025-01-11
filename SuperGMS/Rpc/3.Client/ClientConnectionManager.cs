/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantRpc.Client
 文件名：ClientConnectionManager
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 17:19:17

 功能描述：thrift客户端连接池管理

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using SuperGMS.Config;
using SuperGMS.Log;
using SuperGMS.Rpc.Grpc.Client;
using SuperGMS.Rpc.HttpWebApi;
using SuperGMS.Rpc.Thrift.Client;
using SuperGMS.Tools;

namespace SuperGMS.Rpc.Client
{
    /// <summary>
    /// thrift 连接池管理类
    /// </summary>
    public class ClientConnectionManager
    {
        /// <summary>
        /// 根据服务器的ip和端口来保存当前客户端到这台服务器的所有连接, DataTime 是最后连接时间
        /// </summary>
        /// <example> <![CDATA[ Dictionary<ip, List<一个连接>> ]]></example>
        private static readonly Dictionary<string,Queue<ComboxClass<DateTime, ISuperGMSRpcClient>>> ConnectionPools = new Dictionary<string, Queue<ComboxClass<DateTime, ISuperGMSRpcClient>>>();
        private readonly static ILogger logger = LogFactory.CreateLogger<ClientConnectionManager>();
        private static object root = new object();
        private static bool runing = false;

        /// <summary>
        /// 获取一个到指定服务器的连接
        /// </summary>
        /// <param name="item">配置信息</param>
        /// <returns>rpc连接</returns>
        public static ISuperGMSRpcClient GetClient(ClientItem item)
        {
            // if (item.Pool > 0) return Register(item);// 0,表示开启连接池（默认），1，表示关闭连接池
            string key = GetConnectionPoolKey(item.Ip, item.Port);
            lock (root)
            {
                if (!runing)
                {
                    Check();
                    runing = true;
                }

                if (ConnectionPools.ContainsKey(key))
                {
                    var cls = ConnectionPools[key];
                    tryOne:
                    if (cls.TryDequeue(out var c))
                    {
                        // 从队列中取一个连接，并移除掉，防止二次分配
                        //ISuperGMSRpcClient c = cls.Dequeue().V2;
                        if (c == null || c.V2 == null||!c.V2.IsConnected)
                            goto tryOne;
                        else
                          return c.V2;
                    }
                }
            }

            // 说明没有取到，这里需要新建,新建的直接返回，不能放到连接池，只有用完的才会放回去
            return Register(item);
        }

        /// <summary>
        /// 释放连接回到连接池，回到连接池的连接都是可用的，
        /// 连接只有在用的时候才会检查状态
        /// </summary>
        /// <param name="client"> 客户端</param>
        public static void ReleaseClient(ISuperGMSRpcClient client)
        {
            //if (client.Item.Pool > 0){
            //    client.Close();
            //    return;// 关闭连接池，连接不在放回,直接释放掉
            //}

            if (client == null || !client.IsConnected) // 如果连接不正常就不放回去了
            {
                client.Close();
                client = null;
            }

            string key = GetConnectionPoolKey(client.Item.Ip, client.Item.Port);
            lock (root)
            {
                var newOne = new ComboxClass<DateTime, ISuperGMSRpcClient> { V1 = DateTime.Now, V2 = client };
                if (ConnectionPools.ContainsKey(key))
                {
                    ConnectionPools[key]
                        .Enqueue(newOne);
                }
                else
                {
                    var cls = new Queue<ComboxClass<DateTime, ISuperGMSRpcClient>>();
                    cls.Enqueue(newOne);
                    ConnectionPools.Add(key, cls);
                }
            }
        }

        /// <summary>
        /// 将连接池所有连接全部释放
        /// </summary>
        public static void Dispose()
        {
            if (ConnectionPools.Count > 0)
            {
                lock (root)
                {
                    if (ConnectionPools.Count > 0)
                    {
                        string[] keys = ConnectionPools.Keys.ToArray();
                        foreach (string s in keys)
                        {
                            ComboxClass<DateTime, ISuperGMSRpcClient>[] clients = ConnectionPools[s].ToArray();
                            foreach (var c in clients)
                            {
                                c.V2.Close();
                            }
                        }

                        ConnectionPools.Clear();
                    }
                }
            }
        }

        private static string GetConnectionPoolKey(string ip, int port)
        {
            return string.Format("{0}_{1}", ip, port);
        }

        /// <summary>
        /// 新建一个连接
        /// </summary>
        /// <param name="item">连接配置</param>
        /// <returns>连接</returns>
        private static ISuperGMSRpcClient Register(ClientItem item)
        {
            try
            {
                ISuperGMSRpcClient client = null;
                switch (item.ServerType)
                {
                    case ServerType.Thrift:
                        client = new ThriftClient(item);
                        break;
                    case ServerType.Grpc:
                        client=new GrpcClient(item);
                        break;
                    case ServerType.HttpWebApi:
                        client=new WebApiClient(item);
                        break;
                    default:
                        throw new Exception($"ClientConnectionManager.Register(), ClientItem.ServerType:'{item.ServerType}' is invalid");
                }
                return client;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ClientConnectionManager.register.Error,ServerInfo={item.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// 这里需要定时清理不用的连接
        /// </summary>
        private static void Clear()
        {
            while (true)
            {
                try
                {
                    string[] key = ConnectionPools.Keys.ToArray();
                    if (key.Length > 0)
                    {
                        // 为了保证性能，随机检查，不做全量遍历
                        Random random = new Random(DateTime.Now.Millisecond);
                        int idx = random.Next(0, key.Length);
                        lock (root)
                        {
                            if (ConnectionPools.ContainsKey(key[idx]))
                            {
                                var lst = ConnectionPools[key[idx]];
                                // 2分钟不被使用，就清理掉
                                var timeOut = lst.Where(a => DateTime.Now.Subtract(a.V1).TotalSeconds > 60 * 2 && a.V2 != null)
                                    .ToArray();
                                foreach (var item in timeOut)
                                {
                                    if (item != null && item.V2 != null)
                                    {
                                        // 把超时的移除掉 , 释放掉连接
                                        //lst.Remove(item);
                                        item.V2.Close();
                                        item.V2 = null;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ClientConnectionManager.Clear.Error");
                }

                var rpcSocketIntervalTime = ServerSetting.GetConstValue("RpcSocketRecycleIntervalMin")?.Value;
                int timeSpan = 0;
                int.TryParse(rpcSocketIntervalTime ?? "3", out timeSpan);
                if (timeSpan < 1) timeSpan = 3;
                // 3分钟检查一次,这里只是为了防止连接长期防止缓存池，检查频度不要求太高（2分钟过期+3分钟检查间隔其实就是5分钟）
                Thread.Sleep(timeSpan * 60 * 1000);
            }
        }

        private static void Check()
        {
            Thread th = new Thread(new ThreadStart(Clear))
            {
                IsBackground = true,
                Name = "ClientConnectionManager.check.thread",
            };
            th.Start();
        }
    }
}