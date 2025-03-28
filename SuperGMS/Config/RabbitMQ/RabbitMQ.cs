/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config
 文件名：   RabbitMQ
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/19 13:13:10

 功能描述：

----------------------------------------------------------------*/
using System.Collections.Generic;

namespace SuperGMS.Config
{
    /// <summary>
    /// RabbitMQ
    /// </summary>
    public class RabbitMQ
    {
        /// <summary>
        /// 
        /// </summary>
        public List<HostItem> Host { get; set; }

        //public static RabbitMQ Default
        //{
        //    get { return Newtonsoft.Json.JsonConvert.DeserializeObject<RabbitMQ>(DefaultJson); }
        //}

        public const string DefaultJson = @"{    ""RabbitMQ"": {
        ""Host"": [
            {
                ""Name"": ""Default"",
                ""Ip"": ""192.168.0.11"",
                ""Port"": 5672,
                ""UserName"": ""admin"",
                ""PassWord"": ""admin"",
                ""NoAckMsgCount"": 3
            }
        ]
    }}";
    }
}