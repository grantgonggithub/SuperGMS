/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.RabbitMQ.Config
 文件名：MQueue
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/20 14:37:59

 功能描述：队列

----------------------------------------------------------------*/

namespace GrantMicroService.MQ.RabbitMQ
{
    /// <summary>
    /// 队列
    /// </summary>
    public class MQueue
    {
        /// <summary>
        /// 队列名称
        /// </summary>
       public string QueueName { get; set; }

        /// <summary>
        /// 如果投送模式是Direct 需要指定这个值，作为投送的key,匹配到此Queue
        /// </summary>
       public string RouteKey { get; set; }
        /// <summary>
        /// 是否是持久化队列
        /// </summary>
        public bool Durable { get; set; }
        /// <summary>
        /// 是否允许自动创建
        /// </summary>
        public bool AutoDeclare { get; set; }
        /// <summary>
        /// 是否在空闲时允许删除
        /// </summary>
        public bool AutoDelete { get; set; }
        /// <summary>
        /// 排他性
        /// </summary>
        public bool Exclusive { get; set; }

        /// <summary>
        /// 所在的虚拟主机
        /// </summary>
        public VirtualHost Host { get; set; }

        /// <summary>
        /// 所绑定的交换机
        /// </summary>
        public Exchange Exchange { get; set; }

    }
}
