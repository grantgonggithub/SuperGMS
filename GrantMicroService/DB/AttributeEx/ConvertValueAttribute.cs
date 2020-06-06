using GrantMicroService.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GrantMicroService.DB.AttributeEx
{
    /// <summary>
    /// 转换值属性
    /// </summary>
    public class ConvertValueAttribute : Attribute
    {
        /// <summary>
        /// 动态转换,实现了ICovertValue接口的类型
        /// </summary>
        private readonly Type _t;

        /// <summary>
        /// 动态转换,调用iCovertValue时的参数
        /// </summary>
        private readonly string _invokeParams;
        /// <summary>
        /// 是否通过资源文件转换
        /// </summary>
        private readonly bool _isResource = true;
        /// <summary>
        /// 静态转换,常量类型转换值
        /// </summary>
        private readonly string _values;

        /// <summary>
        /// 字典类型,记录转换的键值对,键对应Value,值对应Name
        /// </summary>
        private Dictionary<string, string> ConvertDic
        {
            get
            {
                throw new NotImplementedException();
                //var cacheKey = _values + "ConvertValueAttribute" + (this._t != null ? this._t.FullName + this._invokeParams : "");

                ////同一次http请求,会将ConvertDic缓存起来
                ////if (AsyncLocal<Dictionary<string, string>>.GetData(cacheKey) != null)
                ////{
                ////    return CallContext.GetData(cacheKey) as Dictionary<string, string>;
                ////}

                ////先判断静态转换
                //if (!string.IsNullOrEmpty(this._values))
                //{
                //    var returnValue = this._values.Split(',').Select(value => value.Split(':')).ToDictionary(arrayValue => arrayValue[0].Trim(), arrayValue => arrayValue[1].Trim());
                //    CallContext.SetData(cacheKey, returnValue);
                //    return returnValue;
                //}

                ////再使用动态转换
                //var obj = Activator.CreateInstance(this._t) as IConvertValue;
                //if (obj != null)
                //{
                //    var returnValue = obj.GetConvertDictionary(this._invokeParams);
                //    CallContext.SetData(cacheKey, returnValue);
                //    return returnValue;
                //}

                ////再使用动态转换
                //var objWMS = Activator.CreateInstance(this._t) as IConvertWMSValue;
                //if (objWMS != null)
                //{
                //    var returnValue = objWMS.GetConvertDictionary(this._invokeParams);
                //    CallContext.SetData(cacheKey, returnValue);
                //    return returnValue;
                //}

                ////都未转换成功,则抛出异常
                //throw new Exception("初始化ConvertValueAttribute失败,参数类型t 没有继承IConvertValue接口");
            }
        }


        public ConvertValueAttribute(Type t, string invokeParams, bool isResource = true)
        {
            this._invokeParams = invokeParams;
            this._isResource = isResource;
            this._t = t;
        }
        /// <summary>
        /// 将转换值以 "OPEN:开放,CONFIRM:审核" 传入
        /// </summary>
        /// <param name="values"></param>
        public ConvertValueAttribute(string values)
        {
            this._values = values;
        }

        /// <summary>
        /// 根据键值获取Name
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetName(string key)
        {
            key = key.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                if (key.Contains("\n") || key.Contains(","))
                {
                    char[] splitchar = key.Contains(",") ? new char[] { ',' } : new char[] { '\n' };
                    var keys = key.Split(splitchar);
                    var listValue = new List<string>();
                    foreach (var k in keys)
                    {
                        if (this.GetKeyNameDictionary().ContainsKey(k))
                        {
                            listValue.Add(this.GetKeyNameDictionary()[k]);
                        }
                    }
                    return string.Join(",", listValue.ToArray());
                }
                else
                {
                    if (this.GetKeyNameDictionary().ContainsKey(key))
                        return this.GetKeyNameDictionary()[key];
                    else
                        return key;
                }
            }
            return key;
        }

        /// <summary>
        /// 获取一个键值对的字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetKeyNameDictionary()
        {
            if (ConvertDic != null)
            {
                if (this._isResource)
                {
                    return ConvertDic.ToDictionary(a => a.Key, a => a.Value);
                }
                return ConvertDic.ToDictionary(a => a.Key, a => a.Value);
            }
            return new Dictionary<string, string>();
        }
    }
}
