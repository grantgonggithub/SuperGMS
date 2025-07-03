/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.DB.EFEx
 文件名：  DataBaseType
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/2 23:11:38

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// 数据访问层的类型
    /// </summary>
    public enum DataAccessLayerType
    {
        EF = 1,
        Dapper = 2,
    }

    /// <summary>
    /// DataBaseType
    /// </summary>
    public enum DbType
    {
        InMemory = -1,
        MySql = 0,
        SqlServer = 1,
        Sqlite = 2,
        Oracle = 3,
        PostgreSQL = 4,
        Dm = 5,
        Kdbndp = 6,
        Oscar = 7,
        MySqlConnector = 8,
        Access = 9,
        OpenGauss = 10,
        QuestDB = 11,
        HG = 12,
        ClickHouse = 13,
        GBase = 14,
        Odbc = 15,
        OceanBaseForOracle = 16,
        TDengine = 17,
        GaussDB = 18,
        OceanBase = 19,
        Tidb = 20,
        Vastbase = 21,
        PolarDB = 22,
        Doris = 23,
        Xugu = 24,
        GoldenDB = 25,
        TDSQLForPGODBC = 26,
        TDSQL = 27,
        HANA = 28,
        DB2 = 29,
        GaussDBNative = 30,
        DuckDB = 31,
        MongoDb = 32,
        Custom = 900
    }

    public class DbTypeParser
    {
        public static DbType Parser(string dbType)
        {
            DbType dt = DbType.MySql;
            if (!Enum.TryParse<DbType>(dbType, out dt))
            {
                dt = DbType.MySql;
            }

            return dt;
        }
    }
}
