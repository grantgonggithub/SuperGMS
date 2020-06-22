using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.AttributeEx
{
    /// <summary>
    /// 原始模型,用来获取数据长度,自动构建验证框架,QtFrom控件使用
    /// 另外自定义列表和编辑时也会用oraModel匹配自定义属性
    ///
    /// </summary>
    public class OraModelAttribute : Attribute
    {
        /// <summary>
        /// 记录原始Model,用来获取MaxLength
        /// </summary>
        /// <param name="oraModel">原始模型</param>
        /// <param name="dbContext">dbcontext</param>
        public OraModelAttribute(Type oraModel, Type dbContext)
        {
            OraModel = oraModel;
            DbContext = dbContext;
        }

        /// <summary>
        /// 原始对象
        /// </summary>
        public Type OraModel { get; set; }

        /// <summary>
        /// 数据库
        /// </summary>
        public Type DbContext { get; set; }
    }
}