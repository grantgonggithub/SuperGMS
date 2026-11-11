/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx
 文件名：  DBInfo
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/18 14:06:37

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// DBInfo
    /// </summary>
    public class DbInfo
    {
        /// <summary>
        /// Gets or sets 数据模型名称
        /// </summary>
        public string DbContextName { get; set; }

        /// <summary>
        /// Gets or sets Dbtype
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// Gets or sets 数据库名称
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// Gets or sets 数据库登录名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets 登录密码
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// Gets or sets 数据库Ip
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 数据执行时间
        /// </summary>
        public int CommandTimeout { get; set; }

        /// <summary>
        ///  Gets or sets 其他特殊的附加信息
        /// </summary>
        public string Other { get; set; }

        /// <summary>
        ///  tostring
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return $"DbContextName：{DbContextName},DbName:{DbName},DbType:{DbType},UserName:{UserName},Pwd:{Pwd},Ip:{Ip},Port:{Port},Other:{Other}";
        }
    }
}