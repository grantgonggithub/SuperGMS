/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Tools
 文件名：  FileHelper
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/31 10:49:20

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperGMS.Tools
{
    /// <summary>
    /// FileHelper
    /// </summary>
  public class FileHelper
    {
        /// <summary>
        /// 按路径读取文件
        /// </summary>
        /// <param name="filePath">文件的完整路径</param>
        /// <returns>文件内容</returns>
        public static string ReadFile(string filePath)
        {
            // filePath = AppContext.BaseDirectory + filePath;  这是个公共方法，路径应该从外面构造，要不就乱套了，这个方法只能按外面的路径来读文件，不应该有任何路径上的处理
            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception($"FileHelper.ReadFile.Error,文件路径不为空");
            }

            if (!File.Exists(filePath))
            {
                throw new Exception($"{filePath}文件路径不存在");
            }

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
    }
}
