/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.UserSession
 文件名：Menu
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
    /// <see cref="Menu" langword="" />
    /// </summary>
   public class Menu
    {
        /// <summary>
        /// 系统Id
        /// </summary>
        public string SysId { get; set; }
        /// <summary>
        /// 菜单Id
        /// </summary>
        public string MenuId { get; set; }
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string MenuName { get; set; }
        /// <summary>
        /// 父菜单Id
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public string SortNo { get; set; }

        /// <summary>
        /// 菜单下的按钮列表， 便于查询，这里不构造树结构
        /// </summary>
        //public List<FunctionInfo> Functions { get; set; }

    }
}
