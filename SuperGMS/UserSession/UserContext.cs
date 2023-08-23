//
// 文件：UserContext.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

using NPOI.SS.Formula;
using NPOI.SS.Util;

using org.apache.zookeeper.data;

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

        /// <summary>
        /// 用户自定义的上下文信息
        /// </summary>
        public object UserObject { get; set; }

        /// <summary>
        /// 获取用户自定义上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetUserObject<T>() where T:class,new()
        {
            if (UserObject == null) return default(T);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(UserObject.ToString());
        }

        /// <summary>
        /// 登录用户信息
        /// </summary>
        public UserInfo UserInfo { get; set; }

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
        ///     判断当前用户有没有访问当前接口的权限
        /// </summary>
        /// <param name="funName">当前接口名称</param>
        /// <param name="serviceName"></param>
        /// <returns>是否有权限</returns>
        public bool HavRights(string funName,string serviceName="")
        {
            var roleFunctionList = GetRoleFunctionList();

            if (roleFunctionList == null)
            {
                return false;
            }
            return roleFunctionList.Any(x => string.Compare(x?.ServiceName, string.IsNullOrEmpty(serviceName)? ServerSetting.AppName:serviceName,StringComparison.OrdinalIgnoreCase)==0 &&
            string.Compare(x?.ApiName,funName,StringComparison.OrdinalIgnoreCase)==0);
        }

        /// <summary>
        /// 拉取用户的子系统权限，包含数据库信息
        /// </summary>
        /// <returns>用户子系统信息 </returns>
        public List<SysDbInfo> GetSysInfo()
        {
            return UserInfo?.SysDbInfos;
        }

        /// <summary>
        ///     拉去用户的角色列表
        /// </summary>
        /// <returns>角色集合</returns>
        public List<Role> GetRoleList()
        {
            return UserInfo?.Roles;
        }

        /// <summary>
        /// 功能按钮列表 ，这个量可能比较大，所以分开存
        /// </summary>
        /// <returns>角色权限</returns>
        public List<FunctionInfo> GetRoleFunctionList()
        {
            if (UserInfo.FunctionInfos == null)
            {
                lock (rootLock)
                {
                    if (UserInfo.FunctionInfos == null)
                    {
                        try
                        {
                            UserInfo.FunctionInfos = CacheManager.Get<List<FunctionInfo>>(GetRoleFunctionKey(Token));
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"UserContext.GetRoleFunctionList.Error.{Token}");
                            return new List<FunctionInfo>();
                        }                    
                    }
                }
            }
            return UserInfo.FunctionInfos;
        }

        /// <summary>
        /// 获取functionkey用于登录设置和获取function
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string GetRoleFunctionKey(string token)
        {
            return $"{token}.functionlist";
        }
        /// <summary>
        /// 获取Menukey用于登录设置和获取Menu
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string GetRoleMenuKey(string token)
        {
            return $"{token}.menulist";
        }

        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <returns></returns>
        public List<Menu> GetRoleMenuList()
        {
            if (UserInfo.Menus == null)
            {
                lock (rootLock)
                {
                    if (UserInfo.Menus == null)
                    {
                        try
                        {
                            UserInfo.Menus = CacheManager.Get<List<Menu>>(GetRoleMenuKey(Token));
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"UserContext.GetRoleMenuList.Error.{Token}");
                            return new List<Menu>();
                        }
                    }
                }
            }
            return UserInfo.Menus;
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