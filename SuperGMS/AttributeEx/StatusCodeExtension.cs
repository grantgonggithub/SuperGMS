using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SuperGMS.ExceptionEx;
using SuperGMS.Protocol.RpcProtocol;

namespace SuperGMS.AttributeEx
{
    /// <summary>
    /// 状态码扩展类
    /// </summary>
    public static class StatusCodeExtension
    {
        /// <summary>
        /// 枚举转为 StatusCode
        /// </summary>
        /// <typeparam name="TEnumClass">枚举类型</typeparam>
        /// <param name="enumValue">枚举值</param>
        /// <returns>状态码</returns>
        public static StatusCode ToCode<TEnumClass>(this TEnumClass enumValue)
            where TEnumClass : struct
        {
            var cd = GetEnumCodeDescription(enumValue);
            var code = (int)(object)enumValue;
            if (cd == null)
            {
                Console.WriteLine($"枚举值{enumValue} 没有定义属性[CodeDescAttribute]");

                // return StatusCode.ErrorCodeUndefined;
                return new StatusCode(code, StatusCode.ErrorCodeUndefined.ToString());
            }

            return new StatusCode(code, cd.Description);
        }

        /// <summary>
        /// 得到枚举类型的CodeDesc属性标签
        /// </summary>
        /// <param name="enumValue">枚举值</param>
        /// <returns>CodeDesc属性标签</returns>
        private static CodeDescAttribute GetEnumCodeDescription<TEnumClass>(TEnumClass enumValue)
        {
            string value = enumValue.ToString();
            FieldInfo field = enumValue.GetType().GetField(value);
            object[] objs = field.GetCustomAttributes(typeof(CodeDescAttribute), false);
            if (objs == null || objs.Length == 0)
            {
                return null;
            }

            CodeDescAttribute descriptionAttribute = (CodeDescAttribute)objs[0];
            return descriptionAttribute;
        }
    }
}