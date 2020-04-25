/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.DB.EFEx
 文件名：  GrantDBContext
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/3 0:43:47

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using GrantMicroService.Config;
using GrantMicroService.DB.EFEx.GrantDbContext;
using GrantMicroService.DB.EFEx.GrantDbFactory;
using GrantMicroService.ExceptionEx;
using GrantMicroService.Log;
using GrantMicroService.Protocol.RpcProtocol;
using Z.EntityFramework.Plus;
using GrantMicroService.Rpc.Server;

namespace GrantMicroService.DB.EFEx
{
    public delegate void DbDapperCommit(GrantDBConnection GrantDBConnection, SqlPara auditSql);
    public delegate void DbEFCommit(DbContext dbContext, Z.EntityFramework.Plus.Audit audit);
    public delegate void QtDapperCommit(RpcContext rpcContext, GrantDBConnection GrantDBConnection, DbInfo dbInfo, SqlPara auditSql);
    public delegate void QtEFCommit(RpcContext rpcContext, DbContext dbContext, DbInfo dbInfo, Z.EntityFramework.Plus.Audit audit);
    /// <summary>
    /// GrantDBContext
    /// </summary>
    public class GrantDBContext
    {
        public static event QtDapperCommit OnQtDapperCommit;
        public static event QtEFCommit OnQtEFCommit;
        private readonly static ILogger logger = LogFactory.CreateLogger<GrantDBContext>();

        private static DbInfo GetDbInfo(RpcContext rpcContext, string dbName)
        {
            try
            {
                // 先找用户私有化的配置
                var sysInfos = rpcContext.GetUserContext()?.GetSysInfo();

                if (sysInfos == null || !sysInfos.Any())
                {
                    if (!string.IsNullOrEmpty(rpcContext.Args.tk))
                        logger.LogWarning("GrantDBContext.GetDbInfo.GetUserCtx.sysInfos==null");
                }

                var dbInfos = sysInfos?.Where(a => a.DbModelName?.ToLower() == dbName).ToList();
                ConstItem dbValue = null;
                if (dbInfos == null || !dbInfos.Any())
                {
                    // 从配置文件做了兼容转换，等数据库tenant_database的DBModelName字段启用后就直接能取到了
                    dbValue = ServerSetting.GetConstValue(dbName);
                    if (dbValue != null)
                    {
                        var sysId = dbValue.Value.ToLower();
                        dbInfos = sysInfos.Where(a => a.SysID?.ToLower() == sysId && !string.IsNullOrEmpty(a.DBIP)).ToList();
                    }
                }
                DbInfo info = null;
                if (dbInfos != null && dbInfos.Any())
                {
                    var dbInfo = dbInfos.First();
                    var dbType = dbInfo.DBType;
                    DbType dType = DbTypeParser.Parser(dbType);
                    info = new DbInfo()
                    {
                        DbType = dType,
                        DbName = dbInfo.DBName,
                        Ip = dbInfo.DBIP,
                        Port = dbInfo.DbPort,
                        UserName = dbInfo.DBUser,
                        Pwd = dbInfo.DBPwd,
                        DbContextName = dbName,
                    };
                }
                else
                {
                    // 如果私有化的表没有，找全局的表
                    var dbModel = ServerSetting.GetDbModelContext(dbName);
                    if (dbModel == null)
                    {
                        throw new Exception($"DataModel Info :{dbName} Is Not Found");
                    }

                    DbType dType = DbTypeParser.Parser(dbModel.DbType);

                    // 需要根据接口配置的主从来选择主从，这里先暂时全部取主，有空了在完善接口主从配置
                    info = new DbInfo()
                    {
                        DbName = dbModel.Database,
                        DbType = dType,
                        Ip = dbModel.Master.Ip,
                        Port = dbModel.Master.Port,
                        UserName = dbModel.UserName,
                        Pwd = dbModel.PassWord,
                        DbContextName = dbName,
                    };
                }
                logger.LogDebug($"获取的数据库信息是：{info?.ToString()}");
                return info;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, $"GrantDBContext.GetDbInfo.Error,获取数据库{dbName}连接信息异常");
                throw new BusinessException(new StatusCode(StatusCode.ServerError.code, $"无法找到用户的数据库信息，请检查租户数据库，或者Config目录下的数据库配置信息({dbName})"));
            }
        }

        public static IGrantEFDbContext GetEFContext<TContext>(RpcContext rpcContext)
            where TContext : DbContext
        {
            var dbName = typeof(TContext).Name.ToLower();
            var info = DBContextOptionsLoader.IsMemoryDb ? new DbInfo() : GetDbInfo(rpcContext, dbName);
            var options = DBContextOptionsLoader.CreateDbOption<TContext>(info);

            if (ServerSetting.GetConstValue("TrackSql")?.Value.ToLower() == "true")
            {
                options.UseLoggerFactory(LogFactory.LoggerFactory);
            }
            var dbContext = (TContext)Activator.CreateInstance(typeof(TContext), options.Options);
            return new GrantEFDbContext(dbContext, info);
            //if (OnQtEFCommit != null)
            //{
            //    ctx.OnDbCommit += (DbContext dbCtx, Z.EntityFramework.Plus.Audit audit) => {
            //        OnQtEFCommit.Invoke(rpcContext, dbCtx, info, audit);
            //    };
            //}
            //return ctx;
        }

        // public static readonly LoggerFactory MyLoggerFactory
        //    = new LoggerFactory(new[]
        //    {
        //        new ConsoleLoggerProvider((category, level) => true, true),
        //    });
        public static IGrantDapperDbContext GetDapperContext(RpcContext rpcContext, string dbContextName)
        {
            var dbName = dbContextName.ToLower();
            var info = GetDbInfo(rpcContext, dbName);
            return new GrantDapperDBContext(info);
            //if (OnQtDapperCommit != null)
            //{
            //    ctx.OnDbCommit += (GrantDBConnection GrantDBConnection, SqlPara auditSql) => {
            //        OnQtDapperCommit.Invoke(rpcContext, GrantDBConnection, info, auditSql);
            //    };
            //}
            //return ctx;
        }
    }
}