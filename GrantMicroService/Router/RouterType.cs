/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.Router
 文件名：RouterType
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/5/8 16:17:34

 功能描述：

----------------------------------------------------------------*/
using System;

namespace GrantMicroService.Router
{
   public enum RouterType
    {
        /// <summary>
        /// 一致性Hash 
        /// </summary>
        Hash=1,
        /// <summary>
        /// 随机
        /// </summary>
        Random=2,

        /// <summary>
        /// 轮询
        /// </summary>
        Polling=3,
        
    }

    /// <summary>
    /// 路由类型转换
    /// </summary>
    public class RouterTypeParse
    {
        /// <summary>
        /// 路由类型转换
        /// </summary>
        /// <param name="routerType"></param>
        /// <returns></returns>
        public static RouterType Parse(string routerType)
        {
            RouterType r = RouterType.Hash;
            if (Enum.TryParse<RouterType>(routerType, out r))
                r=RouterType.Hash;
            return r;
        }        
    }
}
