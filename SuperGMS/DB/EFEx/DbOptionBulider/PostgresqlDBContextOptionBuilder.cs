/*----------------------------------------------------------------
Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

项目名称：SuperGMS.DB.EFEx.DbOptionBulider
文件名：PostgresqlDBContextOptionBuilder.cs
创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
CLR版本：4.0.30319.42000
时间：2019/9/11 14:57:44

功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    ///
    /// <see cref="PostgresqlDBContextOptionBuilder" langword="" />
    /// </summary>
    public class PostgresqlDBContextOptionBuilder : IContextOptionBuilderFactory
    {
        /// <summary>
        /// 根据数据类型获取连接字符串
        /// </summary>
        /// <param name="dbInfo">dbInfo</param>
        /// <returns>string</returns>
        public static string GetDbConnectionString(DbInfo dbInfo)
        {
           return $"Host={dbInfo.Ip}; Port={(dbInfo.Port > 0 ? dbInfo.Port : 5432)}; Database={dbInfo.DbName};User ID={dbInfo.UserName}; Password ={dbInfo.Pwd};Pooling=true;";
        }

        public DbContextOptionsBuilder<T> CreateOptionsBuilder<T>(string connectionString) where T : DbContext
        {
            DbContextOptionsBuilder<T> options = new DbContextOptionsBuilder<T>();
            options.UseNpgsql<T>(connectionString);
            return options;
        }

        public string GetConnectionString(DbInfo dbInfo)
        {
            return PostgresqlDBContextOptionBuilder.GetDbConnectionString(dbInfo);
        }
    }
}