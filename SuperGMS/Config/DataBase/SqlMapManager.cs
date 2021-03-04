/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Config.Models.DataBase
 文件名：  SqlMapManager
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/31 14:13:16

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SuperGMS.Log;

namespace SuperGMS.Config
{
    /// <summary>
    /// SqlMapManager ,sqlMap单独一个管理类，主要是不想和DbModelContext共享一个lock，影响并发性能
    /// </summary>
    internal class SqlMapManager
    {
        private static Dictionary<string, string> sqlMapDictionary = new Dictionary<string, string>();
        private static ReaderWriterLock readerWriterLock = new ReaderWriterLock();
        private readonly static ILogger logger = LogFactory.CreateLogger<SqlMapManager>();
        /// <summary>
        /// 初始化sql文件
        /// </summary>
        /// <param name="sqlMapElement">sql 配置xml</param>
        public static void Initlize(XElement sqlMapElement)
        {
            BuildSqlMap(sqlMapElement);
        }

        /// <summary>
        /// 获取一个sql
        /// </summary>
        /// <param name="dbModelContextName">dbModelContextName</param>
        /// <param name="sqlKey">key</param>
        /// <returns>sql</returns>
        public static string GetSql(string dbModelContextName, string sqlKey)
        {
            readerWriterLock.AcquireReaderLock(100);
            string key = GetSqlKey(dbModelContextName, sqlKey);
            try
            {
                if (sqlMapDictionary.ContainsKey(key))
                {
                    return sqlMapDictionary[key];
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "SqlMapManager.Getsql.Error");
            }
            finally
            {
                if (readerWriterLock.IsReaderLockHeld)
                {
                    readerWriterLock.ReleaseReaderLock();
                }
            }
            logger.LogWarning($"SqlMapManager.Getsql.Error.keyNotFound:{key}");
            throw new Exception($"执行数据库操作时，在配置中没有找到{dbModelContextName}中的Key[{sqlKey}]值");
        }

        /// <summary>
        /// 通过配置文件把sql 以key value的方式放到内存中
        /// </summary>
        /// <param name="sqlInfo">配置文件</param>
        private static void BuildSqlMap(XElement sqlInfo)
        {
            var sqlMapX = sqlInfo?.Elements()?.ToArray();
            if (sqlMapX == null || !sqlMapX.Any())
            {
                logger.LogWarning("SqlMapManager.BuildSqlMap.Error.sqlMapX=null");
                return; // 只记录日志不报错，因为有可能确实没有sql
            }

            foreach (var item in sqlMapX)
            {
                var sqlFiles = item.Elements().ToArray();
                if (!sqlFiles.Any())
                {
                    continue;
                }
                foreach (var file in sqlFiles)
                {
                    if (file.Name != "SqlFile") { // 脚本直接写在sqlmap中
                        if(!string.IsNullOrEmpty(file.Value))
                          AddSqlMap(item.Name.ToString(), file.Name.ToString(), file.Value);
                        continue;
                    };
                    if (string.IsNullOrEmpty(file.Value)) continue;
                    var filePath = $"{AppContext.BaseDirectory}config{Path.DirectorySeparatorChar}{item.Name}{Path.DirectorySeparatorChar}{file.Value}";
                    if (!File.Exists(filePath))
                    {
                        filePath = $"{AppContext.BaseDirectory}config{Path.DirectorySeparatorChar}{item.Name}{Path.DirectorySeparatorChar}{file.Value}";
                        if(!File.Exists(filePath))
                           throw new Exception($"未找到路径{filePath}中的SQL配置文件");
                    }
                    try
                    {
                        var sqlConfig = XElement.Parse(Tools.FileHelper.ReadFile(filePath));
                        if(sqlConfig.Name != "SqlCommand")
                        {
                            throw new Exception($"未找到<SqlCommand>的根节点");
                        }
                        var sqls = sqlConfig?.Elements()?.ToArray();
                        if (sqls != null)
                        {
                            foreach (var sql in sqls)
                            {
                                AddSqlMap(item.Name.ToString(), sql.Name.ToString(), sql.Value);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, $"DbModelContextManager.Initlize.ReadFile.Error{e.Message}");
                        throw new Exception($"解析SQL配置文件{filePath}异常", e);
                    }
                }
            }
        }

        /// <summary>
        /// 组织一个sqlKey
        /// </summary>
        /// <param name="dbModelContextName">dbModelContextName</param>
        /// <param name="sqlMapKey">sqlMapKey</param>
        /// <returns>key</returns>
        private static string GetSqlKey(string dbModelContextName, string sqlMapKey)
        {
            return $"{dbModelContextName.ToLower().Trim()}_{sqlMapKey.ToLower().Trim()}";
        }

        private static void AddSqlMap(string dbModelCtxName, string sqlMapkey, string sql)
        {
            readerWriterLock.AcquireWriterLock(100);
            try
            {
                sql = sql.Trim();
                string key = GetSqlKey(dbModelCtxName, sqlMapkey);
                if (sqlMapDictionary.ContainsKey(key))
                {
                    throw new Exception($"加载SQL配置失败,存在重复Key:{key}");
                }
                else
                {
                    sqlMapDictionary.Add(key, sql);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "SqlMapManager.AddSqlMap.Error");
                throw e;
            }
            finally
            {
                if (readerWriterLock.IsWriterLockHeld)
                {
                    readerWriterLock.ReleaseWriterLock();
                }
            }
        }
    }
}