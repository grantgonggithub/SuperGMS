/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx.GrantDbFactory
 文件名：  SqlServer
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/31 23:04:48

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using SuperGMS.DB.EFEx.CrudRepository;
using SuperGMS.DB.EFEx.DynamicSearch;

namespace SuperGMS.DB.EFEx.GrantDbFactory
{
    /// <summary>
    /// SqlServer
    /// </summary>
    public class SqlServer : DbBase
    {
        public SqlServer(DbInfo dbInfo)
            : base(dbInfo)
        {
        }

        protected override string Prefix
        {
            get { return "@"; }
        }

    }
}