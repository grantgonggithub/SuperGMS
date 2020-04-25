//
// 文件：TypeUtil.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29

#region

using System;
using System.ComponentModel;

#endregion

namespace GrantMicroService.DB.EFEx.DynamicSearch
{
    /// <summary>
    ///     Type类的处理工具类
    ///     Add By Grant 2014-3-27
    /// </summary>
    public class TypeUtil
    {
        /// <summary>
        ///     如果类型是 类型? 或者 Nullable  类型的，直接转换成 原始类型
        /// </summary>
        /// <param name="conversionType">可Null类型</param>
        /// <returns>实际类型</returns>
        public static Type GetUnNullableType(Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //如果是泛型方法，且泛型类型为Nullable<>则视为可空类型
                //并使用NullableConverter转换器进行转换
                var nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            return conversionType;
        }
    }
}