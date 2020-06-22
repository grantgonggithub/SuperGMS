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
        MySql = 1,
        SqlServer = 2,
        Oracle = 3,
        InMemory = 4,
        PostgreSQL=5,
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
