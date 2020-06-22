/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.DB.EFEx.DbOptionBulider
 文件名：  SqlServerDBContextOptionBuilder
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/2 21:04:05

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// SqlServerDBContextOptionBuilder
    /// </summary>
    public class SqlServerDBContextOptionBuilder : IContextOptionBuilderFactory
    {
        public DbContextOptionsBuilder<T> CreateOptionsBuilder<T>(string connectionString) where T : DbContext
        {
            DbContextOptionsBuilder<T> options = new DbContextOptionsBuilder<T>();
            options.UseSqlServer<T>(connectionString);
            return options;
        }

        public string GetConnectionString(DbInfo dbInfo)
        {
            return SqlServerDBContextOptionBuilder.GetDbConnectionString(dbInfo);
        }

        /// <summary>
        /// 根据数据类型获取连接字符串
        /// </summary>
        /// <param name="dbInfo">dbInfo</param>
        /// <returns>string</returns>
        public static string GetDbConnectionString(DbInfo dbInfo)
        {
            dbInfo.Port = dbInfo.Port > 0 ? dbInfo.Port : 1433;
            return $"Data Source = {dbInfo.Ip},{dbInfo.Port};Network Library = DBMSSOCN;Initial Catalog = {dbInfo.DbName};User  ID = {dbInfo.UserName};Password = {dbInfo.Pwd};";
        }
    }
}
