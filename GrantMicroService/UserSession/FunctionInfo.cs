using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.UserSession
{
    /// <summary>
    /// 功能列表信息
    /// </summary>
    public class FunctionInfo
    {
        /// <summary>
        /// Gets or Sets 微服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or Sets api名称
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// Gets or Sets 子系统Id
        /// </summary>
        public string SysId { get; set; }

        /// <summary>
        /// Gets or Sets    视图名称
        /// </summary>
        public string ViewName { get; set; }

        /// <summary>
        /// Gets or Sets    Controller名称
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        ///  Gets or Sets   按钮Name,ID
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///  Gets or Sets   按钮名称
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///  Gets or Sets   按钮单击事件,注意 右键菜单不需要括号, 普通按钮需要带上括号
        /// </summary>
        public string OnClick { get; set; }

        /// <summary>
        ///  Gets or Sets   快捷键
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        ///  Gets or Sets   是否验证权限，默认值为 False, 需要检查权限, 不验证可以设置为True
        /// </summary>
        public bool IsNotCheck { get; set; }

        /// <summary>
        ///  Gets or Sets   是否默认隐藏，点击更多则自动加载显示
        /// </summary>
        public bool IsDefaultHidden { get; set; }
    }
}
