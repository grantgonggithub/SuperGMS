using System;
using System.Collections.Generic;
using GrantMicroService.Rpc;

namespace GrantMicroService.ApiHelper
{
    /// <summary>
    ///     类帮助信息
    /// </summary>
    public class ClassInfo
    {
        /// <summary>
        /// 默认构造
        /// </summary>
        public ClassInfo()
        {
            this.PropertyInfo = new List<ClassInfo>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassInfo"/> class.
        /// </summary>
        /// <param name="assemblyName">程序集</param>
        public ClassInfo(string assemblyName = "")
        {
            this.PropertyInfo = new List<ClassInfo>();
            this.AssemlyName = assemblyName;
        }

        /// <summary>
        ///     静态空类型描述
        /// </summary>
        public static ClassInfo Nullables => new ClassInfo
        {
            FullName = typeof(Nullables).FullName,
            Name = typeof(Nullables).Name,
            Desc = "空参数",
            Type = "空",
        };

        /// <summary>
        ///     微服务集名称
        /// </summary>
        public string AssemlyName { get; set; }

        /// <summary>
        ///     完整名称
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        ///     名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     帮助节点名
        /// </summary>
        public string XmlNode { get; set; }

        /// <summary>
        ///     帮助描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        ///     限制描述
        /// </summary>
        public string LimitDesc { get; set; }

        /// <summary>
        ///     类型名称
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 状态码描述
        /// </summary>
        public string CodeDesc { get; set; }

        /// <summary>
        /// json描述信息
        /// </summary>
        public string JsonDesc { get; set; }

        /// <summary>
        /// 是否开放Api
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        ///     属性信息
        /// </summary>
        public List<ClassInfo> PropertyInfo { get; set; }

        /// <summary>
        /// api
        /// 类相关属性信息
        /// </summary>
        public ApiClassInfo ApiClassInfo { get; set; }

        /// <summary>
        ///     设置简单类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="desc">描述</param>
        public void SetSimpleType(Type type, string desc)
        {
            Name = type.Name;
            FullName = type.FullName;
            Desc = desc;
            XmlNode = string.Empty;
            Type = desc;
            PropertyInfo.Clear();
        }
    }
}