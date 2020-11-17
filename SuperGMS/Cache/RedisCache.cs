using System;
using System.Collections.Generic;
using System.Text;
using SuperGMS.Redis;

namespace SuperGMS.Cache
{
    /// <summary>
    /// Redis Cache 实现类
    /// </summary>
    public class RedisCache : ICache
    {
        private string _redisName = "default";

        /// <summary>
        /// 默认使用的 Default
        /// </summary>
        public RedisCache()
        {
        }

        /// <summary>
        /// 可以规定存储区
        /// </summary>
        /// <param name="redisName"></param>
        public RedisCache(string redisName)
        {
            _redisName = redisName;
        }

        public bool IsExistCfg()
        {
            return RedisProxy.IsExistCfg(_redisName);
        }

        /// <summary>
        /// 放缓存
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="expiry">expiry</param>
        /// <returns>是否成功</returns>
        public bool Set(string key, string value, TimeSpan? expiry = null)
        {
            expiry = expiry == null ? new TimeSpan(24, 0, 0) : expiry;
            return RedisProxy.Set(_redisName, _redisName, key, value, expiry);
        }

        /// <summary>
        /// 放缓存
        /// </summary>
        /// <typeparam name="T">vlue的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="expiry">expiry</param>
        /// <returns>bool</returns>
        public bool Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            expiry = expiry == null ? new TimeSpan(24, 0, 0) : expiry;
            return RedisProxy.Set<T>(_redisName, _redisName, key, value, expiry);
        }

        /// <summary>
        /// 存HashSet
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void SetHash<T>(string key, KeyValuePair<string, T> value)
        {
            RedisProxy.SetHash<T>(_redisName, _redisName, key, value);
        }

        /// <summary>
        /// 存HashSet
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <param name="values">value</param>
        public void SetHash<T>(string key, IDictionary<string, T> values)
        {
            RedisProxy.SetHash<T>(_redisName, _redisName, key, values);
        }

        /// <summary>
        /// 获取键值
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>string</returns>
        public string Get(string key)
        {
            return RedisProxy.Get(_redisName, _redisName, key);
        }

        /// <summary>
        /// 获取键值
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <returns>TT</returns>
        public T Get<T>(string key)
        {
            return RedisProxy.Get<T>(_redisName, _redisName, key);
        }

        /// <summary>
        /// 获取HashSet一条记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <returns>TT</returns>
        public T GetHash<T>(string key, string filed)
        {
            return RedisProxy.GetHash<T>(_redisName, _redisName, key, filed);
        }

        /// <summary>
        /// 取HashSet全部记录
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Dictionary</returns>
        public Dictionary<string, T> GetHashAll<T>(string key)
        {
            return RedisProxy.GetHashAll<T>(_redisName, _redisName, key);
        }

        /// <summary>
        /// 移除一个指定的key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>bool</returns>
        public bool RemoveKey(string key)
        {
            return RedisProxy.RemoveKey(_redisName, _redisName, key);
        }

        /// <summary>
        /// 移除HashKey
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <returns>bool</returns>
        public bool RemoveHashKey(string key, string filed)
        {
            RedisProxy.RemoveHashKey(_redisName, _redisName, key, filed);
            return true;
        }

        /// <summary>
        /// 查找是否包含key的值
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>bool</returns>
        public bool ContainsKey(string key)
        {
            return RedisProxy.ContainsKey(_redisName, _redisName, key);
        }

        /// <summary>
        /// 给Hash的某一个filed 增加 value 个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <param name="value">value</param>
        public void HashIncrement(string key, string filed, long value)
        {
            RedisProxy.HashIncrement(_redisName, _redisName, key, filed, value);
        }

        /// <summary>
        /// 给Hash的某一个filed 减去 value 个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <param name="value">value</param>
        public void HashDecrement(string key, string filed, long value)
        {
            RedisProxy.HashDecrement(_redisName, _redisName, key, filed, value);
        }

        /// <summary>
        /// 给一个字符串增加 value个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void StringIncrement(string key, long value)
        {
            RedisProxy.StringIncrement(_redisName, _redisName, key, value);
        }

        /// <summary>
        /// 给一个字符串减去 value个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void StringDecrement(string key, long value)
        {
            RedisProxy.StringDecrement(_redisName, _redisName, key, value);
        }

        /// <summary>
        /// Redis是单进程单线程，不存在并发问题，因此只要保证多条命令原则执行即可
        /// </summary>
        /// <param name="key">队列名称</param>
        /// <param name="value">值</param>
        /// <param name="queueSize">size</param>
        /// <returns>是否</returns>
        public bool PushQueue(string key, string value, int queueSize = 1000)
        {
            return RedisProxy.PushQueue(_redisName, _redisName, key, value, queueSize);
        }

        /// <summary>
        /// 弹出 最大的日志信息
        /// </summary>
        /// <param name="key">队列名称</param>
        /// <returns>最大数量日志</returns>
        public List<string> PopAllQueue(string key)
        {
            return RedisProxy.PopAllQueue(_redisName, _redisName, key);
        }

        /// <summary>
        /// 默认超时 ：1分钟
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool LockTake(string key, string value, TimeSpan? expiry = null)
        {
            expiry = expiry ?? new TimeSpan(0, 1 , 0);
            return RedisProxy.LockTake(_redisName, "Lock", key,value, expiry.Value);
        }

        /// <summary>
        /// 查询锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string LockQuery(string key, string value)
        {
            return RedisProxy.LockQuery(_redisName, "Lock", key);
        }

        /// <summary>
        /// 释放锁 ，值必须对才可以释放
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool LockRelease(string key, string value)
        {
            return RedisProxy.LockRelease(_redisName, "Lock", key, value);
        }
    }
}