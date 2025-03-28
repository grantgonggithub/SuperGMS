/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.UserSession
 文件名：SystemInfo
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:59:07

 功能描述：

----------------------------------------------------------------*/
namespace SuperGMS.UserSession
{
    /// <summary>
    ///
    /// <see cref="SystemInfo" langword="" />
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// 系统Id
        /// </summary>
        public string SysId { get; set; }
        /// <summary>
        /// 系统名称
        /// </summary>
        public string SysName { get; set; }
        /// <summary>
        /// 菜单显示在那种类型的终端上
        /// </summary>
        public string PlatFormType { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int SortNo { get; set; }
        /// <summary>
        /// 系统图片
        /// </summary>
        public string DefaultIcon { get; set; }
        /// <summary>
        /// 系统首页地址
        /// </summary>
        public string DefaultUrl { get; set; }
    }
}
