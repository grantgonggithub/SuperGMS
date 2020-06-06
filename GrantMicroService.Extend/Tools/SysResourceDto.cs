using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.Extend.Tools
{
    /// <summary>
    /// 系统资源文件，兼顾其他系统
    /// </summary>
    public class SysResourceDto
    {
        /// <summary>
        /// 资源key，子系统同语种内唯一
        /// </summary>
        public string ResourceKey { get; set; }
        
        /// <summary>
        /// 键值
        /// </summary>
        public string ViewText { get; set; }
    }
}
