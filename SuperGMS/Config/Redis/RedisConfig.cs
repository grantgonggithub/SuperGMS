﻿/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config
 文件名：   RedisConfig
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/19 16:14:00

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.Config
{
    /// <summary>
    /// RedisConfig
    /// </summary>
    public class RedisConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public List<RedisNode> Nodes { get; set; }

        //public static RedisConfig Default
        //{
        //    get { return Newtonsoft.Json.JsonConvert.DeserializeObject<RedisConfig>(DefaultJson); }
        //}

        public const string DefaultJson = @"{    ""RedisConfig"": {
        ""Nodes"": [
            {
                ""NodeName"": ""default"",
                ""IsMasterSlave"": false,
                ""Items"": [
                    {
                        ""Pool"": 1,
                        ""IsMaster"": false,
                        ""Server"": ""192.168.100.205"",
                        ""Port"": 6379,
                        ""AllowAdmin"": true,
                        ""ConnectTimeout"": 4000,
                        ""SyncTimeout"": 3000,
                        ""Ssl"": true,
                        ""Pwd"": ""123456""
                    }
                ]
            },
            {
                ""NodeName"": ""resource"",
                ""IsMasterSlave"": false,
                ""Items"": [
                    {
                        ""Pool"": 1,
                        ""IsMaster"": false,
                        ""Server"": ""192.168.100.204"",
                        ""Port"": 6379,
                        ""AllowAdmin"": true,
                        ""ConnectTimeout"": 4000,
                        ""SyncTimeout"": 3000,
                        ""Ssl"": true,
                        ""Pwd"": ""123456""
                    }
                ]
            }
        ]
    }}";
    }
}