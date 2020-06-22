/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config.LogConfig
 文件名：   Filter
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/23 16:39:05

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.Config.LogConfig
{
    /// <summary>
    /// Filter
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// 
        /// </summary>
        public string FilterGroup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FilterApiName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FilterServiceName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FilterWords { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LogLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int MaxSize { get; set; }
    }
}