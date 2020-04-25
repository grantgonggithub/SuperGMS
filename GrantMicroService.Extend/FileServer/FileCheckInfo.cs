using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.Extend.FileServer
{
    /// <summary>
    /// 文件检查信息
    /// </summary>
    public class FileCheckInfo
    {
        /// <summary>
        /// 消息处理 编号
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string BussinessType { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// 发起请求的Token信息
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 上传文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件操作动作
        /// </summary>
        public FileOperateAction Action { get; set; }

        /// <summary>
        /// 其他扩展属性
        /// </summary>
        public Dictionary<string,string> Other { get; set; }
    }
}
