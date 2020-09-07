using System;
using System.Collections.Generic;
using System.Text;
using SuperGMS.Cache;
using SuperGMS.Config;
using SuperGMS.ExceptionEx;
using SuperGMS.Extensions.Enum;
using SuperGMS.Rpc.Server;

namespace SuperGMS.Extensions
{
    /// <summary>
    /// 获取资源信息
    /// </summary>
    public static class RpcContextExtensions
    {
        /// <summary>
        /// 得到指定key的资源信息,默认按照 微服务名称获取相关信息
        ///
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="key">key</param>
        /// <returns>资源</returns>
        public static string R(this RpcContext context, string key)
        {
            key = key.Trim();
            var user = context.GetUserContext();

            string field = CacheTools.GetPublicResourceKey(context.Language);

            string res = string.Empty;
            if (user == null)
            {
                res = ResourceCache.Instance.GetHash<string>(field, key);
            }
            else
            {
                // 从Redis先获取 租户级别的Key,如果没有
                var ttField = CacheTools.GetUdfResourceKey(user.UserInfo.TenantInfo.TTID.ToString(), context.Language);
                res = ResourceCache.Instance.GetHash<string>(ttField, key);
                if (res == null)
                {
                    // 否则拉初始资源到租户级别Key中
                    var defaultRes = ResourceCache.Instance.GetHash<string>(field, key);

                    // 避免null 值建立多个key
                    if (defaultRes != null)
                    {
                        ResourceCache.Instance.SetHash<string>(ttField, new KeyValuePair<string, string>(key, defaultRes));
                        var keyTtids = CacheTools.GetUdfTtidsKey();
                        ResourceCache.Instance.SetHash<string>(keyTtids, new KeyValuePair<string, string>($"{user.UserInfo.TenantInfo.TTID}.{context.Language}", string.Empty));
                        res = defaultRes;
                    }
                }
            }

            return string.IsNullOrEmpty(res) ? key : res; // 没有取到资源文件把key原样返回;
        }
    }
}