/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config.LogConfig
 文件名：   LogConfig
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/23 16:38:28

 功能描述：

----------------------------------------------------------------*/
using System.Collections.Generic;

namespace SuperGMS.Config.LogConfig
{
    /// <summary>
    /// LogConfig
    /// </summary>
    public class LogConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public string LogLocation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ShowConsole { get; set; }

        public int LogLastError { get; set; }

        //过滤日志级别
        public string LogLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Filter> Filter { get; set; }
    }
}