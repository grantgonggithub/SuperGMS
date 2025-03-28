using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace SuperGMS.DB
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DataBaseType
    {
        MySQL = 1,
        SqlServer = 2,
        Oracle = 3,
        None = 0
    }
    /// <summary>
    /// 数据库工具类
    /// </summary>
    public class DatabaseTool
    {
        /// <summary>
        /// 获取数据库类型
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static DataBaseType GetDbType(DbConnection conn)
        {
            if (IsMySqlDb(conn)) return DataBaseType.MySQL;
            if (IsOracleDb(conn)) return DataBaseType.Oracle;
            if (IsSqlServerDb(conn)) return DataBaseType.SqlServer;
            return DataBaseType.None;
        }
        /// <summary>
        /// 获取数据库类型
        /// </summary>
        /// <param name="dataBase"></param>
        /// <returns></returns>
        public static DataBaseType GetDbType(DatabaseFacade dataBase)
        {
            return GetDbType(dataBase.GetDbConnection());
        }
        /// <summary>
        /// 判断是否是Oracle数据库
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool IsOracleDb(DatabaseFacade db)
        {
            return IsOracleDb(db.GetDbConnection());
        }

        /// <summary>
        /// 判断是否是Oracle数据库
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static bool IsOracleDb(DbConnection conn)
        {
            //if (conn.ConnectionString.Contains("1521") || conn is OracleConnection)
            //{
            //    return true;
            //}

            return false;
        }

        /// <summary>
        /// 判断是否是MySql数据库
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool IsMySqlDb(DatabaseFacade db)
        {
            return IsMySqlDb(db.GetDbConnection());
        }

        /// <summary>
        /// 判断是否是MySql数据库
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static bool IsMySqlDb(DbConnection conn)
        {
            if (conn.ConnectionString.Contains("3306") || conn is MySqlConnector.MySqlConnection)
            {
                return true;
            }

            //return false;
            return true;
        }

        /// <summary>
        /// 判断是否是SqlServer数据库
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool IsSqlServerDb(DatabaseFacade db)
        {
            return IsOracleDb(db.GetDbConnection());
        }

        /// <summary>
        /// 判断是否是SqlServer数据库
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static bool IsSqlServerDb(DbConnection conn)
        {
            //if (conn is SqlConnection)
            //{
            //    return true;
            //}

            return false;
        }
    }
}
