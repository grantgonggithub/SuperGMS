/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.RabbitMQ.Config
 文件名：ExchangeType
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/20 14:26:15

 功能描述：交换机类型

----------------------------------------------------------------*/

namespace GrantMicroService.MQ.RabbitMQ
{
    /// <summary>
    /// 交换机类型
    /// </summary>
    public class ExchangeType
    {
        /// <summary>
        /// 直接投送模式，关键字完全匹配
        /// </summary>
        public const string Direct = "direct";
        /// <summary>
        /// 键值对匹配模式
        /// </summary>
        public const string Headers = "headers";
        /// <summary>
        /// 绑定广播模式，所有绑定到Exchange上的队列都会投送
        /// </summary>
        public const string Fanout = "fanout";
        /// <summary>
        /// 主题匹配模式，模糊匹配
        /// </summary>
        public const string Topic = "topic";

    }
}
