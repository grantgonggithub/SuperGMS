/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.GrantRpc.Server
 文件名：  UnRegisterMethodAttribute
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/9/28 16:52:06

 功能描述：

----------------------------------------------------------------*/

using System;

namespace GrantMicroService.Rpc.Server
{
    /// <summary>
    /// 标记为当前属性的方法，将在服务卸载，停止时调用，来执行清理工作
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class UnRegisterMethodAttribute : Attribute
    {
    }
}