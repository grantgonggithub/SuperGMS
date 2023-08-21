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
            object jObject = null;
            try
            {
                jObject = JObject.Parse(str);
            }
            catch { jObject = null; }
            if (jObject == null)
                try
                {
                    jObject = JArray.Parse(str);
                }
                catch { jObject = null; }
            if (jObject == null)
                jObject = JToken.Parse(str);
            SetJsonTimeOrDecmail(jObject);
            return JsonConvert.SerializeObject(jObject);
        }

        /// <summary>
        /// 设置或替换JSON中值
        /// </summary>
        /// <param name="obj">JSON对象</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handling the failure by returning the original string.")]
        private void SetJsonTimeOrDecmail(object obj)
        {
            IJEnumerable<JToken> tokens = null;
            if (obj is JObject)
            {
                var tmpObject = obj as JObject;
                if (tmpObject != null)
                    tokens = tmpObject.Children();
            }
            else if (obj is JArray)
            {
                var tmpArray = obj as JArray;
                if (tmpArray != null)
                    tokens = tmpArray.AsJEnumerable();
            }
            else
            {
                var tmpArray = obj as JToken;
                if (tmpArray != null)
                    tokens = tmpArray.Children();
            }

            foreach (var item in tokens)
            {
                if (item is JObject)
                {
                    SetJsonTimeOrDecmail((JObject)item);
                }
                else if (item is JProperty)
                {
                    JProperty jProperty = (JProperty)item;
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
                                if(jToken is JObject)
                                    SetJsonTimeOrDecmail((JObject)jToken);
                                else
                                    SetJProperty(jProperty);
                            }
                        }
                        catch (Exception ex)
                        {
                            //SuperGMS.Log.GrantLogTextWriter.Write(ex);
                            var jArraies = ((JArray)jProperty.Value).GetEnumerator();
                            var jProp = (JProperty)jArraies;
                            if (jProp?.Value != null)
                            {
                                if (jProp.Value is JObject)
                                {
                                    JObject js = (JObject)jProp.Value;
                                    SetJsonTimeOrDecmail(js);
                                }
                                else
                                    SetJProperty(jProp);
                            }
                        }
                    }
                    else
                        SetValue(jProperty);
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
                    //propList.Remove(keyPair);
                }
            }
        }
    }
}