using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.UserSession
{
    /// <summary>
    /// 角色功能权限
    /// </summary>
    public class RoleFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleFunction"/> class.
        /// 构造
        /// </summary>
        public RoleFunction()
        {
            FunctionList = new List<FunctionInfo>();
        }

        /// <summary>
        /// Gets or Sets 角色id
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or Sets 功能名称
        /// </summary>
        public List<FunctionInfo> FunctionList { get; set; }
    }
}
