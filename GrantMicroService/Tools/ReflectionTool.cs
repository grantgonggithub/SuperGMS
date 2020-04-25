//
// 文件：ReflectionTool.cs
// 作者：Grant
// 最后更新日期：2014-09-04 00:00
using System;
using System.Linq;
using System.Globalization;
using System.Reflection;

namespace GrantMicroService.Tools
{
    using GrantMicroService.DB.AttributeEx;
    using System.Collections.Generic;

    /// <summary>
    /// 反射工具类
    /// </summary>
    public class ReflectionTool
    {
        /// <summary>
        /// 缓存ConvertValue属性对象
        /// </summary>
        private static Dictionary<string, Attribute> cacheConvertValue = new Dictionary<string, Attribute>();

        /// <summary>
        /// 支持多线程
        /// </summary>
        private static object lockObj = new object();

        /// <summary>
        /// 根据目标对象获取指定属性值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="targetObject"></param>
        /// <returns></returns>
        public static object GetPropertyValue(string propertyName, object targetObject)
        {
            if (targetObject != null)
            {
                PropertyInfo propInfo = GetPropertyInfoFromCache(targetObject.GetType(), propertyName);
                if (propInfo != null)
                {
                    return propInfo.GetValue(targetObject, null);
                }
            }
            return "";
        }

        /// <summary>
        /// 根据目标对象获取指定属性值
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetPropertyFormatValue(PropertyInfo pi, object obj)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 返回格式化的数字字符串
        /// </summary>
        /// <param name="propName">字段名</param>
        /// <param name="propValue">字段值</param>
        /// <param name="decimals">小数位数</param>
        /// <returns></returns>
        public static string FormatNumberString(string propName, string propValue, int decimals)
        {
            string result = propValue;
            if (!string.IsNullOrEmpty(propValue))
            {
                // 正则表达式剔除非数字字符（不包含小数点.）
                var decimalStr = System.Text.RegularExpressions.Regex.Replace(propValue, @"[^\d.\d]", "");//数字部分
                if (string.IsNullOrEmpty(decimalStr)) return propValue;
                var enDecimalStr = propValue.Replace(decimalStr, "");//非数字部分
                result = Math.Round(Convert.ToDecimal(decimalStr), decimals, MidpointRounding.AwayFromZero)
                                .ToString(CultureInfo.InvariantCulture) + enDecimalStr;
            }
            return result;
        }
        /// <summary>
        /// 如果属性中含有ConvertValueAttribute,将自动转换值
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetDisplayValue(PropertyInfo pi, object obj)
        {
            var convertAttr = GetCustomAttributeEx<ConvertValueAttribute>(pi);

            var piValue = pi.GetValue(obj, null) == null ? "" : pi.GetValue(obj, null).ToString();

            if (convertAttr != null)
            {
                return convertAttr.GetName(piValue);
            }
            return piValue;
        }

        /// <summary>
        /// 获取属性标签的扩展方法,使用了缓存,提高效率
        /// 支持多线程
        /// </summary>
        /// <typeparam name="T">标签</typeparam>
        /// <param name="pi">属性</param>
        /// <returns>标签</returns>
        public static T GetCustomAttributeEx<T>(PropertyInfo pi) where T : Attribute
        {
            if (pi == null)
            {
                return default(T);
            }
            var cacheKey = (pi.DeclaringType != null ? pi.DeclaringType.Name : "") + pi.Name + typeof(T).Name;
            T convertAttr;
            lock (lockObj)
            {
                if (cacheConvertValue.ContainsKey(cacheKey))
                {
                    convertAttr = cacheConvertValue[cacheKey] as T;
                }
                else
                {
                    convertAttr = pi.GetCustomAttribute<T>();
                    cacheConvertValue.Add(cacheKey, convertAttr);
                }
            }
            return convertAttr;
        }

        /// <summary>
        /// 获取类的自定义标签
        /// </summary>
        /// <typeparam name="T">标签</typeparam>
        /// <param name="type">类</param>
        /// <returns>标签</returns>
        public static T GetCustomAttributeEx<T>(Type type) where T : Attribute
        {

            var cacheKey = type.FullName + typeof(T).Name;
            T convertAttr;

            if (cacheConvertValue.ContainsKey(cacheKey))
            {
                convertAttr = cacheConvertValue[cacheKey] as T;
            }
            else
            {
                convertAttr = type.GetCustomAttribute(typeof(T), true) as T;
                lock (lockObj)
                {
                    if (!cacheConvertValue.ContainsKey(cacheKey))
                    {
                        cacheConvertValue.Add(cacheKey, convertAttr);
                    }
                }
            }

            return convertAttr;
        }

        /// <summary>
        /// 根据类型获取BindingFlags.Public 和 BindingFlags.Instance 的属性
        /// 去掉了缓存
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertyInfosFromCache(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// 根据类型获取指定属性
        /// 去掉了缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfoFromCache(Type type, string name)
        {
            return type.GetProperty(name);
        }
        /// <summary>
        /// 根据类型获取BindingFlags.InvokeMethod 或 BindingFlags.Public 的方法 
        /// 去掉了缓存
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo[] GetMethodInfosFromCache(Type type)
        {
            return type.GetMethods();
        }

        /// <summary>
        /// 根据类型获取BindingFlags.InvokeMethod 或 BindingFlags.Public 的方法 
        ///  去掉了缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfoFromCache(Type type, string methodName)
        {
            return type.GetMethod(methodName);
        }


    }
}
