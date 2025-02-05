﻿/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config
 文件名：   FileServer
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/19 16:22:46

 功能描述：

----------------------------------------------------------------*/
using System.Collections.Generic;

namespace SuperGMS.Config
{
    /// <summary>
    /// FileServer
    /// </summary>
    public class FileServer
    {
        /// <summary>
        /// 
        /// </summary>
        public List<FileServerItem> Items { get; set; }


        //public static FileServer Default
        //{
        //    get { return Newtonsoft.Json.JsonConvert.DeserializeObject<FileServer>(DefaultJson); }
        //}

        public const string DefaultJson = @"{    ""FileServer"": {
        ""Items"": [
            {
                ""Url"": ""http://192.168.100.214/file_server""
            }
        ]
    }}";
    }
}