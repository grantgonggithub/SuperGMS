/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.DB.EFEx.GrantDbFactory
 文件名：  MySql
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/31 21:37:39

 功能描述：

----------------------------------------------------------------*/


namespace GrantMicroService.DB.EFEx.GrantDbFactory
{
    /// <summary>
    /// MySql
    /// </summary>
    public class MySql : DbBase
    {
        /// <summary>
        /// Gets mysql的参数前缀
        /// </summary>
        protected override string Prefix
        {
            get { return "@"; }
        }

        public MySql(DbInfo dbInfo)
            : base(dbInfo)
        {
        }
    }
}