using System;
using System.Collections.Generic;
using System.Text;
using GrantMicroService.Config;

namespace GrantMicroService.ApiHelper
{
    /// <summary>
    /// 微服务信息
    /// </summary>
    public class ServiceInfo
    {
        /// <summary>
        /// 微服务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 发布日期
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// 完成名称
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// 完整路径
        /// </summary>
        public string AssemblyPath { get; set; }

        /// <summary>
        /// 监听端口
        /// </summary>
        public int ListenPort { get; set; }

        /// <summary>
        /// 计算机名称
        /// </summary>
        public string ComputerName { get; set; }

        /// <summary>
        /// 进程信息
        /// </summary>
        public string ProcessInfo { get; set; }

        /// <summary>
        /// 计算机地址
        /// </summary>
        public string ComputerAddress { get; set; }

        /// <summary>
        /// netcore runtime version
        /// </summary>
        public string NetCoreVersion { get; set; }

        public Configuration Config { get; set; }
    }
}
