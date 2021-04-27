using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SuperGMS.ApiHelper
{
    /// <summary>
    /// json例子替换
    /// </summary>
    internal class JsonSampleReplace
    {
        private List<KeyValuePair<string, string>> propList;
        private string jsonObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSampleReplace"/> class.
        /// </summary>
        /// <param name="propList">键值对集合</param>
        /// <param name="jsonObject">json字符串</param>
        public JsonSampleReplace(List<KeyValuePair<string, string>> propList, string jsonObject)
        {
            this.propList = propList;
            this.jsonObject = jsonObject;
        }

        /// <summary>
        /// 格式化json
        /// </summary>
        /// <returns>全部类型格式化为字符串</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handling the failure by returning the original string.")]
        public string TryFormatJson()
        {
            if (string.IsNullOrEmpty(jsonObject))
            {
                return string.Empty;
            }

            try
            {
                var tempStr = TryReplaceFormatJson(jsonObject);

                var parsedJson = JsonConvert.DeserializeObject(tempStr);
                tempStr = JsonConvert.SerializeObject(
                    parsedJson,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    });
                return tempStr;
            }
            catch (Exception ex)
            {
                // SuperGMS.Log.GrantLogTextWriter.Write(ex);
                return jsonObject;
            }
        }

        /// <summary>
        /// 对jsobject进行替换
        /// </summary>
        /// <param name="str">json字符串</param>
        /// <returns>替换后字符串</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handling the failure by returning the original string.")]
        private string TryReplaceFormatJson(string str)
        {
            JObject jObect = JObject.Parse(str);
            SetJsonTimeOrDecmail(jObect);
            return JsonConvert.SerializeObject(jObect);
        }

        /// <summary>
        /// 设置或替换JSON中值
        /// </summary>
        /// <param name="obj">JSON对象</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handling the failure by returning the original string.")]
        private void SetJsonTimeOrDecmail(JObject obj)
        {
            foreach (var item in obj.Children())
            {
                if (item is JObject)
                {
                    SetJsonTimeOrDecmail((JObject)item);
                }
                else if (item is JProperty)
                {
                    JProperty jProperty = (JProperty)item;
                    SetValue(jProperty);
                    if (jProperty.Value.GetType() == typeof(JObject))
                    {
                        SetJsonTimeOrDecmail((JObject)jProperty.Value);
                    }
                    else if (jProperty.Value.GetType() == typeof(JArray))
                    {
                        try
                        {
                            var jArraies = ((JArray)jProperty.Value).AsJEnumerable();
                            foreach (var jToken in jArraies)
                            {
                                SetJsonTimeOrDecmail((JObject)jToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            //SuperGMS.Log.GrantLogTextWriter.Write(ex);
                            var jArraies = ((JArray)jProperty.Value).GetEnumerator();
                            var jProp = (JProperty)jArraies;
                            if (jProp?.Value != null)
                            {
                                JObject js = (JObject)jProp.Value;
                                SetJsonTimeOrDecmail(js);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 设置属性值为字符串
        /// </summary>
        /// <param name="jp">属性</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handling the failure by returning the original string.")]
        private void SetValue(JProperty jp)
        {
            DateTime time;
            decimal decm;
            bool bValue;
            string value = jp.Value.ToString();
            if (DateTime.TryParse(value, out time))
            {
                SetJProperty(jp);
            }
            else if (decimal.TryParse(value, out decm))
            {
                SetJProperty(jp);
            }
            else if (bool.TryParse(value, out bValue))
            {
                SetJProperty(jp);
            }
            else
                SetJProperty(jp);
        }

        private void SetJProperty(JProperty jp)
        {
            if (propList?.Count > 0)
            {
                if (propList.Any(x => x.Key == jp.Name))
                {
                    var keyPair = propList.First(x => x.Key == jp.Name);
                    jp.Value = keyPair.Value;
                    propList.Remove(keyPair);
                }
            }
        }
    }
}