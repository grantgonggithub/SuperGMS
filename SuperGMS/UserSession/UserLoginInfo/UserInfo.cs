/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.UserSession
 文件名：UserInfo
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:59:07

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.UserSession
{
    /// <summary>
    ///
    /// <see cref="UserInfo" langword="" />
    /// </summary>
   public class UserInfo
    {
        /// <summary>
        /// 登录用户Id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 登录名
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 租户信息
        /// </summary>
        public TenantInfo TenantInfo { get; set; }
        /// <summary>
        /// 租户的系统数据库信息
        /// </summary>
        public List<SysDbInfo> SysDbInfos { get; set; }

        /// <summary>
        /// 租户系统信息
        /// </summary>
        public List<SystemInfo> SystemInfos { get; set; }


        /// <summary>
        /// 租户角色列表
        /// </summary>
        public List<Role> Roles { get; set; }

        /// <summary>
        /// 租户用户菜单
        /// </summary>
        internal List<Menu> Menus { get; set; }

        /// <summary>
        /// 租户功能按钮列表
        /// </summary>
        internal List<FunctionInfo> FunctionInfos { get; set; }
    }
}
