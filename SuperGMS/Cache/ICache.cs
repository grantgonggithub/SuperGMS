/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Cache
 文件名：  ICache
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/23 17:28:33

 功能描述：

----------------------------------------------------------------*/
using SuperGMS.Redis;

using System;
using System.Collections.Generic;

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
        /// <returns>递增后的值</returns>
        long HashIncrement(string key, string filed, long value);

        /// <summary>
        /// 给Hash的某一个filed 减去 value 个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <param name="value">value</param>
        /// <returns>递减后的值</returns>
        long HashDecrement(string key, string filed, long value);

        /// <summary>
        /// 给一个字符串增加 value个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>递增后的值</returns>
        long StringIncrement(string key, long value);

        /// <summary>
        /// 给一个字符串减去 value个计数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>递减后的值</returns>
        long StringDecrement(string key, long value);

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

        /// <summary>
        /// 获取符合指定匹配条件的所有keys，为了保证性能一次最多只能提取1000条
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="matchPaten">要匹配key的关键字表达式</param>
        /// <param name="pageSize">一次提取的数量，最大1000</param>
        /// <param name="pageIndex">取页数</param>
        /// <returns></returns>
        IEnumerable<string> GetAllKeys(string matchPaten, int pageSize = 250, int pageIndex = 0);


        /// <summary>
        /// 消息队列生产者发布消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="QueueKey">消息分类key,根据这个key接收消息</param>
        /// <param name="obj">消息对象</param>
        /// <param name="MaxLength">队列最大长度，超过则丢弃最早的消息</param>
        /// <param name="UseApproximateMaxLength"></param>
        /// <returns>新增消息的messageId</returns>
        string StreamAdd<T>(string QueueKey, T obj, int MaxLength, bool UseApproximateMaxLength = true);

        /// <summary>
        /// 注册一个消费者，消息组，防止不同客户端重复接收消息
        /// </summary>
        /// <param name="QueueKey">消息key</param>
        /// <param name="GroupName">消息分组，注意：相同分组的多个客户端不会重复接收到同一个消息</param>
        /// <returns></returns>
        bool CreateConsumerGroup(string QueueKey, string GroupName);

        /// <summary>
        /// 消息获取
        /// </summary>
        /// <typeparam name="T">获取的对象，根据生产者来确定</typeparam>
        /// <param name="QueueKey">消息key，根据这个来获取消息，根据生产者来确定</param>
        /// <param name="GroupName">消息分组，防止重复</param>
        /// <param name="ConsumerName"></param>
        /// <param name="Count">获取消息数量</param>
        /// <returns></returns>
        List<T> StreamReadGroup<T>(string QueueKey, string GroupName, string ConsumerName, int Count) where T : RedisStreamEntriy;

        /// <summary>
        /// 消费完消息以后，ACK消息
        /// </summary>
        /// <param name="QueueKey"></param>
        /// <param name="GroupName"></param>
        /// <param name="ids"></param>
        /// <returns>ACK消息的数量</returns>
        long StreamAck(string QueueKey, string GroupName, string[] ids);

        /// <summary>
        /// 获取指定数量的消息，包括ACK和未ACK的，从最新的往后取指定数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="QueueKey"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        List<T> StreamRange<T>(string QueueKey, int Count) where T : RedisStreamEntriy;

        /// <summary>
        /// 获取指定消费组的已经获取的消息，但是未ACK的消息（Padding队列的消息）到指定的消费客户端，注意：处理完业务需要ACK
        /// 用于消息补偿（一个客户端获取了消息，但是崩溃了，未进行ACK的场景）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="QueueKey"></param>
        /// <param name="GroupName"></param>
        /// <param name="ClaimConsumerName"></param>
        /// <param name="MinIdleTime"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        List<T> StreamAutoClaim<T>(string QueueKey, string GroupName, string ClaimConsumerName, long MinIdleTime, int Count) where T : RedisStreamEntriy;

        /// <summary>
        /// 按照消息Id删除消息
        /// </summary>
        /// <param name="QueueKey">rediskey</param>
        /// <param name="msgIds">消息id列表</param>
        /// <returns>删除成功数量</returns>
        long StreamDelete(string QueueKey, string[] msgIds);
    }
}
