/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx
 文件名：  MySqlDBContextOptionBuilder
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/2 20:59:58

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// MySqlDBContextOptionBuilder
    /// </summary>
    public class MySqlDBContextOptionBuilder : IContextOptionBuilderFactory
    {
        /// <summary>
        /// 根据数据类型获取连接字符串
        /// </summary>
        /// <param name="dbInfo">dbInfo</param>
        /// <returns>string</returns>
        public static string GetDbConnectionString(DbInfo dbInfo)
        {
            return $"Data Source={dbInfo.Ip};port={(dbInfo.Port > 0 ? dbInfo.Port : 3306)};Initial Catalog={dbInfo.DbName};user id={dbInfo.UserName};password={dbInfo.Pwd};Character Set=utf8;pooling=true;MaximumPoolsize=1000;";
        }

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
            options.UseMySql<T>(connectionString,ServerVersion.AutoDetect(connectionString));
            return options;
        }

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <param name="dbInfo">dbInfo</param>
        /// <returns>string</returns>
        public string GetConnectionString(DbInfo dbInfo)
        {
            return MySqlDBContextOptionBuilder.GetDbConnectionString(dbInfo);
        }
    }
}
