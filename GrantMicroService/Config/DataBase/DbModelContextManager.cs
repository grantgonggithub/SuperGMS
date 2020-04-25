/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Config.Models.DataBase
 文件名：  DbModelContextManager
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/22 11:53:11

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using NPOI.XSSF.Extractor;
using GrantMicroService.Log;

namespace GrantMicroService.Config
{
    /// <summary>
    /// DbModelContextManager
    /// </summary>
    internal class DbModelContextManager
    {
        /// <summary>
        /// 配置文件中数据库的跟配置节
        /// </summary>
        public const string DATABASE = "DataBase";

        /// <summary>
        /// 数据库连接信息配置节点
        /// </summary>
        private const string DATABASEINFO = "DataBaseInfo";

        /// <summary>
        /// sql脚本配置节点
        /// </summary>
        private const string SQLMAP = "SqlMap";

        /// <summary>
        /// 是否通过引用文件来配置的标记
        /// </summary>
        private const string REFFILE = "RefFile";

        private static ReaderWriterLock readerWriterLock = new ReaderWriterLock();

        private static Dictionary<string, DbModelContext> dbModelContexts = new Dictionary<string, DbModelContext>();

        private readonly static ILogger logger = LogFactory.CreateLogger<DbModelContextManager>();
        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="xml">xml</param>
        public static void Initlize(XElement xml)
        {
            if (xml == null || xml.Name.LocalName.ToLower() != DATABASE.ToLower())
            {
                logger.LogError("DbModelContextManager.Initlize.Error,本地没有数据库配置信息");
                return;
            }
            XElement dbRoot = xml;
            XAttribute refFile = dbRoot.Attribute(REFFILE);
            XElement dataBaseInfo = null;
            XElement sqlMap = null;
            if (refFile == null || refFile.Value == "false")
            {
                // 和grant.config在一个配置文件中,也有可能是zk里面拉过来的
                dataBaseInfo = dbRoot.Element(DATABASEINFO);
            }
            else
            {
                // 单独的配置文件
                string dbFile = dbRoot.Attribute(DATABASEINFO)?.Value?.Trim();
                if (string.IsNullOrEmpty(dbFile))
                {
                    logger.LogError($"数据库配置不正确，你指定了RefFile=true，所以需要提供{DATABASEINFO}");
                    return;
                }

                if (dbFile.ToLower().StartsWith("http://"))
                {
                    string dbPath = string.Empty;
                    try
                    {
                        string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");
                        if (!Directory.Exists(downloadPath))
                            Directory.CreateDirectory(downloadPath);
                        dbPath = Path.Combine(downloadPath, "DataBase.config");
                        using (var webClient = new WebClient())
                        {
                            var dataBytes = webClient.DownloadData(dbFile);
                            //保存下载的远端配置文件
                            var configStr = Encoding.UTF8.GetString(dataBytes);
                            using (var fileWriter = File.CreateText(dbPath))
                            {
                                fileWriter.Write(configStr);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"下载数据库配置DataBase.config异常:{dbFile}", e);
                    }
                    dbFile = dbPath;
                }
                dataBaseInfo = GetDataBase(dbFile);                
            }
            BulidDataBaseInfo(dataBaseInfo);
            SqlMapManager.Initlize(GetSqlMap());
        }
        /// <summary>
        /// 解析数据库配置文件内容到Xml
        /// </summary>
        /// <param name="dbFile">不包含文件路径时,则在应用的根目录下的Config文件夹中获取</param>
        /// <returns></returns>
        public static XElement GetDataBase(string dbFile)
        {
            var dbFilePath = dbFile;
            if (!Path.IsPathRooted(dbFilePath))
            {
                dbFilePath = $"{AppContext.BaseDirectory}Config{Path.DirectorySeparatorChar}{dbFile}";
                if (!File.Exists(dbFilePath))
                {
                    dbFilePath = $"{AppContext.BaseDirectory}config{Path.DirectorySeparatorChar}{dbFile}"; // 兼容大小写
                    if (!File.Exists(dbFilePath))
                    {
                        logger.LogInformation($"找不到配置文件{dbFilePath}");
                        throw new Exception($"找不到配置文件{dbFilePath}");
                    }
                }
            }

            try
            {
                string dbConfig = Tools.FileHelper.ReadFile(dbFilePath);
                return XElement.Parse(dbConfig);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"DbModelContextManager.Initlize.ReadFile.Error");
                throw new Exception($"解析配置文件异常{dbFilePath}",e);
                //return (null,null);
            }
        }
        public static XElement GetSqlMap()
        {
            var mapFilePath = $"{AppContext.BaseDirectory}Config{Path.DirectorySeparatorChar}MySql.config";
            if (!File.Exists(mapFilePath))
            {
                logger.LogInformation($"找不到配置文件{mapFilePath}");
                throw new Exception($"找不到配置文件{mapFilePath}");
            }
            try
            {
                string mapConfig = Tools.FileHelper.ReadFile(mapFilePath);
                return XElement.Parse(mapConfig);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"DbModelContextManager.Initlize.ReadFile.Error");
                throw new Exception($"解析配置文件异常{mapFilePath}", e);
                //return (null,null);
            }
        }
        /// <summary>
        /// 获取特定的数据库连接信息
        /// </summary>
        /// <param name="dbContextName">dbContextName</param>
        /// <returns>DbModelContext</returns>
        public static DbModelContext GetDbModelContext(string dbContextName)
        {
            readerWriterLock.AcquireReaderLock(100);
            try
            {
                if (dbModelContexts.ContainsKey(dbContextName.ToLower()))
                {
                    return dbModelContexts[dbContextName.ToLower()];
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "DbModelContextManager.GetDbModelContext.Error");
            }
            finally
            {
                if (readerWriterLock.IsReaderLockHeld)
                {
                    readerWriterLock.ReleaseReaderLock();
                }
            }

            return null;
        }

        private static void BulidDataBaseInfo(XElement dbInfo)
        {
            var dBContexts = dbInfo?.Elements()?.ToArray();
            if (dBContexts == null || !dBContexts.Any())
            {
                logger.LogInformation("BulidDataBaseInfo.dBContexts=null");
                return;
            }

            foreach (var item in dBContexts)
            {
                DbModelContext ctx = new DbModelContext();
                ctx.DbContextName = item.Name?.ToString();
                ctx.DbType = item.Attribute("DbType")?.Value;
                ctx.UserName = item.Attribute("UserName")?.Value;
                ctx.PassWord = item.Attribute("PassWord")?.Value;
                ctx.Database = item.Attribute("Database")?.Value;
                int p = 0;
                int.TryParse(item.Attribute("Pool")?.Value, out p);
                ctx.Pool = p;
                XElement master = item.Element("Master");
                if (master == null)
                {
                    logger.LogInformation("DbModelContextManager.BulidDataBaseInfo.Error.master=null");
                    continue; // master 一定不能为空
                }

                int port = 0;
                int.TryParse(master.Attribute("Port")?.Value, out port);
                ctx.Master = new Master() { Ip = master.Attribute("Ip")?.Value, Port = port };
                ctx.Slaves = new List<Slave>();
                XElement slave = item.Element("Slave");

                // 可以没有slave
                if (slave != null && slave.HasElements)
                {
                    XElement[] items = slave.Elements("Item")?.ToArray();
                    foreach (var slv in items)
                    {
                        int.TryParse(slv.Attribute("Pool")?.Value, out p);
                        int.TryParse(slv.Attribute("Port")?.Value, out port);
                        var s = new Slave() { Ip = slv.Attribute("Ip")?.Value, Pool = p, Port = port };
                        ctx.Slaves.Add(s);
                    }
                }

                AddDbModelContext(ctx);
            }
        }

        private static void AddDbModelContext(DbModelContext ctx)
        {
            readerWriterLock.AcquireWriterLock(100);
            try
            {
                if (dbModelContexts.ContainsKey(ctx.DbContextName.ToLower()))
                {
                    dbModelContexts[ctx.DbContextName.ToLower()] = ctx;
                }
                else
                {
                    dbModelContexts.Add(ctx.DbContextName.ToLower(), ctx);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "DbModelContextManager.AddDbModelContext.Error");
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