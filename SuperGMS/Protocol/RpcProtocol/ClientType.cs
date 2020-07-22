/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Protocol.RpcProtocol
 文件名：  ClientType
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/21 13:47:55

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.Protocol.RpcProtocol
{
    /// <summary>
    /// ClientType
    /// </summary>
    public enum ClientType
    {
        /// <summary>
        /// web前端，需要检查接入ip或者域名
        /// </summary>
        Web,

        /// <summary>
        /// 移动app
        /// </summary>
        App,

        /// <summary>
        /// 微信
        /// </summary>
        WeiXin,

        /// <summary>
        /// 第三方
        /// </summary>
        ThirdPart,

        /// <summary>
        /// 内部微服务，前端接入层会拦截这个类型
        /// </summary>
        InnerRpc,

        /// <summary>
        /// 未知类型
        /// </summary>
        Unkunwn,
    }
}