/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

项目名称：SuperGMS.Protocol.ApiProtocol
文件名：ApiResult.cs
创建者：grant(巩建春)
CLR版本：4.0.30319.42000
时间：2020/8/18 星期二 11:09:32

功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SuperGMS.Protocol.ApiProtocol
{
    /// <summary>
    ///
    /// <see cref="ApiResult" langword="" />
    /// </summary>
   public class ApiResult
    {
        /// <summary>
        /// 返回的Body内容
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 要写给客户端的Header内容
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 写给客户端的cookies
        /// </summary>
        public Dictionary<string, string> Cookies { get; set; }

        /// <summary>
        /// 返回内容的格式
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// 返回给对方的HttpCode
        /// </summary>
        public HttpStatusCode Code { get; set; }
    }
}
