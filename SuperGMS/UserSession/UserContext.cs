//
// 文件：UserContext.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SuperGMS.Cache;
using SuperGMS.Config;
using SuperGMS.ExceptionEx;
using SuperGMS.Log;

namespace SuperGMS.UserSession
{
    /// <summary>
    ///     用户上下文类，获取，设置，检查 用户信息
    /// </summary>
    /// <exception cref="FrameworkException">用户未登录</exception>
    public class UserContext
    {
        // 共享互斥对象
        private readonly object rootLock = new object();

        private readonly static ILogger logger = LogFactory.CreateLogger<UserContext>();

        private List<UserSysInfo> sysInfo;

        private List<string> roleList;

        /// <summary>
        ///     Gets or sets 用户登录后分配的Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        ///   Gets or sets  客户端渠道类型Key
        /// </summary>
        public string ClientType { get; set; }

        /// <summary>
        ///   Gets or sets  客户端版本号
        /// </summary>
        public string ClientVersion { get; set; }

        /// <summary>
        ///   Gets or sets  Token过期时间
        /// </summary>
        public DateTime ExpireTime { get; set; }

        /// <summary>
        ///  Gets or sets   Token请求授权IP
        /// </summary>
        public string AccessIpAddress { get; set; }

        /// <summary>
        ///     Gets or sets token建立日期
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        ///  Gets or sets   租户ID
        /// </summary>
        public string TTID { get; set; }

        /// <summary>
        ///  Gets or sets   用户唯一ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        ///  Gets or sets   用户登录账号
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        ///   Gets or sets  用户语言设置
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        ///     判断当前用户有没有访问当前接口的权限
        /// </summary>
        /// <param name="funName">当前接口名称</param>
        /// <returns>是否有权限</returns>
        public bool HavRights(string funName)
        {
            var roleFunctionList = GetRpcRoleFunctionList();

            if (roleFunctionList == null)
            {
                return false;
            }

            return roleFunctionList.Exists(x =>
                x.FunctionList.Exists(
                    y => (string.Compare(y?.ServiceName, ServerSetting.AppName, StringComparison.OrdinalIgnoreCase) == 0) &&
                         (string.Compare(y?.ApiName, funName, StringComparison.OrdinalIgnoreCase) == 0)));
        }

        /// <summary>
        /// 拉取用户的子系统权限，包含数据库信息
        /// </summary>
        /// <returns>用户子系统信息 </returns>
        public List<UserSysInfo> GetSysInfo()
        {
            try
            {
                if (sysInfo==null)
                {
                    lock (rootLock)
                    {
                        if (sysInfo == null)
                        {
                            sysInfo = CacheManager.Get<List<UserSysInfo>>($"{Token}.SysInfo");
                        }
                    }
                }
                return sysInfo;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"UserContext.GetUserSysInfo.Error.{Token}");
                return new List<UserSysInfo>();
            }
        }

        /// <summary>
        ///     拉去用户的角色列表
        /// </summary>
        /// <returns>角色集合</returns>
        public List<string> GetRoleList()
        {
            try
            {
                if (roleList == null)
                {
                    lock (rootLock)
                    {
                        if (roleList == null)
                        {
                            roleList = CacheManager.Get<List<string>>($"{Token}.RoleList");
                        }
                    }
                }
                return roleList;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"UserContext.GetRoleList.Error.{Token}");
                return new List<string>();
            }
        }

        /// <summary>
        ///     根据用户的角色集合，拉取所有的功能列表
        ///     从Redis 获取,  grantCloud Function_List, ex_function_list 表 增加 RpcRoleFunction, RoleMange 的rolefunction 增加
        ///     RpcRoleFunction, rolemanage 初始化的时候把缓存写入到Redis, Key 为 TTID+ RoleID
        /// </summary>
        /// <returns>角色权限</returns>
        public List<RoleFunction> GetRpcRoleFunctionList()
        {
            var roleList = GetRoleList();
            if (roleList == null)
            {
                return new List<RoleFunction>();
            }

            try
            {
                var funs = CacheManager.Get<List<RoleFunction>>($"{Token}.FunctionList");
                return funs;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"UserContext.GetRpcRoleFunctionList.Error.{Token}");
                return new List<RoleFunction>();
            }
        }


        /// <summary>
        ///     這個方法只能框架里用，外面看不到，框架的RpcContext通過此方法初始化UserContext
        /// </summary>
        /// <param name="token">token</param>
        /// <returns>用户上下文</returns>
        internal static UserContext GetUserContext(string token)
        {
            try
            {
                return CacheManager.Get<UserContext>(token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"UserContext.GetUserContext.Error.{token}");
                return null;
            }
        }
    }
}