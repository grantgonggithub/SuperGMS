/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Redis
 文件名：RedisProxy
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/3/24 14:51:12

 功能描述：提供给上层使用的redis封装，
           不用关心底层的负载模式，会根据配置自动实现负载

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;

using StackExchange.Redis;
using Newtonsoft.Json;
using SuperGMS.Log;
using Microsoft.Extensions.Logging;

namespace SuperGMS.Redis
{
    /// <summary>
    /// 访问redis的一个本地代理
    /// </summary>
   public partial class RedisProxy
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<RedisProxy>();
        /// <summary>
        /// 是哦夫存在节点配置
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public static bool IsExistCfg(string nodeName)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            return cfg != null;
        }

        /// <summary>
        /// 设置指定key的过期时间
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="logicName">业务名称</param>
        /// <param name="key">key</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public static bool KeyExpire(string nodeName, string logicName, string key, TimeSpan? expiry)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Set(db => db.KeyExpire(k,expiry));
        }

        /// <summary>
        /// 保存一个string value
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="logicName">业务名称</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>bool</returns>
        public static bool Set(string nodeName, string logicName, string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Set(db => db.StringSet(k, value, expiry));
        }

        /// <summary>
        /// 存储一个key value的HashSet
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="logicName">业务名称</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public static void SetHash<T>(string nodeName, string logicName,string key, KeyValuePair<string, T> value)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            HashEntry[] hash = new HashEntry[] { new HashEntry(value.Key, ConvertJson(value.Value)) };
            cfg.Set(db =>
            {
                db.HashSet(k, hash);
                return true;
            });
        }

        /// <summary>
        /// 给一个指定的key字符串，增加value的计数
        /// </summary>
        /// <param name="nodeName">nodeName</param>
        /// <param name="logicName">logicName</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public static void StringIncrement(string nodeName, string logicName, string key, long value)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            cfg.Set(db => { db.StringIncrement(k, value);
                return true;
            });
        }

        /// <summary>
        /// 给一个指定的key字符串，减去value的计数
        /// </summary>
        /// <param name="nodeName">nodeName</param>
        /// <param name="logicName">logicName</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public static void StringDecrement(string nodeName, string logicName, string key, long value)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            cfg.Set(db =>
            {
                db.StringDecrement(k, value);
                return true;
            });
        }

        /// <summary>
        /// 给一个指定的key的某一个filed增加value个计数
        /// </summary>
        /// <param name="nodeName">nodeName</param>
        /// <param name="logicName">logicName</param>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <param name="value">value</param>
        public static void HashIncrement(string nodeName, string logicName, string key,string filed, long value)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            cfg.Set(db =>
            {
                db.HashIncrement(k, filed, value);
                return true;
            });
        }

        /// <summary>
        /// 给一个指定的key的某一个filed减去value个计数
        /// </summary>
        /// <param name="nodeName">nodeName</param>
        /// <param name="logicName">logicName</param>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <param name="value">value</param>
        public static void HashDecrement(string nodeName, string logicName, string key, string filed, long value)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            cfg.Set(db =>
            {
                db.HashDecrement(k, filed, value);
                return true;
            });
        }


        /// <summary>
        /// 获取Hash字段值
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="nodeName">nodeName</param>
        /// <param name="logicName">logicName</param>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        /// <returns>TT</returns>
        public static T GetHash<T>(string nodeName, string logicName, string key, string filed)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            RedisValue v = cfg.Get<RedisValue>(db =>
            {
              return db.HashGet(k,filed);
            });
            return ConvertValue<T>(v);
        }

        /// <summary>
        /// 获取整个Hash表
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="nodeName">nodeName</param>
        /// <param name="logicName">logicName</param>
        /// <param name="key">key</param>
        /// <returns>static</returns>
        public static Dictionary<string, T> GetHashAll<T>(string nodeName, string logicName, string key)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            HashEntry[] vs = cfg.Get<HashEntry[]>(db =>
            {
                return db.HashGetAll(k);
            });
            if (vs == null || vs.Length < 1)
            {
                return null;
            }

            return vs.ToDictionary(a => a.Name.ToString(), a => ConvertValue<T>(a.Value));
        }

        /// <summary>
        /// 移除指定key值
        /// </summary>
        /// <param name="nodeName">nodeName</param>
        /// <param name="logicName">logicName</param>
        /// <param name="key">key</param>
        /// <param name="filed">filed</param>
        public static void RemoveHashKey(string nodeName, string logicName, string key,string filed)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            cfg.Set(db => { return db.HashDelete(k, filed); });
        }

        /// <summary>
        /// 这个方法最好不要用，因为你在外部构建一个IDictionary需要循环一次，
        /// 扔里面来里面再循环一次，做了没有意义的事情，所以在外面就直接调用KeyValuePair的参数吧
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="logicName">业务名称</param>
        /// <param name="key">key</param>
        /// <param name="values">values</param>
        public static void SetHash<T>(string nodeName, string logicName, string key, IDictionary<string, T> values)
        {
            if (values == null || values.Count < 1)
            {
                return;
            }

            //批量入库，一次写，速度更快
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            cfg.Set(db =>
            {
                db.HashSet(k, values.Select(x => new HashEntry(x.Key, ConvertJson(x.Value))).ToArray());
                return true;
            });
            //foreach (KeyValuePair<string, T> kv in values)
            //{
            //    SetHash(nodeName, logicName, key, kv);
            //}
        }

        /// <summary>
        /// 存放key为Obj的对象到模型nodeName下面
        /// </summary>
        /// <typeparam name="T">要存储的对象Type</typeparam>
        /// <param name="nodeName">节点名称</param>
        /// <param name="logicName">业务名称</param>
        /// <param name="key">key</param>
        /// <param name="obj">要存储的对象</param>
        /// <param name="expiry">过期时间，一般不指定，使用默认</param>
        /// <returns>bool</returns>
        public static bool Set<T>(string nodeName,string logicName,string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            string json = ConvertJson(obj);
            return cfg.Set(db => db.StringSet(k, json, expiry));
        }

        /// <summary>
        /// 获取字符串对象
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="logicName">业务名称</param>
        /// <param name="key">key</param>
        /// <returns>string</returns>
        public static string Get(string nodeName,string logicName, string key)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Get<string>(db => db.StringGet(k));
        }

        /// <summary>
        /// 获取某个模型下面的特定key的值
        /// </summary>
        /// <typeparam name="T">存储对象</typeparam>
        /// <param name="nodeName">节点名称</param>
        /// <param name="logicName">业务名称</param>
        /// <param name="key">key</param>
        /// <returns>T</returns>
        public static T Get<T>(string nodeName, string logicName, string key)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Get<T>(db =>
            {
                RedisValue rv = db.StringGet(k);
                return ConvertValue<T>(rv);
            });
        }

        /// <summary>
        /// 移除某个key的值
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="logicName">业务名称</param>
        /// <param name="key">key</param>
        /// <returns>bool</returns>
        public static bool RemoveKey(string nodeName, string logicName, string key)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Set(db => db.KeyDelete(k));
        }

        public static byte[] DumpKey(string nodeName, string logicName, string key)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Get(db =>
            {
                return db.KeyDump(k);               
            });            
        }
        public static bool RestoreKey(string nodeName, string logicName, string key, byte[] obj, TimeSpan? expiry = default(TimeSpan?))
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Set(db => {
                db.KeyRestore(k, obj, expiry);
                return true;
                });
        }

        /// <summary>
        /// 清除某个业务模型下面的所有节点
        /// </summary>
        /// <param name="nodeName">业务模型名称</param>
        /// <returns>bool</returns>
        // public static bool FlushAll(string nodeName,string logicName)
        // {
        //    RedisNode cfg = RedisConfig.GetNode(nodeName);
        //    return cfg.Set(db => db.KeyDelete(nodeName));
        // }

        /// <summary>
        /// 查询指定的key是否存在
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="logicName">logicName</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ContainsKey(string nodeName,string logicName, string key)
        {
            RedisNode cfg = RedisConfig.GetNode(nodeName);
            string k = getKey(logicName, key);
            return cfg.Get<bool>(db => db.KeyExists(k));
        }

        /// <summary>
        /// 获取符合指定匹配条件的所有keys，为了保证性能一次最多只能提取1000条
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="matchPaten">要匹配key的关键字表达式</param>
        /// <param name="pageSize">一次提取的数量，最大1000</param>
        /// <param name="pageIndex">取页数</param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllKeys(string nodeName,string matchPaten,int pageSize=250,int pageIndex=0) {
            RedisNode cfg=  RedisConfig.GetNode(nodeName);
            if(pageSize>1000) pageSize = 1000;
            return cfg.Get<IEnumerable<string>>((db, server) =>
            {
                return server.Keys(cfg.IsMasterSlave? cfg.MasterServer.DbIndex : cfg.SlaveServers[0].DbIndex, matchPaten, pageSize,0,pageIndex).Select(k => k.ToString());
            },true);
        }



        private static string getKey(string logicName, string orgKey)
        {
            return string.Format("{0}:{1}",logicName, orgKey);
        }

        private static T[] ConvetValueList<T>(RedisValue[] values)
        {
            List<T> result = new List<T>();
            foreach (var item in values)
            {
                var model = ConvertValue<T>(item);
                result.Add(model);
            }

            return result.ToArray();
        }

        private static T ConvertValue<T>(RedisValue value)
        {
            if (!value.HasValue || value.IsNullOrEmpty)
            {
                return default(T);
            }

            try
            {
                if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception e)
            {
                logger.LogError(e, "RedisProxy.ConvertValue<T>.Error.OrgString=" + value);
            }

            return default(T);
        }

        private static string ConvertJson(object value)
        {
            string result = value is string ? value.ToString() : JsonConvert.SerializeObject(value,new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            return result;
        }
    }
}
