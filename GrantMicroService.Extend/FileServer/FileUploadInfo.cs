using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.Extend.FileServer
{
    /// <summary>
    /// 文件上传信息
    /// </summary>
    public class FileUploadInfo
    {
        /// <summary>
        /// Gets or sets 远程服务器名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets 上传业务Guid
        /// </summary>
        public string Guid { get; set; }
    }
}
