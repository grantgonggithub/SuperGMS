﻿/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config
 文件名：   HttpProxy
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/19 16:18:49

 功能描述：

----------------------------------------------------------------*/
using System.Collections.Generic;

namespace SuperGMS.Config
{
    /// <summary>
    /// HttpProxy
    /// </summary>
    public class HttpProxy
    {
        /// <summary>
        /// 
        /// </summary>
        public List<HttpProxyItem> Items { get; set; }

        //public static HttpProxy Default
        //{
        //    get { return Newtonsoft.Json.JsonConvert.DeserializeObject<HttpProxy>(DefaultJson); }
        //}

        public const string DefaultJson = @"{    ""HttpProxy"": {
        ""Items"": [
            {
                ""Name"": ""ExpService1""
            },
            {
                ""Name"": ""ExpService2""
            }
        ]
    }}";
    }
}