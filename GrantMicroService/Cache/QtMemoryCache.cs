using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.Cache
{
    /// <summary>
    /// 内存缓存, 单元测试使用
    /// </summary>
    public class QtMemoryCache : ICache
    {
        private static Microsoft.Extensions.Caching.Memory.MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        public bool IsExistCfg()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual bool Set(string key, string value, TimeSpan? expiry = null)
        {
            cache.Set(key, value, expiry.HasValue ? expiry.Value : new TimeSpan(0, 20, 20));
            return true;
        }

        /// <inheritdoc />
        public virtual bool Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            cache.Set<T>(key, value, expiry.HasValue ? expiry.Value : new TimeSpan(0, 20, 20));
            return true;
        }

        /// <inheritdoc />
        public virtual void SetHash<T>(string key, KeyValuePair<string, T> value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual void SetHash<T>(string key, IDictionary<string, T> values)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual string Get(string key)
        {
            return cache.Get<string>(key);
        }

        /// <inheritdoc />
        public virtual T Get<T>(string key)
        {
            return cache.Get<T>(key);
        }

        /// <inheritdoc />
        public virtual T GetHash<T>(string key, string filed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual Dictionary<string, T> GetHashAll<T>(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual bool RemoveKey(string key)
        {
            cache.Remove(key);
            return true;
        }

        /// <inheritdoc />
        public virtual bool RemoveHashKey(string key, string filed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual bool ContainsKey(string key)
        {
            return cache.TryGetValue(key, out var res);
        }

        /// <inheritdoc />
        public virtual void HashIncrement(string key, string filed, long value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual void HashDecrement(string key, string filed, long value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual void StringIncrement(string key, long value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual void StringDecrement(string key, long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Redis是单进程单线程，不存在并发问题，因此只要保证多条命令原则执行即可
        /// </summary>
        /// <param name="key">队列名称</param>
        /// <param name="value">值</param>
        /// <param name="queueSize">值</param>
        /// <returns>是否</returns>
        public bool PushQueue(string key, string value, int queueSize = 1000)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 弹出 最大的日志信息
        /// </summary>
        /// <param name="key">队列名称</param>
        /// <returns>最大数量日志</returns>
        public List<string> PopAllQueue(string key)
        {
            throw new NotImplementedException();
        }

        public bool LockTake(string key, string value, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public string LockQuery(string key, string value)
        {
            throw new NotImplementedException();
        }

        public bool LockRelease(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}