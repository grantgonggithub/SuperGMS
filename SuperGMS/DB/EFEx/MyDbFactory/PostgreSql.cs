/*----------------------------------------------------------------
Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

项目名称：SuperGMS.DB.EFEx.GrantDbFactory
文件名：PostgreSql.cs
创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
CLR版本：4.0.30319.42000
时间：2019/9/11 15:25:29

功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.DB.EFEx.GrantDbFactory
{
    /// <summary>
    ///
    /// <see cref="PostgreSql" langword="" />
    /// </summary>
    public class PostgreSql : DbBase
    {
        public PostgreSql(DbInfo dbInfo) : base(dbInfo)
        {
        }
        /// <summary>
        /// 参数前缀
        /// </summary>
        protected override string Prefix => ":";
    }
}