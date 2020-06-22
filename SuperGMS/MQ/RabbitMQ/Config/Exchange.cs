/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.RabbitMQ
 文件名：RabbitMQConfig
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/20 13:25:16

 功能描述：一个虚拟主机上的一个交换机

----------------------------------------------------------------*/

namespace SuperGMS.MQ.RabbitMQ
{
    /// <summary>
    /// 交换机
    /// </summary>
    public class Exchange
    {
        /// <summary>
        /// 所属虚拟Host
        /// </summary>
        public VirtualHost VHost { get; set; }
        /// <summary>
        /// 交换机名称
        /// </summary>
        public string ExchangeName { get; set; }
        /// <summary>
        /// 投送模式
        /// </summary>
        public string ExchangeType { get; set; }

        /// <summary>
        /// 是否是持久化的交换机
        /// </summary>
        public bool Durable { get; set; }
        /// <summary>
        /// 是否可以自动创建
        /// </summary>
        public bool AutoDeclare { get; set; }
        /// <summary>
        /// 是否可以在空闲时自动删除
        /// </summary>
        public bool AutoDelete { get; set; }

    }
}
