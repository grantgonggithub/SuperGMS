using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.UserSession
{
    /// <summary>
    /// 用户上下文扩展
    /// </summary>
    public class UserContextEx
    {
        public UserContextEx()
        {
            AllDomains = new List<string>();
        }

        /// <summary>
        ///     邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     邮箱
        /// </summary>
        public string QQ { get; set; }

        /// <summary>
        ///     是否系统用户
        /// </summary>
        public bool IsSys { get; set; }

        /// <summary>
        ///     手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        ///     租户Name
        /// </summary>
        public string TtName { get; set; }

        /// <summary>
        ///     用户域
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        ///     仓库ID
        /// </summary>
        public string Whid { get; set; }

        /// <summary>
        ///     日志数据库连接
        /// </summary>
        public string LogDbConn { get; set; }

        /// <summary>
        ///     消息数据库连接
        /// </summary>
        public string MessageDbConn { get; set; }

        /// <summary>
        ///     是否启用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        ///     皮肤
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        ///     ApiKey
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        ///     最近一次登录日期
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        ///     用户自定义1
        /// </summary>
        public string Udf1 { get; set; }

        /// <summary>
        ///     用户自定义2
        /// </summary>
        public string Udf2 { get; set; }

        /// <summary>
        ///     用户自定义3
        /// </summary>
        public string Udf3 { get; set; }

        /// <summary>
        ///     用户自定义4
        /// </summary>
        public string Udf4 { get; set; }

        /// <summary>
        ///     用户自定义5
        /// </summary>
        public string Udf5 { get; set; }

        /// <summary>
        ///     允许APP登录
        /// </summary>
        public string IsAllowApp { get; set; }

        /// <summary>
        ///     角色ID
        /// </summary>
        public string DefaultRoleId { get; set; }

        public List<string> AllDomains { get; set; }
    }
}
