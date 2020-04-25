/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.GrantMQ.Publisher
 文件名：Publisher
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 11:43:12

 功能描述：

----------------------------------------------------------------*/
using GrantMicroService.MQ.RabbitMQ;

namespace GrantMicroService.MQ
{
    /// <summary>
    /// 消息发布者
    /// </summary>
    /// <typeparam name="M">M</typeparam>
   public sealed class GrantDefaultPublisher<M> : GrantPublisher<M>
       where M : class
   {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrantDefaultPublisher{M}"/> class.
        /// 消息发布者（默认配置）注释掉了，不能有这样的重载，要不乱套了，都发到一个上面了
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="autoDelete">是否自动删除</param>
        // public GrantDefaultPublisher(M msg, bool autoDelete)
        //    : this(msg, RouterKeyConst.DefaultRouterKey, autoDelete)
        // {
        // }

        /// <summary>
        /// 消息发布者(默认配置，指定特定routerKey)
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="routerKey">关注的特定key消息，决定路由</param>
        /// <param name="autoDelete">是否自动删除</param>
        public GrantDefaultPublisher(M msg, string routerKey, bool autoDelete)
           : this(msg, routerKey, MQueueConst.DefaultGrantMQ, autoDelete)
        {
        }

       /// <summary>
       /// Initializes a new instance of the <see cref="GrantDefaultPublisher{M}"/> class.
       /// 消息发布者  非默认，一个自定义的消息交换机、队列、路由key
       /// </summary>
       /// <param name="msg">消息</param>
       /// <param name="exchange">交换机</param>
       /// <param name="routeKey">路由key</param>
       /// <param name="queueName">队列</param>
       /// <param name="autoDelete">是否自动删除</param>
        public GrantDefaultPublisher(M msg, string exchange, string routeKey, string queueName, bool autoDelete)
           : base(
               new MQueue()
               {
                   AutoDeclare = false,
                   AutoDelete = autoDelete,
                   Durable = true,
                   Exchange = new Exchange()
                   {
                       AutoDeclare = false,
                       AutoDelete = false,
                       Durable = true,
                       ExchangeType = ExchangeType.Direct,
                       ExchangeName = exchange,
                   },
                   Exclusive = false,
                   RouteKey = routeKey,
                   QueueName = queueName,
                   Host = MQHostConfigManager.GetDefaultHost(),
               }, msg)
       {
       }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantDefaultPublisher{M}"/> class.
        /// 消息发布者（默认配置，指定了特定的routeKey和queueName）
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="routeKey">消息路由标记routeKey</param>
        /// <param name="queueName">发送者没有队列，这里不对，所以改成private</param>
        /// <param name="autoDelete">是否自动删除</param>
        private GrantDefaultPublisher(M msg, string routeKey, string queueName, bool autoDelete)
            : this(msg, ExchangeConst.DefaultExchange, routeKey, queueName, autoDelete)
        {
        }
    }
}
