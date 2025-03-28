/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx.GrantDbContext
 文件名：  GrantDapperDBContext
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/3 0:40:46

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Dapper;
using SuperGMS.DB.EFEx.CrudRepository;
using SuperGMS.DB.EFEx.GrantDbContext;
using SuperGMS.DB.EFEx.GrantDbFactory;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// GrantDapperDBContext
    /// </summary>
    public class DapperDBContext : IDapperDbContext
    {
        //public event DbDapperCommit OnDbCommit;

        private DbInfo dbInfo;

        /// <summary>
        /// 当前数据库信息
        /// </summary>
        public DbInfo DbInfo { get { return dbInfo; } }

        public DapperDBContext(DbInfo info)
        {
            dbInfo = info;
            // 这里应该根据工厂生成 mysql或者其他数据库连接实例，
            // _dbConnection = ConnectionManager.CreateConection(connectionString, info.DbType);
        }

        private void SqlRepositoryManager_OnDbCommit(DBConnection GrantDBConnection, SqlPara auditSql)
        {
            //OnDbCommit?.Invoke(GrantDBConnection, auditSql);
        }

        public void Dispose()
        {
            dbInfo = null;
        }

        /// <summary>
        /// 获取执行Sql的Repository
        /// </summary>
        /// <returns>ISqlRepository</returns>
        public ISqlRepository GetRepository()
        {
            return SqlRepositoryManager.GetSqlRepository(dbInfo);
        }
    }
}