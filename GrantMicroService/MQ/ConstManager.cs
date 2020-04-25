/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.GrantMQ
 文件名：ConstManager
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 11:32:58

 功能描述：这个只在框架里面用，外面自己定义

----------------------------------------------------------------*/

namespace GrantMicroService.MQ
{
    /// <summary>
    /// 定义交换机相关的常量
    /// </summary>
    internal class ExchangeConst
    {
        public const string DefaultExchange = "GrantExchange.direct";
        public const string DefaultFanoutExchange = "GrantExchange.fanout";
    }
    /// <summary>
    /// 虚拟机
    /// </summary>
    internal class VirtualHostConst
    {
        public const string DefaultVirtualHost = "DefaultVirtualHost";
    }
    /// <summary>
    /// 定义队列名称常量
    /// </summary>
    internal class MQueueConst
    {
        public const string DefaultGrantMQ = "DefaultGrantMQ";
    }

    /// <summary>
    /// 路由key，根据这个key进行分发路由
    /// </summary>
    internal class RouterKeyConst
    {
        public const string DefaultRouterKey = "DefaultRouterKey";

    }



}
