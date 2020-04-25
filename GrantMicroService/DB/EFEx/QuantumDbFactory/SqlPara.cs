/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.DB.EFEx.GrantDbFactory
 文件名：  SqlPara
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/11/12 17:42:59

 功能描述：

----------------------------------------------------------------*/
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace GrantMicroService.DB.EFEx.GrantDbFactory
{
    /// <summary>
    /// SqlPara
    /// </summary>
    public class SqlPara
    {
        public string sql { get; set; }

        public DynamicParameters parameters { get; set; }

        public object param { get; set; }

        public DbTransaction dbTransaction { get; set; }

    }
}
