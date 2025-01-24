// <copyright file="DownFileInfo.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace SuperGMS.Extend.FileServer
{
    /// <summary>
    /// 下载文件参数
    /// </summary>
    public class DownFileInfo
    {
        /// <summary>
        /// Gets or sets 远程文件路径
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Gets or sets 文件原始名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets 错误引导页面
        /// </summary>
        public string ErrUri { get; set; }

        /// <summary>
        /// Gets or sets token
        /// </summary>
        public string Token { get; set; }
    }
}
