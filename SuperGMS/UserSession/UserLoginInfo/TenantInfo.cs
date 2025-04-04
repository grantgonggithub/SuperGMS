﻿/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.UserSession
 文件名：TenantInfo
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:59:07

 功能描述：

----------------------------------------------------------------*/
using System;

namespace SuperGMS.UserSession
{
    /// <summary>
    ///
    /// <see cref="TenantInfo" langword="" />
    /// </summary>
    public class TenantInfo
    {
        /// <summary>
        ///  Gets or sets   租户ID
        /// </summary>
        public int TTID { get; set; }

        /// <summary>
        /// 租户名称
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// 使用开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 租户需要扩展的信息
        /// </summary>
        public object TenantObjCtx { get; set; }

        /// <summary>
        /// 获取租户扩展信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetTenantObjCtx<T>() where T : class, new()
        {
            if (TenantObjCtx == null) return default(T);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(TenantObjCtx.ToString());
        }

    }
}
