using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrantMicroService.Audit;
using GrantMicroService.Audit.Model;
using Microsoft.EntityFrameworkCore.Infrastructure;
using GrantMicroService.DB.EFEx.DynamicSearch;
using GrantMicroService.Tools;
using Microsoft.EntityFrameworkCore;
using GrantMicroService.UserSession;
using GrantMicroService.Log;
using GrantMicroService.DB.EFEx;
using Microsoft.Extensions.Logging;

namespace GrantMicroService.Audit
{
    /// <summary>
    /// 审计相关函数
    /// </summary>
    public class AuditTool
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<AuditTool>();
        #region 审计需要的函数
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.Database.ExecuteSqlCommand("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.Database.ExecuteSqlCommand("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));

        /// <summary>
        /// 保存审计数据到业务库，如果表不存在，则抛出异常
        /// </summary>
        /// <param name="db">数据数据库</param>
        /// <param name="data">需要落盘的数据</param>
        /// <param name="dbInfo">数据k</param>
        public static void SaveAuditData(DatabaseFacade db, List<AuditData> data, DbInfo dbInfo)
        {
            try
            {
                if (dbInfo.DbType == DbType.MySql)
                {
                    data?.ForEach(x =>
                    {
                        var guid = Guid.NewGuid().ToString("N");
                        //插入审计数据
                        db.ExecuteSqlCommand(
                            "insert into audit_data(`AUDIT_ID`,`TTID`,`SYS_ID`,`TABLE_NAME`,`PRIMARY_KEY`,`TRANSACTION_ID`,`STATE_NAME`,`IS_SQL`,`SQL`,`USER_ID`,`CREATED_DATE`) values(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10)",
                            guid, x.Ttid, x.SysId, x.TableName, x.PrimarkKey, x.TransactionId, x.StateName, x.IsSql,
                            x.Sql,
                            x.UserId, DateTime.Now);
                        if (!x.IsSql)
                        {
                            x.Properties?.ForEach(y =>
                            {
                                db.ExecuteSqlCommand("insert into  audit_data_line(`AUDIT_ID`,`PROPERTY_NAME`,`NEW_VALUE_FORMATTED`,`OLD_VALUE_FORMATTED`) values(@p0,@p1,@p2,@p3)",
                                    guid, y.PropertyName, y.NewValueFormatted, y.OldValueFormatted);
                            });
                        }
                    });
                }
                else if (dbInfo.DbType == DbType.SqlServer)
                {
                    data?.ForEach(x =>
                    {
                        var guid = Guid.NewGuid().ToString("N");
                        //插入审计数据
                        db.ExecuteSqlCommand(
                            "insert into audit_data([AUDIT_ID],[TTID],[SYS_ID],[TABLE_NAME],[PRIMARY_KEY],[TRANSACTION_ID],[STATE_NAME],[IS_SQL],[SQL],[USER_ID],[CREATED_DATE]) values(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10)",
                            guid, x.Ttid, x.SysId, x.TableName, x.PrimarkKey, x.TransactionId, x.StateName, x.IsSql,
                            x.Sql,
                            x.UserId, DateTime.Now);
                        if (!x.IsSql)
                        {
                            x.Properties?.ForEach(y =>
                            {
                                db.ExecuteSqlCommand("insert into  audit_data_line([AUDIT_ID],[PROPERTY_NAME],[NEW_VALUE_FORMATTED],[OLD_VALUE_FORMATTED]) values(@p0,@p1,@p2,@p3)",
                                    guid, y.PropertyName, y.NewValueFormatted, y.OldValueFormatted);
                            });
                        }
                    });
                }
                else //oracle
                {
                    data?.ForEach(x =>
                    {
                        var guid = Guid.NewGuid().ToString("N");
                        //插入审计数据
                        db.ExecuteSqlCommand(
                            "insert into \"audit_data\"(AUDIT_ID,TTID,SYS_ID,TABLE_NAME,PRIMARY_KEY,TRANSACTION_ID,STATE_NAME,IS_SQL,SQL,USER_ID,CREATED_DATE) values(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10)",
                            guid, x.Ttid, x.SysId, x.TableName, x.PrimarkKey, x.TransactionId, x.StateName, x.IsSql,
                            x.Sql,
                            x.UserId, DateTime.Now);
                        if (!x.IsSql)
                        {
                            x.Properties?.ForEach(y =>
                            {
                                db.ExecuteSqlCommand("insert into  \"audit_data_line\"(AUDIT_ID,PROPERTY_NAME,NEW_VALUE_FORMATTED,OLD_VALUE_FORMATTED) values(@p0,@p1,@p2,@p3)",
                                    guid, y.PropertyName, y.NewValueFormatted, y.OldValueFormatted);
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "保存审计数据异常");
                throw ex;
            }

        }


        internal static string FindSysIdByDb(UserContext user, DbInfo dbInfo)
        {
            var sysInfo = user.GetSysInfo();
            for (int i = 0; i < sysInfo.Count; i++)
            {
                if (string.Equals(sysInfo[i].DBName, dbInfo.DbName, StringComparison.CurrentCultureIgnoreCase)
                    && string.Equals(sysInfo[i].DBIP, dbInfo.Ip, StringComparison.CurrentCultureIgnoreCase))
                {
                    return sysInfo[i].SysID;
                }
            }
            // if wms ,todo 暂时不做，不支持wms            

            return "";
        }
        /// <summary>
        /// 锁定api获取
        /// </summary>
        private static object lockObject = new object();
        /// <summary>
        /// 得到审计配置信息,租户下所有配置
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dbInfo"></param>
        /// <returns></returns>
        internal static List<AuditConfigResult> GetAuditConfig(UserContext user, DbInfo dbInfo)
        {

            if (user == null)
            {
                //todo 无法获取用户信息
                return new List<AuditConfigResult>();
            }

            try
            {
                var sysId = FindSysIdByDb(user, dbInfo);
                if (string.IsNullOrEmpty(sysId))
                {
                    logger.LogError($"无法根据dbName:{dbInfo?.DbName}[ip:{dbInfo?.Ip}]查找到sysId");
                    return new List<AuditConfigResult>();
                }
                var key = $"{user.TTID}.{dbInfo?.DbName}.{dbInfo?.Ip}.AuditConfigResult";
                var cacheValue = GlobalCache.Get<List<AuditConfigResult>>(key);
                if (cacheValue == null)
                {
                    var args = new AuditConfigArgs
                    {
                        TtId = user.TTID,
                        Host = sysId,
                        DbName = dbInfo?.DbName,
                    };
                    lock (lockObject)
                    {
                        cacheValue = GlobalCache.Get<List<AuditConfigResult>>(key);
                        if (cacheValue == null)
                        {
                            var api = new Qt2Api(user);
                            cacheValue = ReTryTools.ReTry(
                                api.Call<AuditConfigArgs, List<AuditConfigResult>>,
                                "grantAuditService/GetAuditConfig",
                                args,6000);

                            //配置修正为统一小写格式化格式
                            cacheValue?.ForEach(x =>
                            {
                                x.DbName = x.DbName?.ToLower();
                                x.FieldName = x.FieldName?.Replace("_", "").ToLower();
                                x.TableName = x.TableName?.Replace("_", "").ToLower();
                            });

                            GlobalCache.Set<List<AuditConfigResult>>(key, cacheValue); //默认10分钟缓存
                        }
                    }
                }

                return cacheValue;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AuditTool.GetAuditConfig().Error");
                return new List<AuditConfigResult>();
            }
        }

        /// <summary>
        /// 根据数据库，表获取主键
        /// </summary>
        /// <param name="results">此处result已是ttid下的result</param>
        /// <param name="dbName"></param>
        /// <param name="tableName"></param>
        /// <returns>不返回null，排序按照列名自然排序</returns>
        internal static List<string> GetPrimaryKey(List<AuditConfigResult> results, string dbName, string tableName)
        {
            var list = results?.Where(x => x.DbName == dbName?.ToLower() && x.TableName == tableName?.ToLower() && x.IsPrimaryKey == 1)
                .OrderByDescending(x => x.FieldName?.Length).ToList(); //Grant 总结
            var fields = list?.Select(x => x.FieldName).ToList();
            return fields ?? new List<string>();
        }

        /// <summary>
        /// 获取连接主键
        /// </summary>
        /// <param name="pks"></param>
        /// <param name="properties"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static string GetPrimaryValue(List<string> pks, List<Z.EntityFramework.Plus.AuditEntryProperty> properties, object entity)
        {
            StringBuilder sb = new StringBuilder();
            pks?.ForEach(x =>
            {
                var val = properties?.FirstOrDefault(y => x ==y.PropertyName?.ToLower())?.NewValueFormatted;
                if(val == null)
                { //如果没找到主键，则通过反射找
                    val = GetObjectProperty(entity, x);
                }
                sb.AppendFormat("{0},", val ?? string.Empty);
            });
            if (sb.Length > 0)
            {
                return sb.ToString(0, sb.Length - 1);
            }
            else
            {
                return sb.ToString();
            }
        }

        /// <summary>
        /// 获取指定表的审计列
        /// </summary>
        /// <param name="results"></param>
        /// <param name="dbName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal static List<string> GetAuditColumns(List<AuditConfigResult> results, string dbName, string tableName)
        {
            var list = results?.Where(x => x.DbName == dbName?.ToLower() &&
                    (x.TableName == tableName?.ToLower()) && x.IsAudit == 1).ToList();
            return list?.Select(x => x.FieldName?.ToLower()).ToList() ?? new List<string>();
        }

        /// <summary>
        /// 根据审计信息组织新审计数据，按审计配置过滤
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user"></param>
        /// <param name="audit"></param>
        /// <param name="dbInfo"></param>
        /// <returns></returns>
        internal static List<AuditData> AuditConvertToArgs(DatabaseFacade db, UserContext user, Z.EntityFramework.Plus.Audit audit, DbInfo dbInfo)
        {
            var id = Guid.NewGuid().ToString("N");//LogEx.GetTransactionId();
            var cfgs = GetAuditConfig(user, dbInfo);
            var dbName = db.GetDbConnection().Database;


            List<AuditData> list = new List<AuditData>();
            if(cfgs == null || cfgs.Count <= 0)
            {
                return list;
            }

            audit?.Entries.ForEach(x =>
            {
                var auditColumns = GetAuditColumns(cfgs, dbName, x.EntityTypeName);
                // 增加判断空
                if(x!= null && auditColumns != null && auditColumns.Count > 0)
                {
                    var hasAuditColumn = x.Properties?.Any(o=>auditColumns.Contains(o?.PropertyName?.ToLower())) ?? false;
                    if (hasAuditColumn) //有审计列则do
                    {
                        AuditData data = new AuditData()
                        {
                            TableName = x.EntityTypeName,
                            AuditId = Guid.NewGuid().ToString("N"),
                            IsSql = false,
                            StateName = x.StateName,
                            TransactionId = id,
                            CreatedDate = DateTime.Now,
                            Ttid = user.TTID,
                            UserId = user.LoginName,
                            SysId = cfgs[0].SysId,
                            PrimarkKey = GetPrimaryValue(GetPrimaryKey(cfgs, dbName, x.EntityTypeName), x.Properties,x.Entity)
                        };
                        x.Properties?.ForEach(y =>
                        {
                            if (auditColumns.Contains(y.PropertyName?.ToLower()))
                            {                            
                                var p = GetAuditDataLine(y, data.AuditId);
                                if (p != null)
                                {
                                    data.Properties.Add(p);
                                }
                            }
                        });

                        //过滤新旧值都相同的记录，产生的原因是 某些字段未在审计配置中，但是按审计配置 无法剔除主键
                        if (x.State == Z.EntityFramework.Plus.AuditEntryState.EntityModified &&
                        (data.Properties?.All(o => o.NewValueFormatted == o.OldValueFormatted) ?? true))
                        {
                            //判断有点难懂，留空
                        }
                        else
                        {
                            list.Add(data);
                        }
                    }

                }
            });
            return list;
        }

        /// <summary>
        /// 转换格式化
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="auditId"></param>
        /// <returns></returns>
        private static AuditDataLine GetAuditDataLine(Z.EntityFramework.Plus.AuditEntryProperty prop, string auditId)
        {
            if (prop == null)
                return null;

            var p = new AuditDataLine
            {
                PropertyName = prop.PropertyName,
                NewValueFormatted = prop.NewValueFormatted,
                OldValueFormatted = prop.OldValueFormatted,
                AuditId = auditId
            };

            // 格式化数据，日期、浮点数字、整数
            FormatDataByType(prop, ref p);
            return p;
        }

        private static void FormatDataByType(Z.EntityFramework.Plus.AuditEntryProperty prop, ref AuditDataLine line)
        {           
            try
            {
                var t = GetType(prop?.OldValue) ?? GetType(prop?.NewValue);
                if (t == null) return;

                //1.处理日期
                if (t == typeof(DateTime))
                {

                    if (prop?.OldValue == null)
                    {
                        line.OldValueFormatted = string.Empty;
                    }
                    else
                    {
                        line.OldValueFormatted = string.Format("{0:yyyy/MM/dd HH:mm:ss}", prop.OldValue);
                    }

                    if (prop?.NewValue == null)
                    {
                        line.NewValueFormatted = string.Empty;
                    }
                    else
                    {
                        line.NewValueFormatted = string.Format("{0:yyyy/MM/dd HH:mm:ss}", prop.NewValue);
                    }
                }

                //2.处理浮点数字
                if (t == typeof(decimal) || t == typeof(double) || t == typeof(float))
                {
                    if (prop?.OldValue == null)
                    {
                        line.OldValueFormatted = string.Empty;
                    }
                    else
                    {
                        line.OldValueFormatted = string.Format("{0:#0.######}", prop.OldValue);
                    }

                    if (prop?.NewValue == null)
                    {
                        line.NewValueFormatted = string.Empty;
                    }
                    else
                    {
                        line.NewValueFormatted = string.Format("{0:#0.######}", prop.NewValue);
                    }
                }
                //3.处理整数
                if (t == typeof(int) || t == typeof(long) || t == typeof(byte))
                {
                    if (prop?.OldValue == null)
                    {
                        line.OldValueFormatted = string.Empty;
                    }
                    else
                    {
                        line.OldValueFormatted = prop.OldValue.ToString();
                    }

                    if (prop?.NewValue == null)
                    {
                        line.NewValueFormatted = string.Empty;
                    }
                    else
                    {
                        line.NewValueFormatted = prop.NewValue.ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AuditTool.FormatDataByType().Error");
            }            
        }

        /// <summary>
        /// 判断类型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

        private static Type[] _myTypes = {typeof(int),typeof(long), typeof(byte), typeof(DateTime), typeof(double), typeof(float), typeof(decimal) };
        /// <summary>
        /// 得到数据模型基本类型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static Type GetType(object obj)
        {
            if (obj == null) return null;

            var type = obj.GetType();
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = type.GetGenericArguments().First();
                }
                else
                {
                    //dont deal
                    return null;
                }
            }

            var t = _myTypes.FirstOrDefault(x => x.Name == type.Name);
            return t;           
        }
        /// <summary>
        /// 获取属性的所有名称
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static List<string> GetPropertyNames(object obj)
        {
            return obj?.GetType().GetProperties().Select(x => x.Name).ToList() ?? new List<string>();
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetObjectProperty(object obj,string name)
        {
            var list = GetPropertyNames(obj);
            var pn = list.FirstOrDefault(x => x.ToLower() == name?.ToLower());
            if(pn != null)
            {
                return ReflectionTool.GetPropertyValue(pn, obj)?.ToString();
            }
            return null;
        }
        #endregion

    } 
    
    
}
