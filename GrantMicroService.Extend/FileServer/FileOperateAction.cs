using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.Extend.FileServer
{
    /// <summary>
    /// 文件操作动作
    /// </summary>
    public enum FileOperateAction
    {
        FileUpload = 1,
        FileDownload,
        FileDelete,
        DirCreate,
    }
}
