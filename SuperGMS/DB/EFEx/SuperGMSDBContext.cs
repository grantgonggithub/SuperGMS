/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx
 文件名：  GrantDBContext
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/3 0:43:47

 功能描述：

----------------------------------------------------------------*/

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SuperGMS.Config;
using SuperGMS.DB.EFEx.GrantDbContext;
using SuperGMS.ExceptionEx;
using SuperGMS.Log;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc.Server;

using System;
using System.Linq;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// GrantDBContext
    /// </summary>
    public class SuperGMSDBContext
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<SuperGMSDBContext>();

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
                    info = ServerSetting.GetDbModelContext(dbName);
                    if (info == null)
                        throw new Exception($"DataModel Info :{dbName} Is Not Found");
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

        /// <summary>
        /// 根据登录用户上下文获取DbContext
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="rpcContext"></param>
        /// <param name="dbModelName"></param>
        /// <returns></returns>
        public static IEFDbContext GetEFContext<TContext>(RpcContext rpcContext,string dbModelName=null)
            where TContext : DbContext
        {
            var dbName = string.IsNullOrWhiteSpace(dbModelName) ? typeof(TContext).Name.ToLower() : dbModelName;
            var info = DBContextOptionsLoader.IsMemoryDb ? new DbInfo() : GetDbInfo(rpcContext, dbName);
            return GetEFDbContext<TContext>(info);
        }

        /// <summary>
        /// 直接通过DbInfo创建DbContext,用于非用户登录的上下文中
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="dbInfo">数据库连接信息</param>
        /// <returns></returns>
        public static IEFDbContext GetEFDbContext<TContext>(DbInfo dbInfo)
            where TContext : DbContext
        {
            if (dbInfo == null) throw new Exception("数据库连接信息DbInfo不能为空，请检查");
            var options = DBContextOptionsLoader.CreateDbOption<TContext>(dbInfo);
            if (ServerSetting.GetConstValue("TrackSql")?.Value.ToLower() == "true")
            {
                options.UseLoggerFactory(LogFactory.LoggerFactory);
                options.EnableSensitiveDataLogging();
            }
            var dbContext = (TContext)Activator.CreateInstance(typeof(TContext), options.Options);
            return new EFDbContext(dbContext, dbInfo);
        }

        /// <summary>
        /// 根据登录用户上下文获取DbContext
        /// </summary>
        /// <param name="rpcContext"></param>
        /// <param name="dbContextName"></param>
        /// <returns></returns>
        public static IDapperDbContext GetDapperContext(RpcContext rpcContext, string dbContextName)
        {
            var dbName = dbContextName.ToLower();
            var info = GetDbInfo(rpcContext, dbName);
            return GetDapperContext(info);
        }

        /// <summary>
        /// 直接通过DbInfo创建DbContext,用于非用户登录的上下文中
        /// </summary>
        /// <param name="dbInfo">数据库连接信息</param>
        /// <returns></returns>
        public static IDapperDbContext GetDapperContext(DbInfo dbInfo)
        {
            return new DapperDBContext(dbInfo);
        }
    }
}