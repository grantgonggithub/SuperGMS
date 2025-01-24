//
// 文件：JsonConvert.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29
// 最后更新日期：2017-11-14 11:09

#region

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using SuperGMS.ExceptionEx;
using SuperGMS.Log;

using System;
using System.Text.RegularExpressions;

#endregion

namespace SuperGMS.JsonEx
{

    /// <summary>
    /// Json序列化，使用Newtonsoft 提供的序列化方法
    /// </summary>
    public class JsonConvert
    {
        /// <summary>
        /// log
        /// </summary>
        private static readonly ILogger logger = LogFactory.CreateLogger<JsonConvert>();

        /// <summary>
        /// 将字符串反序列化为指定的对象
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T">指定的对象</typeparam>
        /// <returns></returns>
        public static T DeserializeObject<T>(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "反序列化JSON失败");
                    throw FrameworkException.CreateNew($"反序列化JSON失败,值:{value}");
                }
            }
            return default(T);
        }

        /// <summary>
        /// Copy 序列化一个新对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="t">需要Copy的对象</param>
        /// <returns>新对象</returns>
        public static T CopyObject<T>(T t)
        {
            return DeserializeObject<T>(JsonSerializer(t));
        }

        /// <summary>
        /// 将字符串反序列化为指定的对象
        /// </summary>
        /// <param name="value"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object DeserializeObject(string value, Type t)
        {
            //保持与上面函数功能一致
            if (string.IsNullOrEmpty(value))
            {
                return t.IsValueType ? Activator.CreateInstance(t) : null;
            }

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject(value, t);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "反序列化JSON失败");
                throw FrameworkException.CreateNew($"反序列化JSON失败,值:{value}");
            }
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="xml">xml文档</param>
        /// <returns></returns>
        public static T XmlToDeserializeObject<T>(string xml)
        {
            try
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(xml);
                if (doc.DocumentElement != null)
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "反序列化XML失败");
                throw FrameworkException.CreateNew($"反序列化XML失败,值:{xml}", ex);
            }
            return default(T);
        }

        /// <summary>
        /// 将对象序列化为字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="removeEmptyProp">将null属性,0.0属性,0001-01-01 00:00:00 属性的值替换为空</param>
        /// <returns></returns>
        public static string JsonSerializer(object obj, bool removeEmptyProp = false)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss" };
            var value = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Formatting.None, timeConverter);
            if (removeEmptyProp)
            {
                Regex removeEmpty = new Regex("\"[^\"]*?\":null|\"[^\"]*?\":\"0001-01-01 00:00:00\"|\"[^\"]*?\":0.0", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                value = removeEmpty.Replace(value, "");
                Regex removeMoreComma = new Regex("[,]+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                value = removeMoreComma.Replace(value, ",");
                Regex rl = new Regex("{[,]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                value = rl.Replace(value, "{");
                Regex rr = new Regex("[,]}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                value = rr.Replace(value, "}");
            }
            return value;
        }
    }
}