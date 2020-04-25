using System;
using System.Collections.Generic;
using System.Text;
using GrantMicroService.Extensions.Enum;

namespace GrantMicroService.Cache
{
    /// <summary>
    /// 资源缓存名称生成器
    /// </summary>
    public static class CacheTools
    {
        /// <summary>
        /// 返回指定语言的基础资源key
        /// </summary>
        /// <param name="lang">语言</param>
        /// <returns>基础资源key</returns>
        public static string GetPublicResourceKey(string lang)
        {
            return GetResourceFiledPrefix(HashKeyType.LocalResource, lang?.ToLower());
        }

        /// <summary>
        /// 返回指定ttid，语言的定制化资源key
        /// </summary>
        /// <param name="ttid">域</param>
        /// <param name="lang">语言</param>
        /// <returns>定制化资源key</returns>
        public static string GetUdfResourceKey(string ttid, string lang)
        {
            return GetResourceFiledPrefix(HashKeyType.TtResource, $"{ttid}.{lang}".ToLower());
        }

        /// <summary>
        /// 返回指定ttid，语言的定制化资源key
        /// </summary>
        /// <param name="ttidLang">域</param>
        /// <returns>定制化资源key</returns>
        public static string GetUdfResourceKey(string ttidLang)
        {
            return GetResourceFiledPrefix(HashKeyType.TtResource, ttidLang?.ToLower());
        }


        /// <summary>
        /// 返回定制化资源 ttids集合 Key
        /// </summary>
        /// <returns>ttids集合 Key</returns>
        public static string GetUdfTtidsKey()
        {
            return GetResourceFiledPrefix(HashKeyType.TtResource, "ttids");
        }

        /// <summary>
        /// 返回资源语言种类Key
        /// </summary>
        /// <returns>ttids集合 Key</returns>
        public static string GetLangsKey()
        {
            return GetResourceFiledPrefix(HashKeyType.LocalResource, "langs");
        }

        /// <summary>
        /// 获取资源文件 HashKeyFiled , 一种是默认级别, 一种是租户级别
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="type">类型</param>
        /// <param name="field">field</param>
        /// <returns>资源文件的前缀</returns>
        private static string GetResourceFiledPrefix(HashKeyType type, string field)
        {
            switch (type)
            {
                case HashKeyType.LocalResource:
                    return $"LocalResource:public:{field}";
                case HashKeyType.TtResource:
                    return $"LocalResource:udf:{field}";

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
