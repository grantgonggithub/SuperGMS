using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.AttributeEx
{
    /// <summary>
    /// 指定参数模型约束符合 udf模型定义的约束
    /// </summary>
    public class UdfModelAttribute : Attribute
    {
        /// <summary>
        /// 指定dto的自定义模型 遵守的原型
        /// </summary>
        /// <param name="sysId"></param>
        /// <param name="modelName"></param>
        public UdfModelAttribute(string sysId, string modelName)
        {
            this.SysId = sysId;
            this.ModelName = modelName;
        }

        /// <summary>
        /// sysId
        /// </summary>
        public string SysId { get; set; }
        /// <summary>
        /// 模型名称
        /// </summary>
        public string ModelName { get; set; }
    }
}
