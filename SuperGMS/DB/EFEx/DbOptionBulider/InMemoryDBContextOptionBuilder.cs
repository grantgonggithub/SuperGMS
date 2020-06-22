/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx.DbOptionBulider
 文件名：  InMemoryDBContextOptionBuilder
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/2 21:21:01

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// InMemoryDBContextOptionBuilder
    /// </summary>
    public class InMemoryDBContextOptionBuilder : IContextOptionBuilderFactory
    {
        /// <summary>
        /// CreateOptionsBuilder
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="connectionString">connectionString</param>
        /// <returns>DbContextOptionsBuilder</returns>
        public DbContextOptionsBuilder<T> CreateOptionsBuilder<T>(string connectionString)
            where T : DbContext
        {
            DbContextOptionsBuilder<T> options = new DbContextOptionsBuilder<T>();

            // InMemoryDatabaseDbContextOptionsBuilder<T> options = new InMemoryDatabaseDbContextOptionsBuilder<T>();
            options.UseInMemoryDatabase<T>(typeof(InMemoryDBContextOptionBuilder).Name);
            return options;
        }

        /// <summary>
        ///  返回连接字符串
        /// </summary>
        /// <param name="dbInfo">dbInfo</param>
        /// <returns>string</returns>
        public string GetConnectionString(DbInfo dbInfo)
        {
            return dbInfo.ToString();
        }
    }

    // public class InMemoryDatabaseDbContextOptionsBuilder<T> : DbContextOptionsBuilder<T> where T:DbContext
    // {
    //    public override DbContextOptionsBuilder<T> ConfigureWarnings(Action<WarningsConfigurationBuilder> warningsConfigurationBuilderAction)
    //    {
    //        return base.ConfigureWarnings(warningsConfigurationBuilderAction);
    //    }
    // }
}
