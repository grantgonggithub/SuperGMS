/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Redis
 文件名：RedisNode
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/3/24 14:40:26

 功能描述：业务模型节点，在此模型下可能会存在多个redis服务器，共同实现当前业务Node的功能
           会根据模型的特点，实现主从，或者单主，或者同质节点

----------------------------------------------------------------*/
using System;
using Microsoft.Extensions.Logging;
using SuperGMS.Log;
using StackExchange.Redis;

namespace SuperGMS.Redis
{
    /// <summary>
    /// 业务模型节点，在此模型下可能会存在多个redis服务器，共同实现当前业务Node的功能
    /// 会根据模型的特点，实现主从，或者单主，或者同质节点
    /// </summary>
   public class RedisNode
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<RedisNode>();
        /// <summary>
        /// 节点名称
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// 是否支持主从
        /// </summary>
        public bool IsMasterSlave { get; set; }

        /// <summary>
        /// 从节点
        /// </summary>
        public RedisServer[] SlaveServers { get; set; }

        /// <summary>
        /// 主节点，如果不区分主从，全部按从来取
        /// </summary>
        public RedisServer MasterServer { get; set; }

        /// <summary>
        /// 写库
        /// </summary>
        /// <param name="fn">fn</param>
        /// <param name="needServer"></param>
        /// <returns>bool</returns>
        public bool Set(Func<IDatabase,IServer, bool> fn,bool needServer=false)
        {
            try
            {
                if (this.IsMasterSlave)
                {
                    return setMaster(fn,needServer);
                }
                else
                {
                    return setSlave(fn,needServer);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "redis.set 异常");
                return false;
            }
        }

        /// <summary>
        /// 写库
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        public bool Set(Func<IDatabase, bool> fn)
        {
            return Set((db, server) => fn(db));
        }

        /// <summary>
        /// 查库
        /// </summary>
        /// <param name="fn">fn</param>
        /// <param name="needServer"></param>
        /// <returns>T</returns>
        public T Get<T>(Func<IDatabase,IServer, T> fn,bool needServer=false)
        {
            try
            {
                return GetSlave<T>(fn,needServer);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "redis Get<T> Error");
                return default(T);
            }
        }

        /// <summary>
        /// 查库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fn"></param>
        /// <returns></returns>
        public T Get<T>(Func<IDatabase, T> fn)
        {
            return Get((db, server) => fn(db));
        }

        #region  set 操作

        /// <summary>
        /// 操作主库，一般都是set操作
        /// </summary>
        /// <param name="fn">fn</param>
        /// <param name="needServer"></param>
        /// <returns>bool</returns>
        private bool setMaster(Func<IDatabase, IServer, bool> fn, bool needServer = false)
        {
            int error = 0;
        gotoHere:
            ConnectionMultiplexer conn = null;
            try
            {
                conn = RedisConnectionManager.GetConnection(this.MasterServer);
                if (conn == null)
                {
                    return false;
                }

                IDatabase db = conn.GetDatabase(this.MasterServer.DbIndex);
                IServer server = needServer? conn.GetServer(this.MasterServer.Server, this.MasterServer.Port):null;
                return fn(db, server);
            }
            catch (Exception ex)
            {
                if (error < 2)//出错可以重试两次
                {
                    if (conn != null)
                    {
                        RedisConnectionManager.Dispose(this.MasterServer, conn);  // conn = null;//把这个链接设置为空，防止第二次还是被取出来
                    }

                    error += 1;
                    System.Threading.Thread.Sleep(1000);
                    goto gotoHere;
                }

                logger.LogError(ex, "RedisNode.setMaster.Error");
                return false;
            }
        }

        /// <summary>
        /// 操作从库，一般是不分主从的set,不能乱了
        /// </summary>
        /// <param name="fn">fn</param>
        /// <param name="needServer"></param>
        /// <returns>bool</returns>
        private bool setSlave(Func<IDatabase,IServer, bool> fn, bool needServer=false)
        {
            int error = 0;
        gotHere:
            if (this.SlaveServers == null || this.SlaveServers.Length < 1)
            {
                return false;
            }

            int num = 0;
            ConnectionMultiplexer conn = null;
            for (int i = 0; i < this.SlaveServers.Length; i++)
            {
                try
                {
                    conn = RedisConnectionManager.GetConnection(this.SlaveServers[i]);
                    if (conn == null)
                    {
                        continue;
                    }

                    IDatabase db = conn.GetDatabase(this.SlaveServers[i].DbIndex);
                    IServer server =needServer? conn.GetServer(this.SlaveServers[i].Server, this.SlaveServers[i].Port):null;
                    num += fn(db,server) ? 1 : 0;
                }
                catch (Exception ex)
                {
                    if (error < 2)
                    {
                        if (conn != null)
                        {
                            RedisConnectionManager.Dispose(this.SlaveServers[i], conn); // conn = null;
                        }

                        error += 1;
                        System.Threading.Thread.Sleep(1000);
                        goto gotHere;
                    }
                    logger.LogError(ex, "RedisNode.SetSlave.Error");
                }
            }

            return num == this.SlaveServers.Length;
        }



        #endregion

        #region get操作

        private T GetSlave<T>(Func<IDatabase,IServer, T> fn,bool needServer=false)
        {
            int error = 0;
        gotoHere:
            ConnectionMultiplexer conn = null;
            int idx = DateTime.Now.Millisecond % SlaveServers.Length; // 如果读从库，随机取
            try
            {
                conn = RedisConnectionManager.GetConnection(SlaveServers[idx]);
                if (conn == null)
                {
                    return default(T);
                }

                IDatabase db = conn.GetDatabase(this.SlaveServers[idx].DbIndex);
                IServer server = needServer? conn.GetServer(this.SlaveServers[idx].Server, this.SlaveServers[idx].Port):null;
                try
                {
                    return fn(db, server);  // 回调异常和连接异常分开处理，回调异常是业务处理异常，连接异常底层做重试
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"RedisNode.GetSlave<T>类型{typeof(T).FullName}转换异常");
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                if (error < 2)
                {
                    if (conn != null)
                    {
                        // 释放掉坏连接
                        RedisConnectionManager.Dispose(SlaveServers[idx], conn);
                    }

                    error += 1;
                    System.Threading.Thread.Sleep(1000);
                    goto gotoHere;
                }
                logger.LogError(ex, "RedisNode.GetSlave<T>.Error");
                return default(T);
            }
        }

        #endregion
    }
}
