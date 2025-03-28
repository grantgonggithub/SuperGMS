/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Config.Models.DataBase
 文件名：  Slave
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/22 11:42:10

 功能描述：

----------------------------------------------------------------*/
namespace SuperGMS.Config
{
    /// <summary>
    /// Slave
    /// </summary>
    internal class Slave : DbIpPort
    {
        /// <summary>
        /// Gets or sets 索引
        /// </summary>
        public int Pool { get; set; }
    }
}
