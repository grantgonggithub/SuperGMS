using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.UserSession
{
    /// <summary>
    /// 功能列表信息
    /// </summary>
    public class FunctionInfo
    {
        /// <summary>
        /// 功能按钮Id
        /// </summary>
        public string FunctionId { get; set; }
        /// <summary>
        /// 功能按钮菜单
        /// </summary>
        public string FunctionName { get; set; }
        /// <summary>
        /// 操作类型 List/add/update/del/view
        /// </summary>
        public string ViewType { get; set; }
        /// <summary>
        /// 系统Id
        /// </summary>
        public string SysId { get; set; }
        /// <summary>
        /// 菜单Id
        /// </summary>
        public string MenuId { get; set; }
        /// <summary>
        /// 微服务名称
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// 接口名称
        /// </summary>
        public string ApiName { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int SortNo { get; set; }
    }
}
