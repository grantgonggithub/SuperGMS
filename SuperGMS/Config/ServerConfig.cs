/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config
 文件名：   Configuration
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/19 11:56:47

 功能描述：

----------------------------------------------------------------*/

namespace SuperGMS.Config
{
    public class ServerConfig
    {
        /// <summary>
        ///  rpcService
        /// </summary>
        public RpcService RpcService { get; set; }

        public ConfigCenter ConfigCenter { get; set; }

        //public static ServerConfig Default { get { return Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfig>(DefaultJson); } }
        public static string DefaultJson(string appName) => string.Concat(@"{
    ""ServerConfig"": {
        ""RpcService"": {
            ""Pool"": 0,
            ""Ip"": ""192.168.7.*"",
            ""Port"": 0,
            ""PortList"": {
                """+appName+ @""": 19999
            },
            ""ServerType"": 1,
            ""RouterType"": 2,
            ""AssemblyPath"": null,
            ""TimeOut"": 0,
            ""Enable"": true
        }
    }
}");
    }
}