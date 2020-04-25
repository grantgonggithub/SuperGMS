/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.JsonEx
 文件名：  GrantDateTimeJsonConvert
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/16 13:34:51

 功能描述：

        [DefaultValue(typeof(DateTime), GrantDateTimeJsonConvert.DefaultDateTimeValueString)] // 设置默认值为1900-1-1,用于反序列化赋值
        [JsonConverter(typeof(GrantDateTimeJsonConvert))] // 用于序列化时将1900-1-1设置为空字符串
        public DateTime Date
        {
            get;
            set;
        }

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GrantMicroService.JsonEx
{
    /// <summary>
    /// GrantDateTimeJsonConvert
    /// </summary>
    public class GrantDateTimeJsonConvert : DateTimeConverterBase
    {
        public const string DefaultDateTimeValueString = "1900-1-1";
        public static readonly DateTime DefaultDateTimeValue = DateTime.Parse(DefaultDateTimeValueString);

        /// <summary>
        /// 读Json
        /// </summary>
        /// <param name="reader">reader</param>
        /// <param name="objectType">objectType</param>
        /// <param name="existingValue">existingValue</param>
        /// <param name="serializer">serializer</param>
        /// <returns>object</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (objectType==typeof(Nullable<DateTime>) || objectType.Name == typeof(DateTime).Name)
            {
                DateTime t;
                object cValue = reader.Value;

                // 兼容客户端时间提交的 null 和 ""字符串
                if (cValue == null || cValue.ToString() == string.Empty)
                {
                    t = DefaultDateTimeValue;
                }
                else if (!DateTime.TryParse(reader.Value?.ToString(), out t))
                {
                    throw new ArgumentOutOfRangeException("时间格式错误，请检查此传入的时间字段的字符串格式是否正确");
                }

                if (t <= DefaultDateTimeValue)
                {
                    return DefaultDateTimeValue;
                }
                else
                {
                    return t;
                }
            }

            throw new ArgumentOutOfRangeException("时间格式错误，请检查此传入的时间字段的字符串格式是否正确");
        }

        /// <summary>
        /// 写Json
        /// </summary>
        /// <param name="writer">writer</param>
        /// <param name="value">value</param>
        /// <param name="serializer">serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                // throw new NotImplementedException();
                DateTime dateTime = (DateTime) value;
                if (dateTime <= DefaultDateTimeValue)
                {
                    writer.WriteValue(string.Empty);
                }
                else
                {
                    writer.WriteValue(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("时间格式错误，请检查此属性是否标记在合适的字段上");
            }
        }
    }
}
