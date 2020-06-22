using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.AttributeEx
{
    /// <summary>
    /// 错误代码描述属性
    /// </summary>
    public class CodeDescAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeDescAttribute"/> class.
        /// 默认构造函数，提供接口函数名称 和 错误描述信息
        /// 用于ErrorCode标记的枚举类的枚举属性上，辅助生成帮助文档
        /// </summary>
        /// <param name="interfaceName">接口函数名称</param>
        /// <param name="desc">错误描述信息</param>
        public CodeDescAttribute(string desc, params string[] interfaceName)
        {
            InterfaceName = interfaceName;
            Description = desc;
        }

        /// <summary>
        /// Gets or sets 代码引用的接口
        /// </summary>
        public string[] InterfaceName { get; set; }

        /// <summary>
        /// Gets or sets 错误代码内容
        /// </summary>
        public string Description { get; set; }
    }
}