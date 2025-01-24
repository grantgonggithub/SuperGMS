/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Config.Models.FileServer
 文件名：  FileServerManager
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/17 17:50:54

 功能描述：

----------------------------------------------------------------*/
using System.Collections.Generic;

namespace SuperGMS.Config
{
    /// <summary>
    /// FileServerManager
    /// </summary>
    internal class FileServerManager
    {
        public const string FileServerName = "FileServer";
        private static List<FileServerItem> fileServers;

        private static object _root = new object();

        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="xml">xml</param>
        public static void Initlize(FileServer fileServer)
        {
            if (fileServer != null && fileServer.Items != null && fileServer.Items.Count > 0)
            {
                lock (_root)
                {
                    fileServers = fileServer.Items;
                }
            }
        }

        /// <summary>
        /// 获取文件服务器列表
        /// </summary>
        /// <returns>文件服务器列表</returns>
        public static FileServerItem[] GetFileServers()
        {
            lock (_root)
            {
                return fileServers?.ToArray();
            }
        }
    }
}
