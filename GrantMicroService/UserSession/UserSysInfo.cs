using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.UserSession
{
    /// <summary>
    /// 用户子系统权限，数据库信息
    /// </summary>
    public class UserSysInfo
    {
        /// <summary>
        /// Gets or sets 扩展系统ID
        /// </summary>
        public string ExSysID { get; set; }

        /// <summary>
        /// Gets or sets 系统ID
        /// </summary>
        public string SysID { get; set; }

        /// <summary>
        /// Gets or sets 系统ID
        /// </summary>
        public string ExSysName { get; set; }

        /// <summary>
        /// Gets or sets 系统地址
        /// </summary>
        public string SysUrl { get; set; }

        /// <summary>
        /// Gets or sets 系统图标
        /// </summary>
        public string SysMICon { get; set; }

        /// <summary>
        /// Gets or sets 数据库User
        /// </summary>
        public string DBUser { get; set; }

        /// <summary>
        /// Gets or sets 数据库Pwd
        /// </summary>
        public string DBPwd { get; set; }

        /// <summary>
        /// Gets or sets 数据库Name
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// Gets or sets 数据库地址
        /// </summary>
        public string DBIP { get; set; }

        /// <summary>
        /// Gets or sets 数据库类型
        /// </summary>
        public string DBType { get; set; }

        /// <summary>
        /// Gets or sets 排序字段
        /// </summary>
        public int SortID { get; set; }

        /// <summary>
        /// Gets or sets 跟dbcontext对应数据库模型名称，用于获取dbcontext的连接信息
        /// </summary>
        public string DbModelName { get; set; }

        /// <summary>
        /// Gets or sets 数据库端口号
        /// </summary>
        public int DbPort { get; set; }
    }
}
