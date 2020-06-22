/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config
 文件名：   RedisItem
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/19 16:17:57

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.Config
{
    /// <summary>
    /// RedisItem
    /// </summary>
    public class RedisItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int Pool { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsMaster { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AllowAdmin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Ssl { get; set; }

        /// <summary>
        /// 新加redis ssl配置，为了兼容老的ssl 默认是false
        /// </summary>
        public bool Ssl2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// redis 数据库索引
        /// </summary>
        public int DbIndex { get; set; }

    }
}