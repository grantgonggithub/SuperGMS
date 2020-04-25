/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Cache.redis
 文件名：DefaultRedis
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/18 13:30:03

 功能描述：提供一个DefaultRedis的配置

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace GrantMicroService.Cache
{
    /// <summary>
    /// 默认的Default的Redis
    /// </summary>
    // [InitlizeMethod]
    public class CacheManager
    {
        public const string RedisConfig = "RedisConfig";
        /// <summary>
        /// 操作redis的实例
        /// </summary>
        private static ICache instance;

        /// <summary>
        /// 在用之前需要初始化，这里只所以不做成单例的原因是不想做锁判断
        /// </summary>
        // [InitlizeMethod]
        internal static void Initlize(Config.RedisConfig redisConfig)
        {
            if (redisConfig == null)
            {
                return;
            }
            Redis.RedisConfig.Initlize(redisConfig);
            instance = new RedisCache();

            ResourceCache.Initlize(); // 初始化ResourceCache
        }
        /// <summary>
        /// 单元测试时, Mock 缓存时,使用,手动初始化
        /// </summary>
        /// <param name="cache">Mock 的缓存</param>
        public static void Initlize(ICache cache)
        {
            if (instance == null && cache != null)
            {
                instance = cache;
            }
        }
        /// <summary>
        /// 放缓存
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="expiry">expiry</param>
        /// <returns>是否成功</returns>
        public static bool Set(string key, string value, TimeSpan? expiry = null)
        {
            return instance.Set(key, value, expiry);
        }

        /// <summary>
        /// 放缓存
        /// </summary>
        /// <typeparam name="T">vlue的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="expiry">expiry</param>
        /// <returns>成功与否</returns>
        public static bool Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            return instance.Set<T>(key, value, expiry);
        }

        /// <summary>
        /// 存HashSet
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public static void SetHash<T>(string key, KeyValuePair<string, T> value)
        {
            instance.SetHash<T>(key, value);
        }

        /// <summary>
        /// 存HashSet
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <param name="values">values</param>
        public static void SetHash<T>(string key, IDictionary<string, T> values)
        {
            instance.SetHash<T>(key, values);
        }

        /// <summary>
        /// 获取键值
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>string</returns>
        public static string Get(string key)
        {
            return instance.Get(key);
        }

        /// <summary>
        /// 获取键值
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public static T Get<T>(string key)
        {
            return instance.Get<T>(key);
        }

        /// <summary>
        /// 获取HashSet一条记录
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">key</param>
        /// <param name="filed">列</param>
        /// <returns>该泛型的列值</returns>
        public static T GetHash<T>(string key, string filed)
        {
            return instance.GetHash<T>(key, filed);
        }

        /// <summary>
        /// 取HashSet全部记录
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">key</param>
        /// <returns>所有值</returns>
        public static Dictionary<string, T> GetHashAll<T>(string key)
        {
            return instance.GetHashAll<T>(key);
        }

        /// <summary>
        /// 移除一个指定的key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>成功与否</returns>
        public static bool RemoveKey(string key)
        {
            return instance.RemoveKey(key);
        }

        /// <summary>
        /// 移除HashKey
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">列</param>
        /// <returns>成功与否</returns>
        public static bool RemoveHashKey(string key, string filed)
        {
            return instance.RemoveKey(key);
        }

        /// <summary>
        /// 查找是否包含key的值
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>bool</returns>
        public static bool ContainsKey(string key)
        {
            return instance.ContainsKey(key);
        }

        /// <summary>
        /// 给Hash的某一个filed 增加 value 个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">列</param>
        /// <param name="value">值</param>
        public static void HashIncrement(string key, string filed, long value)
        {
            instance.HashIncrement(key, filed, value);
        }

        /// <summary>
        /// 给Hash的某一个filed 减去 value 个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">列</param>
        /// <param name="value">值</param>
        public static void HashDecrement(string key, string filed, long value)
        {
            instance.HashDecrement(key, filed, value);
        }

        /// <summary>
        /// 给一个字符串增加 value个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public static void StringIncrement(string key, long value)
        {
            instance.StringIncrement(key, value);
        }

        /// <summary>
        /// 给一个字符串减去 value个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public static void StringDecrement(string key, long value)
        {
            instance.StringDecrement(key, value);
        }

        /// <summary>
        /// Redis是单进程单线程，不存在并发问题，因此只要保证多条命令原则执行即可
        /// </summary>
        /// <param name="key">队列名称</param>
        /// <param name="value">值</param>
        /// <param name="queueSize">值</param>
        /// <returns>是否</returns>
        public static bool PushQueue(string key, string value,int queueSize = 1000)
        {
            return instance.PushQueue(key, value, queueSize);
        }

        /// <summary>
        /// 弹出 最大的日志信息
        /// </summary>
        /// <param name="key">队列名称</param>
        /// <returns>最大数量日志</returns>
        public static List<string> PopAllQueue(string key)
        {
            return instance.PopAllQueue(key);
        }
    }
}