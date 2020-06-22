/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx
 文件名：  InitlizeAudit
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/11/12 17:06:28

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SuperGMS.Rpc.Server;
using SuperGMS.Audit;
using SuperGMS.Config;
using Z.EntityFramework.Plus;
using SuperGMS.Audit.Model;
using Microsoft.Extensions.Logging;
using SuperGMS.Log;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// InitlizeAudit
    /// </summary>
    [InitlizeMethod()]
    public class InitlizeAudit
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<InitlizeAudit>();
        [InitlizeMethod()]
        public static void Init()
        {
            var bAudit = ServerSetting.GetConstValue("IsAudit")?.Value ?? "true";
            if (bAudit.ToLower() == "true")
            {
                GrantDBContext.OnQtDapperCommit += GrantDBContext_OnQtDapperCommit;
                GrantDBContext.OnQtEFCommit += GrantDBContext_OnQtEFCommit;
            }
            
            //初始化过滤审计内容
            AuditManager.DefaultConfiguration.IgnoreRelationshipAdded = true;
            AuditManager.DefaultConfiguration.IgnoreEntityAdded = true;
            AuditManager.DefaultConfiguration.IgnoreEntitySoftAdded = true;
            AuditManager.DefaultConfiguration.IgnoreEntitySoftDeleted = true;           

        }

        private static void GrantDBContext_OnQtEFCommit(RpcContext rpcContext, Microsoft.EntityFrameworkCore.DbContext dbContext, DbInfo dbInfo, Z.EntityFramework.Plus.Audit audit)
        {
            var user = rpcContext.GetUserContext();
            if (haveAudit(user))
            {
                if (audit?.Entries == null)
                {
                    return;
                }

                var auditData = new List<AuditData>();
                try
                {
                    audit.Entries = audit.Entries.Where(x =>
                        x.State == AuditEntryState.EntityModified ||
                        x.State == AuditEntryState.EntityDeleted).ToList();

                    //修正空审计记录
                    if (audit.Entries.Count() == 0)
                    {
                        return;
                    }
                    auditData = AuditTool.AuditConvertToArgs(dbContext.Database, user, audit, dbInfo);
                    auditData?.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(rpcContext.Args.rid))
                        {
                            x.TransactionId = rpcContext.Args.rid;
                        }
                    });
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "GrantDBContext_OnQtEFCommit error.");
                }
                AuditTool.SaveAuditData(dbContext.Database, auditData, dbInfo);
            }

        }

        private static void GrantDBContext_OnQtDapperCommit(RpcContext rpcContext, GrantDbFactory.GrantDBConnection GrantDBConnection, DbInfo dbInfo, EFEx.GrantDbFactory.SqlPara sqlPara)
        {
            return;
            var user = rpcContext.GetUserContext();
            if (haveAudit(user))
            {
                throw new NotImplementedException();
            }
        }

        private static bool haveAudit(UserSession.UserContext userContext)
        {
            if (userContext == null) return false;
            var sysInfo = userContext.GetSysInfo();
            if (sysInfo == null) return false;
            return sysInfo.Any(x => x.SysID.ToLower() == "audit");
        }
    }
}
