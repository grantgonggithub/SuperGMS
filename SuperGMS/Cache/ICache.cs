/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Cache
 文件名：  ICache
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/23 17:28:33

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.Cache
{
    /// <summary>
    /// ICache
    /// </summary>
   public interface ICache
    {
        /// <summary>
        /// 是否存在Redis配置
        /// </summary>
        /// <returns></returns>
        bool IsExistCfg();

        /// <summary>
        /// Set
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="expiry">expiry</param>
        /// <returns>bool</returns>
        bool Set(string key, string value, TimeSpan? expiry = null);

        /// <summary>
        /// 放缓存
        /// </summary>
        /// <typeparam name="T">vlue的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="expiry">expiry</param>
        /// <returns>bool</returns>
        bool Set<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// 存HashSet
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        void SetHash<T>(string key, KeyValuePair<string, T> value);

        /// <summary>
        /// 存HashSet
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="values">value</param>
        void SetHash<T>(string key, IDictionary<string, T> values);

        /// <summary>
        /// 获取键值
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>string</returns>
        string Get(string key);

        /// <summary>
        /// 获取键值
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <returns>TT</returns>
        T Get<T>(string key);

        /// <summary>
        /// 获取HashSet一条记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <returns>TT</returns>
        T GetHash<T>(string key, string filed);

        /// <summary>
        /// 取HashSet全部记录
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Dictionary</returns>
        Dictionary<string, T> GetHashAll<T>(string key);

        /// <summary>
        /// 移除一个指定的key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>bool</returns>
        bool RemoveKey(string key);

        /// <summary>
        /// 移除HashKey
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <returns>bool</returns>
        bool RemoveHashKey(string key, string filed);

        /// <summary>
        /// 查找是否包含key的值
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>bool</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// 给Hash的某一个filed 增加 value 个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <param name="value">value</param>
        void HashIncrement(string key, string filed, long value);

        /// <summary>
        /// 给Hash的某一个filed 减去 value 个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <param name="value">value</param>
        void HashDecrement(string key, string filed, long value);

        /// <summary>
        /// 给一个字符串增加 value个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        void StringIncrement(string key, long value);

        /// <summary>
        /// 给一个字符串减去 value个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        void StringDecrement(string key, long value);

        /// <summary>
        /// Redis是单进程单线程，不存在并发问题，因此只要保证多条命令原则执行即可
        /// </summary>
        /// <param name="key">队列名称</param>
        /// <param name="value">值</param>
        /// <param name="queueSize">队列长度</param>
        /// <returns>是否</returns>
        bool PushQueue(string key, string value,int queueSize = 1000);

        /// <summary>
        /// 弹出 最大的日志信息
        /// </summary>
        /// <param name="key">队列名称</param>
        /// <returns>最大数量日志</returns>
        List<string> PopAllQueue(string key);

        /// <summary>
        /// 默认超时 ：1分钟
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        bool LockTake(string key, string value, TimeSpan? expiry = null);
        

        /// <summary>
        /// 查询锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        string LockQuery(string key, string value);


        /// <summary>
        /// 释放锁 ，值必须对才可以释放
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool LockRelease(string key, string value);

        /// <summary>
        /// 设置指定key的过期时间
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        bool KeyExpire(string key, TimeSpan? expiry = null);
    }
}
