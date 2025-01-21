using Microsoft.EntityFrameworkCore;

using SuperGMS.Config;
using SuperGMS.Log;

using System;
using System.Data.Common;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// 数据连接管理器
    /// 2015年10月13日修改  为了给DbContextFactory提供依赖注入
    /// 2016-12-16 增加读写分离相关内容
    /// </summary>
    public class ConnectionManager
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public DbConnection DbConnection { get; set; }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ConnectionManager()
        {
            this.DbConnection = null;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbConection">数据库连接对象</param>
        public ConnectionManager(DbConnection dbConection)
        {
            this.DbConnection = dbConection;
        }

        /// <summary>
        /// 创建DbContext 构造参数
        /// </summary>
        /// <returns></returns>
        public static DbContextOptions<T> CreateMySqlDbOption<T>(string ip, string uName, string uPwd, string dbName, string port = "3306") where T : DbContext
        {
            ////这里通过登录用户构建 connection
            var sqlConnectionString =
                $"Data Source={ip};port={port};Initial Catalog={dbName};user id={uName};password={uPwd};Character Set=utf8;Allow Zero Datetime=true;Convert Zero Datetime=true;pooling=true;MaximumPoolsize=100;";

            DbContextOptionsBuilder<T> options = new DbContextOptionsBuilder<T>();
            // 使用Console Debug EF Sql
            if (ServerSetting.GetConstValue("TrackSql")?.Value.ToLower() == "true")
            {
                options.UseLoggerFactory(LogFactory.LoggerFactory);
                options.EnableSensitiveDataLogging();
            }

            options.UseMySql(sqlConnectionString,ServerVersion.AutoDetect(sqlConnectionString));
            return options.Options;
        }

        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="conectionString">数据库连接字符串</param>
        /// <param name="dbType">数据库类型MYSQL,MSSQL</param>
        /// <returns></returns>
        public static DbConnection CreateConection(string conectionString, DbType dbType)
        {
            string connectionString = string.Empty;
            DbConnection dbConection = null;
            switch (dbType)
            {
                default:
                case DbType.MySql:
                    dbConection = new MySqlConnector.MySqlConnection(conectionString);
                    break;

                case DbType.SqlServer:
                    throw new NotImplementedException();

                case DbType.Oracle:
                    throw new NotImplementedException();
            }

            return dbConection;
        }

        /// <summary>
        /// 克隆原始数据库连接
        /// </summary>
        /// <param name="conection"></param>
        /// <returns></returns>
        public DbConnection Clone(DbConnection conection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 创建数据库连接对象
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="database">数据库</param>
        /// <param name="uid">用户名</param>
        /// <param name="pwd">密码</param>
        /// <param name="dbType">数据库类型,默认MYSQL</param>
        /// <returns>数据库连接对象</returns>
        public DbConnection CreateConection(string server, string database, string uid, string pwd, string dbType)
        {
            string connectionString = string.Empty;
            DbConnection dbConection = null;
            switch (dbType)
            {
                default:
                case "MYSQL":
                    dbConection = new MySqlConnector.MySqlConnection(string.Format("server={0};user id={1};password={2};persistsecurityinfo=True;database={3};Character Set=utf8;Allow Zero Datetime=true;Convert Zero Datetime=true;pooling=true;MaximumPoolsize=100;", server, uid, pwd, database));
                    break;
                case "SQLSERVER":
                    throw new NotImplementedException();
                case "ORACLE":
                    throw new NotImplementedException();
            }
            return dbConection;
        }
    }
}