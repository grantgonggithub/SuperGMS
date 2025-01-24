using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGMS.Redis
{
    /// <summary>
    /// Redis高级操作
    /// </summary>
    public partial class RedisProxy
    {

        /// <summary>
        /// Redis是单进程单线程，不存在并发问题，因此只要保证多条命令原则执行即可
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="logicName">业务名称</param>
        /// <param name="key">队列名称</param>
        /// <param name="value">值</param>
        /// <param name="queueSize">值</param>
        /// <returns>是否</returns>
        public static bool PushQueue(string nodeName, string logicName, string key, string value,int queueSize =1000)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Set(db =>
            {
                var rtn = db.ListLeftPush(k, value);
                db.ListTrim(k, 0, queueSize);
                return true;
            });
        }

        /// <summary>
        /// 弹出 最大的日志信息
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="logicName">业务名称</param>
        /// <param name="key">队列名称</param>
        /// <returns>最大数量日志</returns>
        public static List<string> PopAllQueue(string nodeName, string logicName, string key)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Get<List<string>>(db =>
            {
                var list = db.ListRange(k);
                return list?.Select(x => x.ToString()).ToList();
            });
        }

        public static bool LockTake(string nodeName, string logicName, string key,string value,TimeSpan expiry)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Get<bool>(db =>
            {
                return db.LockTake(k,value, expiry);
            });
        }

        public static bool LockRelease(string nodeName, string logicName, string key, string value)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Get<bool>(db =>
            {
                return db.LockRelease(k,value, CommandFlags.FireAndForget);
            });
        }
        public static string LockQuery(string nodeName, string logicName, string key)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Get<string>(db =>
            {
                return db.LockQuery(k);
            });
        }
    }
}
