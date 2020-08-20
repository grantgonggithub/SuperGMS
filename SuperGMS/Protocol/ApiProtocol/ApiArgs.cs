/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

项目名称：SuperGMS.Protocol.ApiProtocol
文件名：ApiArgs.cs
创建者：grant(巩建春)
CLR版本：4.0.30319.42000
时间：2020/8/18 星期二 11:05:16

功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.Protocol.ApiProtocol
{
    /// <summary>
    ///  可以有自由定义WebApi参数个格式，不走标准格式
    /// <see cref="ApiArgs" langword="" />
    /// </summary>
    public class ApiArgs
    {
        /// <summary>
        /// Http请求的Header头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Url ？后面的参数
        /// </summary>
        public Dictionary<string, string> Params { get; set; }
        /// <summary>
        /// http body的内容
        /// </summary>
        public string Body { get; set; }
    }

}
