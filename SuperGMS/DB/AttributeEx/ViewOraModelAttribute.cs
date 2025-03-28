//
// 文件：OraModelAttribute.cs
// 作者：Grant
// 最后更新日期：2014-8-20
using System;

namespace SuperGMS.DB.AttributeEx
{
    public class ViewOraModelAttribute : Attribute
    {
        /// <summary>
        /// 原始对象
        /// </summary>
        public Type OraModel { get; set; }


        /// <summary>
        /// 记录原始Model
        /// </summary>
        /// <param name="oraModel"></param>
        public ViewOraModelAttribute(Type oraModel)
        {
            OraModel = oraModel;
        }
    }
}
