/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantRpc.Server
 文件名：InitlizeMethodAttribute
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 17:59:56

 功能描述：用于标记一个需要在系统加载时初始化的类和方法

----------------------------------------------------------------*/

using System;

namespace SuperGMS.Rpc.Server
{
    /// <summary>
    /// 微服务启动中需要初始化的方法标签, 此标签必须同时标记类 和 方法才生效
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class InitlizeMethodAttribute : Attribute
    {
    }
}