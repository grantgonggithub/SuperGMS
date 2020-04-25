using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrantMicroService.Cache;

namespace GrantMicroService.Audit
{
    /// <summary>
    /// 简单全局二级缓存
    /// </summary>
    internal class GlobalCache
    {
        /// <summary>
        /// 全局词典
        /// </summary>
        private static  ConcurrentDictionary<string, object> _dict = new ConcurrentDictionary<string, object>();
        /// <summary>
        /// 返回缓存值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key) where T : class
        {
            object obj ;
            if (_dict.TryGetValue(key, out obj))
            {
                var cc = (CacheClass<T>)obj;
                if (cc.IsExpire())
                {
                    _dict.TryRemove(key, out obj);
                }
                else
                {
                    return cc.Value;
                }
            }

            //从redis中取值
            var val = CacheManager.Get<CacheClass<T>>(key) ;
            if (val != null)
            {
                _dict[key] = val;
            }
            else
            {
                _dict.TryRemove(key,out obj);
            }
            return val?.Value;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expireDate"></param>
        public static void Set<T>(string key, T obj, DateTime? expireDate = null) where T : class
        {
            var cc = new CacheClass<T>(obj, expireDate);
            if (!cc.IsExpire())
            {
                _dict[key]= cc;
                CacheManager.Set<CacheClass<T>>(key, cc, cc.ExpireDate-DateTime.Now);
            }
        }
    }

    /// <summary>
    /// 带过期时间的缓存类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheClass<T> where T:class
    {
        /// <summary>
        /// 缓存值
        /// </summary>
        public T Value { private set; get; }

        /// <summary>
        /// 绝对过期时间
        /// </summary>
        public DateTime ExpireDate {private set;get;}

        /// <summary>
        /// 判断是否过期,true-过期
        /// </summary>
        /// <returns></returns>
        public bool IsExpire()
        {
            return (ExpireDate < DateTime.Now);
        }
       
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="expireDate">绝对过期时间</param>
        public CacheClass(T obj, DateTime? expireDate = null)
        {
            this.Value = obj;
            if (!expireDate.HasValue)
            {
                ExpireDate = DateTime.Now + new TimeSpan(0, 10, 0);
            }
            else
            {
                ExpireDate = expireDate.Value;
            }
        }
    }
}
