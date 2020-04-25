/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Config.Models.DataBase
 文件名：  DataBaseInfo
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/22 11:42:34

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.Config
{
    /// <summary>
    /// 数据库配置对象,  包含了数据库连接信息, 以及EF对应的上下文名称, 以及数据库类型, 主从配置
    /// </summary>
    internal class DbModelContext
    {
        /// <summary>
        /// Gets or sets dbcontext名字，通过这个名字来关联物理数据库和数据模型
        /// </summary>
        public string DbContextName { get; set; }

        /// <summary>
        /// Gets or sets 数据库类型
        /// </summary>
        public string DbType { get; set; }

        /// <summary>
        /// Gets or sets 索引
        /// </summary>
        public int Pool { get; set; }

        /// <summary>
        /// Gets or sets 登录用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets 密码
        /// </summary>
        public string PassWord { get; set; }

        /// <summary>
        /// Gets or sets 数据库名称
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets 主库配置
        /// </summary>
        public Master Master { get; set; }

        /// <summary>
        /// Gets or sets 从库配置列表
        /// </summary>
        public List<Slave> Slaves { get; set; }
    }
}