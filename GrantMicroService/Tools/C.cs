using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using GrantMicroService.Cache;

namespace GrantMicroService.Tools
{
    /// <summary>
    /// 简写类, 调用资源文件
    /// </summary>
    public static class C
    {
        /// <summary>
        /// 方便用户直接使用C.R() 来获取资源文件,更少的参数，可以采用 RpcContext.R("key")
        /// </summary>
        /// <param name="resourceName">资源key</param>
        /// <param name="lang">语言种类</param>
        /// <returns>资源文字</returns>
        public static string R(string resourceName, string lang)
        {
            string field = CacheTools.GetPublicResourceKey(lang);
            var res = ResourceCache.Instance.GetHash<string>(field, resourceName);
            return string.IsNullOrEmpty(res) ? resourceName : res; // 没有取到资源文件把key原样返回;
            // return ResourceCache.Instance.GetHash<string>(resourceName, $"LocalResource.{subSystem}.{lang}");
        }
    }
}