/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.DB.EFEx.GrantDbFactory
 文件名：  SqlRepositoryManager
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/31 22:59:30

 功能描述：

----------------------------------------------------------------*/
using Dm.filter;
using Dm.util;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace SuperGMS.DB.EFEx.GrantDbFactory
{
    /// <summary>
    /// SqlRepositoryManager
    /// </summary>
    public class SqlRepositoryManager
    {
        private static ILogger logger = SuperGMS.Log.LogFactory.CreateLogger<SqlRepositoryManager>();
        public static DbBase GetSqlRepository(DbInfo dbInfo)
        {
            DbBase db = null;
            switch (dbInfo.DbType)
            {
                case DbType.MySql:
                    db = new MySql(dbInfo);
                    break;
                case DbType.Oracle:
                    db = new Oracle(dbInfo);
                    break;
                case DbType.SqlServer:
                    db = new SqlServer(dbInfo);
                    break;
                case DbType.PostgreSQL:
                    db = new PostgreSql(dbInfo);
                    break;
                default:
                    throw new Exception($"SqlRepositoryManager.GetSqlRepository未知数据库类型{dbInfo.DbType.ToString()}");
            }
            return db;
        }

        public static ISqlSugarClient GetSqlSugarClient(DbInfo dbInfo)
        {
            var client = new SqlSugarClient(new ConnectionConfig
            {
                ConfigId = dbInfo.toString().GetHashCode(),
                ConnectionString = getDbConnectionString(dbInfo),
                DbType = getSqlSugarDbType(dbInfo),
                IsAutoCloseConnection = false,
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    // 实体名转换为表名（驼峰转下划线）
                    EntityNameService = (type, entity) =>
                    {
                        // 如果实体有 SugarTable 特性，使用特性指定的表名
                        var sugarTable = type.GetCustomAttribute<SugarTable>();
                        if (sugarTable != null && !string.IsNullOrEmpty(sugarTable.TableName))
                            return;

                        // 否则使用规则转换：FormGoogleData -> form_google_data
                        entity.DbTableName = ConvertToSnakeCase(type.Name);
                    },

                    // 修改 EntityService 委托，不返回值，只设置列名
                    EntityService = (property, column) =>
                    {
                        // 如果属性有 SugarColumn 特性，使用特性指定的列名
                        var sugarColumn = property.GetCustomAttribute<SugarColumn>();
                        if (sugarColumn != null && !string.IsNullOrEmpty(sugarColumn.ColumnName))
                            return;

                        // 否则使用规则转换：CreatedDate -> created_date
                        column.DbColumnName = ConvertToSnakeCase(property.Name);
                    }
                }
            }, sqlInfo =>
            {
                sqlInfo.Aop.OnLogExecuting = (sql, para) =>
                {
                    logger.LogInformation(UtilMethods.GetNativeSql(sql, para));
                };
            });
            doSomePreProcess(client, dbInfo);
            return client;
        }
        private static string ConvertToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder();
            result.Append(char.ToLower(input[0]));

            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    result.Append('_');
                    result.Append(char.ToLower(input[i]));
                }
                else
                {
                    result.Append(input[i]);
                }
            }

            return result.ToString();
        }

        private static SqlSugar.DbType getSqlSugarDbType(DbInfo dbInfo) {
            return (SqlSugar.DbType)dbInfo.DbType;
        }

        private static void doSomePreProcess(ISqlSugarClient client, DbInfo dbInfo)
        {
            switch (dbInfo.DbType)
            {
                case DbType.Doris:
                    client.Ado.ExecuteCommand($"USE {dbInfo.DbName};");
                    break;
                default:
                    break;

            }
        }

        private static string getDbConnectionString(DbInfo dbInfo)
        {
            return dbInfo.DbType switch
            {
                DbType.MySql => MySqlDBContextOptionBuilder.GetDbConnectionString(dbInfo),
                DbType.SqlServer => SqlServerDBContextOptionBuilder.GetDbConnectionString(dbInfo),
                DbType.Oracle => OracleDBContextOptionBuilder.GetDbConnectionString(dbInfo),
                DbType.PostgreSQL => PostgresqlDBContextOptionBuilder.GetDbConnectionString(dbInfo),
                DbType.Doris => $"server={dbInfo.Ip};port={dbInfo.Port};database={dbInfo.DbName};user={dbInfo.UserName};password={dbInfo.Pwd};Pooling=true;LoadBalance=RoundRobin;",
                DbType.MongoDb => mongoDbConnectionString(dbInfo),
                DbType.Sqlite => new SqliteConnectionStringBuilder() { DataSource = Path.Combine(Environment.CurrentDirectory, dbInfo.DbName), Mode = SqliteOpenMode.ReadWriteCreate, Password = dbInfo.Pwd }.toString(),
                _ => throw new Exception($"database.config中配置了不支持的数据库类型{dbInfo.DbType}"),
            };
        }

        private static string mongoDbConnectionString(DbInfo dbInfo)
        {
            var otherInfo = string.IsNullOrWhiteSpace(dbInfo.Other) ?string.Empty: ";replicaSet=" + dbInfo.Other;
            //host = 222.71.212.3; Port = 27017; Database = testDB; Username = root; Password = 123456; authSource = admin; replicaSet =
            // var str="mongodb://root:123456@117.72.212.3:27017,117.72.212.4:27017,117.72.212.5:27017/testDB?authSource=admin&replicaSet=rs0";//117.72.212.3:27017,117.72.212.4:27017,...：多个 MongoDB 节点地址。 //testDB：你要连接的数据库。 //authSource=admin：认证数据库是 admin。 //replicaSet=rs0：副本集名称必须写对（和你 MongoDB 实际副本集名字一致）。
            //return dbInfo.Ip.indexOf(":") < 0 ? $"mongodb://{dbInfo.UserName}:{dbInfo.Pwd}@{dbInfo.Ip}:{dbInfo.Port}/{dbInfo.DbName}?authSource=admin{otherInfo}" : $"mongodb://{dbInfo.UserName}:{dbInfo.Pwd}@{dbInfo.Ip}/{dbInfo.DbName}?authSource=admin{otherInfo}";
            return $"host={dbInfo.Ip};Port={dbInfo.Port};Database={dbInfo.DbName};Username={dbInfo.UserName};Password={dbInfo.Pwd};authSource=admin{otherInfo}";
        }
    }
}
