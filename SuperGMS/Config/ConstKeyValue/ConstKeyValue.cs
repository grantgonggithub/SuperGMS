/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config
 文件名：   ConstKeyValue
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/19 16:09:53

 功能描述：

----------------------------------------------------------------*/
using System.Collections.Generic;

namespace SuperGMS.Config
{
    /// <summary>
    /// ConstKeyValue
    /// </summary>

    public class ConstKeyValue
    {
        /// <summary>
        /// 
        /// </summary>
        public List<ConstItem> Items { get; set; }

        //public static ConstKeyValue Default
        //{
        //    get { return Newtonsoft.Json.JsonConvert.DeserializeObject<ConstKeyValue>(DefaultJson); }
        //}

        public const string DefaultJson = @"{""ConstKeyValue"": {
        ""Items"": [
            {
                ""Key"": ""MaxHttpBody"",
                ""Value"": ""104857600""
            },
            {
                ""Key"": ""TrackSql"",
                ""Value"": ""true""
            },
            {
                ""Key"": ""HttpProxy"",
                ""Value"": ""http://192.168.100.2/v2_api/""
            }
        ]
    }}";
    }
}