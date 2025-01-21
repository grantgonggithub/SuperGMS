/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Redis
 文件名：RedisConnectionManager
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/3/24 14:36:16

 功能描述：redis服务器连接管理器，根据模型和服务器节点作为key管理

----------------------------------------------------------------*/
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using SuperGMS.Log;

using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.Redis
{
    /// <summary>
    /// Redis链接的管理器,主要是根据业务模型+索引来管理连接
    /// </summary>
    public class RedisConnectionManager
    {
        private static readonly object _lockObj = new object();
        private static readonly Dictionary<string, ConnectionMultiplexer> ConnectionCache = new Dictionary<string, ConnectionMultiplexer>();
        private readonly static ILogger logger = LogFactory.CreateLogger<RedisConnectionManager>();
        /// <summary>
        /// 单例获取
        /// </summary>
        public static ConnectionMultiplexer GetConnection(RedisServer srvCfg)
        {
            // var date = DateTime.Now;
            lock (_lockObj)
            {
                // 业务模型节点+索引确定一个连接
                string key = string.Format("{0}_{1}_{2}", srvCfg.Node.NodeName, srvCfg.Server, srvCfg.Port);
                ConnectionMultiplexer conn = null;
                if (ConnectionCache.ContainsKey(key))
                {
                    conn = ConnectionCache[key];
                }

                if (conn == null || !conn.IsConnected)
                {
                    if (conn != null)
                    {
                        conn.Dispose(); // 如果一个连接断了，必须释放掉，才能重新建
                    }

                    // Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffffff") + "---Wait---" + DateTime.Now.Subtract(date).TotalMilliseconds);
                    conn = getConnection(srvCfg);
                    ConnectionCache[key] = conn;
                }

                return conn;
            }
        }

        /// <summary>
        /// 释放掉坏掉的连接
        /// </summary>
        /// <param name="srvCfg">srvCfg</param>
        /// <param name="conn">conn</param>
        public static void Dispose(RedisServer srvCfg, ConnectionMultiplexer conn)
        {
            lock (_lockObj)
            {
                try
                {
                    if (conn != null)
                    {
                        conn.Dispose();
                        conn = null;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "RedisConnectionManager.Dispose.Error");
                }

                string key = string.Format("{0}_{1}_{2}", srvCfg.Node.NodeName, srvCfg.Server, srvCfg.Port);

                if (ConnectionCache.ContainsKey(key))
                {
                    ConnectionCache.Remove(key);
                }
            }
        }

        private static ConnectionMultiplexer getConnection(RedisServer cfg)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}:{1}", cfg.Server, cfg.Port);
                sb.AppendFormat(",allowAdmin={0}", cfg.AllowAdmin);
                sb.AppendFormat(",abortConnect=false,connectRetry=3,syncTimeout={0}",cfg.SyncTimeout<=3000?3000:cfg.SyncTimeout);
                sb.AppendFormat(",connectTimeout={0}", cfg.ConnectTimeout>0?cfg.ConnectTimeout:5000);
                sb.AppendFormat(",ssl={0},password={1}", cfg.Ssl2, cfg.Pwd);

                //return new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(sb.ToString())).Value;
                return ConnectionMultiplexer.Connect(sb.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "redis连接创建失败");
                return null;
            }
        }
    }
}
