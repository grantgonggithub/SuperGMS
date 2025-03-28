/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.UserSession.UserLoginInfo
 文件名：Rolefunction
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:59:07

 功能描述：

----------------------------------------------------------------*/
namespace SuperGMS.UserSession.UserLoginInfo
{
    /// <summary>
    ///
    /// <see cref="Rolefunction" langword="" />
    /// </summary>
    public class Rolefunction
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        public int RoleId { get; set; }
        /// <summary>
        /// 功能按钮列
        /// </summary>
        public int FunctionId { get; set; }
        /// <summary>
        /// 菜单Id
        /// </summary>
        public string MenuId { get; set; }
        /// <summary>
        /// 系统Id
        /// </summary>
        public string SysId { get; set; }
    }
}
