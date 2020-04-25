/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.GrantMQ.Consumer
 文件名：Consumer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 11:43:26

 功能描述：一个业务层的消费者

----------------------------------------------------------------*/

using GrantMicroService.MQ.RabbitMQ;

namespace GrantMicroService.MQ
{
    /// <summary>
    /// 一个业务层的消费者
    /// </summary>
    /// <typeparam name="M">M</typeparam>
    public sealed class GrantDefaultConsumer<M> : GrantConsumer<M>
        where M :class
    {
        ///// <summary>
        ///// 当收到底层消息时触发的事件
        ///// </summary>
        // public event GrantMsgReceiveHandle<M> OnGrantMsgReceive;

        // private Consumer consumer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantDefaultConsumer{M}"/> class.
        /// 注册一个消费者
        /// </summary>
        /// <returns>55</returns>
        // public bool Register()
        // {
        //    return this.consumer.Register();
        // }

        /// <summary>
        /// 手动释放消费者
        /// </summary>
        // public void Dispose()
        // {
        //    this.consumer.Dispose();
        // }
        /// <summary>
        /// 消费者 最全构造
        /// </summary>
        /// <param name="queue"></param>
        // public GrantDefaultConsumer(MQueue queue)
        //    :base(queue)
        // {
        // if (this.consumer == null)
        // {
        //    this.consumer = new Consumer(queue);
        //    this.consumer.OnMessageReceive += Consumer_OnMessageReceive;
        // }
        // }

        // private bool Consumer_OnMessageReceive(string args, Exception ex)
        // {
        //    if (OnGrantMsgReceive != null)
        //    {
        //        M m = default(M);
        //        if (ex != null)
        //           return OnGrantMsgReceive(null, ex);
        //        else
        //        {
        //            try
        //            {
        //                if (!string.IsNullOrEmpty(args))
        //                    m = JsonConvert.DeserializeObject<M>(args);
        //            }
        //            catch (Exception ex1)
        //            {
        //                ex = new Exception("Consumer_OnMessageReceive.JsonConvert.DeserializeObject.Error", ex1);
        //            }
        //           return OnGrantMsgReceive(m, ex);
        //        }
        //    }
        //    return true;//没有传回调直接删除掉
        // }

        /// <summary>
        /// 订阅系统全部默认的消息   这个注释掉了，要不糊里糊涂搞乱了消息
        /// </summary>
        /// <param name="autoDelete"></param>
        // public GrantDefaultConsumer(bool autoDelete)
        //    : this(RouterKeyConst.DefaultRouterKey, autoDelete)
        // {
        // }

        /// <summary>
        /// 消费者，一个指定了特定routerKey的消费订阅，其他系统默认  这个也注释掉了，防止把一个RouterKey绑定到一个队列上，消息就乱套了
        /// </summary>
        /// <param name="routerKey">routerKey</param>
        /// <param name="autoDelete">autoDelete</param>
        // public GrantDefaultConsumer(string routerKey, bool autoDelete)
        //    : this(routerKey, MQueueConst.DefaultGrantMQ, autoDelete)
        // {

        // }

        /// <summary>
        /// 消费者，一个指定了特定queueName上特定routeKey的消费，其他系统默认
        /// </summary>
        /// <param name="queueName">这个队列一定要注意，特定的key，对应特定的接收队列，
        /// 真正决定消息的路由是routeKey,Queue是接收的容器，理论上不同的routeKey要定义不同的Queue,否则不同的key投送到同一个Queue上就乱了</param>
        /// <param name="routeKey">路由key</param>
        /// <param name="autoDelete">是否自动删除</param>
        public GrantDefaultConsumer(string routeKey, string queueName, bool autoDelete)
            : this(ExchangeConst.DefaultExchange, routeKey, queueName, autoDelete)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantDefaultConsumer{M}"/> class.
        /// 消费者，一个完全自定义的消费者
        /// </summary>
        /// <param name="exchange">exchange</param>
        /// <param name="routeKey">routeKey</param>
        /// <param name="queueName">queueName</param>
        /// <param name="autoDelete">autoDelete</param>
        public GrantDefaultConsumer(string exchange, string routeKey, string queueName, bool autoDelete)
            : base(new MQueue()
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
                QueueName = queueName,
                RouteKey = routeKey,
                Host = MQHostConfigManager.GetDefaultHost(),
            })
        {
        }
    }
}
