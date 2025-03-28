/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：grant.RpcProxy.GlobalTools.BackGroundMessage
 文件名：  SetBackGroudMessageArgs
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/30 10:23:11

 功能描述：

----------------------------------------------------------------*/

using SuperGMS.Protocol.RpcProtocol;

namespace SuperGMS.Extend.BackGroundMessage
{
    using System;

    /// <summary>
    /// SetBackGroudMessageArgs
    /// </summary>
    public class SetBackGroudMessageArgs
    {
        public string Data { get; set; }

        /// <summary>
        /// 如果这个是点对点消息，此值为Router 如果是扇波消息则为ExChangeName
        /// </summary>
        public string MQRouterName { get; set; }

        public string TtId { get; set; }

        public int UserId { get; set; }

        public DateTime CreateDateTime { get; set; }

        public string BussinessId { get; set; }

        // public string Rid { get; set; }

        // public string Tk { get; set; }
        public Args<object> Args { get; set; }
    }

    public class SetBackGroudMessageResult
    {
        public string TaskGuid { get; set; }
    }
}
