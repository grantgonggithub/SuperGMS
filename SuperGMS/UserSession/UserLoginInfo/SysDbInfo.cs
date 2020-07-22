using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.UserSession
{
    /// <summary>
    /// 租户数据库信息
    /// </summary>
    public class SysDbInfo
    {

        /// <summary>
        /// Gets or sets 系统ID
        /// </summary>
        public string SysID { get; set; }


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
        /// Gets or sets 跟dbcontext对应数据库模型名称，用于获取dbcontext的连接信息
        /// </summary>
        public string DbModelName { get; set; }

        /// <summary>
        /// Gets or sets 数据库端口号
        /// </summary>
        public int DbPort { get; set; }
    }
}
