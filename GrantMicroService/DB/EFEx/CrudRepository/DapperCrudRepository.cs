/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.DB.EFEx.CrudRepository
 文件名：  DapperCrudRepository
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/2 22:00:13

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data.Common;
using GrantMicroService.DB.EFEx.CrudRepository;

namespace GrantMicroService.DB.EFEx
{
    // 已经被ISqlRepository的实现类替代，如Mysql,
    /// <summary>
    /// DapperCrudRepository
    /// </summary>
    // public class DapperCrudRepository : ISqlRepository
    // {
    //    private DbInfo _dbInfo;

    // public DapperCrudRepository(DbConnection dbConn, DbInfo dbInfo)
    //    {
    //        _DbConnection = dbConn;
    //        _dbInfo = dbInfo;
    //    }

    // public List<T> SqlQuery<T>(string sql, params object[] parameters)
    //    {
    //        throw new NotImplementedException();
    //    }

    // public int ExecuteSqlCommand(string sql, params object[] parameters)
    //    {
    //        throw new NotImplementedException();
    //    }
    // }
}