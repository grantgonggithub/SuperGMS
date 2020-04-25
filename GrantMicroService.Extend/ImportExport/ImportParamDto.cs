using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.Extend.ImportExport
{
    /// <summary>
    /// 导入参数
    /// </summary>
    public class ImportParamDto
    {
        /// <summary>
        /// Gets or sets 远程服务器名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets 上传业务Guid
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// 保留字段
        /// </summary>
        public string Reserve { get; set; }
    }
}
