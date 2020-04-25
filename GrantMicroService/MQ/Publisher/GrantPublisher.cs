/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.GrantMQ.Consumer
 文件名：GrantPublisher
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/6/5 16:30:16

 功能描述：

----------------------------------------------------------------*/

using System;
using Newtonsoft.Json;
using GrantMicroService.MQ.RabbitMQ;

namespace GrantMicroService.MQ
{
    /// <summary>
    /// 所有生产者的基类
    /// </summary>
    /// <typeparam name="M">M</typeparam>
   public class GrantPublisher<M>:IDisposable
       where M : class
   {
        private Publisher publisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantPublisher{M}"/> class.
        /// 消息发布者 最全量的构造
        /// </summary>
        /// <param name="mq">mq</param>
        /// <param name="msg">msg</param>
        public GrantPublisher(MQueue mq, M msg)
        {
            string m = JsonConvert.SerializeObject(msg);
            this.publisher = new Publisher(mq, m);
        }

        ///// <summary>
        ///// 消息发布者（默认配置）
        ///// </summary>
        ///// <param name="msg">消息</param>
        ///// <param name="autoDelete">是否自动删除</param>
        // public GrantPublisher(M msg, bool autoDelete)
        //    : this(msg,RouterKeyConst.DefaultRouterKey,autoDelete)
        // {

        // }

        ///// <summary>
        ///// 消息发布者(默认配置，指定特定routerKey)
        ///// </summary>
        ///// <param name="msg">消息</param>
        ///// <param name="routerKey">关注的特定key的队列</param>
        ///// <param name="autoDelete">是否自动删除</param>
        // public GrantPublisher(M msg, string routerKey, bool autoDelete)
        //    : this(msg,routerKey,MQueueConst.DefaultGrantMQ,autoDelete)
        // {
        // }
        ///// <summary>
        ///// 消息发布者（默认配置，指定了特定的routeKey和queueName）
        ///// </summary>
        ///// <param name="msg">消息</param>
        ///// <param name="routeKey">关注指定队列queueName的特定routeKey</param>
        ///// <param name="queueName">队列名称</param>
        ///// <param name="autoDelete">是否自动删除</param>
        // public GrantPublisher(M msg, string routeKey, string queueName, bool autoDelete)
        //    : this(msg, ExchangeConst.DefaultExchange, routeKey, queueName, autoDelete)
        // {

        // }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantPublisher{M}"/> class.
        /// 消息发布者  非默认，一个自定义的消息交换机、队列、路由key
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="exchange">交换机</param>
        /// <param name="routeKey">路由key</param>
        /// <param name="queueName">队列</param>
        /// <param name="autoDelete">是否自动删除</param>
        public GrantPublisher(M msg, string exchange, string routeKey, string queueName, bool autoDelete, VirtualHost host)
            : this(
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
            Host = host,
            }, msg)
        {
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <returns>bool</returns>
        public bool Publish()
        {
            return this.publisher.Publish();
        }

        public void Dispose()
        {
           this.publisher?.Dispose();
        }
    }
}
