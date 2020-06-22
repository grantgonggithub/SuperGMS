using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SuperGMS.JsonEx
{
    /// <summary>
    /// IXmlSerializable类型的Json序列化
    /// </summary>
    public class IXmlSerializableConverter : JsonConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            //只能序列化IXmlSerializable的实现类
            return typeof(IXmlSerializable).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //只能反序列化IXmlSerializable的实现类
            if (typeof(IXmlSerializable).IsAssignableFrom(objectType))
            {
                if (reader.Value == null)
                    return null;
                string xmlStr = reader.Value.ToString();
                using (var strReader = new StringReader(xmlStr))
                {
                    using (var xmlReader = XmlReader.Create(strReader))
                    {
                        var xmlSerializer = new XmlSerializer(objectType);
                        var obj = xmlSerializer.Deserialize(xmlReader);
                        return obj;
                    }
                }
            }
            else
                throw new ArgumentOutOfRangeException("只能反序列化IXmlSerializable的实现类");
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IXmlSerializable)
            {
                string xml = null;
                if (value == null)
                    writer.WriteValue(xml);
                else
                {
                    StringBuilder sb = new StringBuilder();
                    using (var xmlWriter = XmlWriter.Create(sb))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(value.GetType());
                        xmlSerializer.Serialize(xmlWriter, value);
                        xmlWriter.Close();
                        writer.WriteValue(sb.ToString());
                    }
                }
            }
            else
                throw new ArgumentOutOfRangeException("只能序列化IXmlSerializable的实现类");
        }
    }
    
}
