/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config
 文件名：   DataBase
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/19 13:03:43

 功能描述：

----------------------------------------------------------------*/
namespace SuperGMS.Config
{
    /// <summary>
    /// DataBase
    /// </summary>
    public class DataBase
    {
        public string RefFile { get; set; }

        /// <summary>
        /// 数据库文件引用路径
        /// </summary>
        public string DbFile { get; set; }

        public string SqlFile { get; set; }

        //public static DataBase Default
        //{
        //    get { return Newtonsoft.Json.JsonConvert.DeserializeObject<DataBase>(DefaultJson); }
        //}

        public static string DefaultJson(string env) => @"{    ""DataBase"": {
        ""RefFile"": ""true"",
        ""DbFile"": ""database." + env + @".config"",
        ""SqlFile"": ""sqlmap.config""
    }}";
    }
}