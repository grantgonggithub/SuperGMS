/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.GrantRpc.Client
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
using GrantMicroService.Config;
using GrantMicroService.Log;
using GrantMicroService.Rpc.HttpWebApi;
using GrantMicroService.Rpc.Thrift.Client;
using GrantMicroService.Tools;

namespace GrantMicroService.Rpc.Client
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
        private static readonly Dictionary<string, List<ComboxClass<DateTime, IGrantRpcClient>>> ConnectionPools = new Dictionary<string, List<ComboxClass<DateTime, IGrantRpcClient>>>();
        private readonly static ILogger logger = LogFactory.CreateLogger<ClientConnectionManager>();
        private static object root = new object();
        private static bool runing = false;

        /// <summary>
        /// 获取一个到指定服务器的连接
        /// </summary>
        /// <param name="item">配置信息</param>
        /// <returns>rpc连接</returns>
        public static IGrantRpcClient GetClient(ClientItem item)
        {
            if (item.Pool > 0) return Register(item);// 0,表示开启连接池（默认），1，表示关闭连接池
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
                    List<ComboxClass<DateTime, IGrantRpcClient>> cls = ConnectionPools[key];
                    if (cls.Count > 0)
                    {
                        // 从队列中取一个连接，并移除掉，防止二次分配
                        IGrantRpcClient c = cls[0].V2;
                        cls.Remove(cls[0]);
                        return c;
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
        public static void ReleaseClient(IGrantRpcClient client)
        {
            if (client.Item.Pool > 0){
                client.Close();
                return;// 关闭连接池，连接不在放回,直接释放掉
            }
            string key = GetConnectionPoolKey(client.Item.Ip, client.Item.Port);
            lock (root)
            {
                if (ConnectionPools.ContainsKey(key))
                {
                    ConnectionPools[key]
                        .Add(new ComboxClass<DateTime, IGrantRpcClient> { V1 = DateTime.Now, V2 = client });
                }
                else
                {
                    List<ComboxClass<DateTime, IGrantRpcClient>> cls = new List<ComboxClass<DateTime, IGrantRpcClient>>(1)
                    {
                        new ComboxClass<DateTime, IGrantRpcClient> { V1 = DateTime.Now, V2 = client },
                    };
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
                            ComboxClass<DateTime, IGrantRpcClient>[] clients = ConnectionPools[s].ToArray();
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
        private static IGrantRpcClient Register(ClientItem item)
        {
            try
            {
                IGrantRpcClient client = null;
                switch (item.ServerType)
                {
                    case ServerType.Thrift:
                        client = new GrantThriftClient(item);
                        break;
                    case ServerType.HttpWebApi:
                        client=new GrantWebApiClient(item);
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

                                // 连接池里面里连接大于1的时候才会清理，防止清空了就起不到连接池的作用了
                                if (lst.Count > 1)
                                {
                                    // 2分钟不被使用，就清理掉
                                    var timeOut = lst.Where(a => DateTime.Now.Subtract(a.V1).TotalSeconds > 60 * 2)
                                        .ToArray();
                                    foreach (var item in timeOut)
                                    {
                                        // 始终保持有1个，超时也不能清空
                                        if (lst.Count > 1)
                                        {
                                            // 把超时的移除掉 , 释放掉连接
                                            lst.Remove(item);
                                            item.V2.Close();
                                        }
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