/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.GrantMQ.Consumer
 文件名：Consumer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 11:43:26

 功能描述：一个业务层的消费者

----------------------------------------------------------------*/

using SuperGMS.MQ.RabbitMQ;

namespace SuperGMS.MQ
{
    /// <summary>
    /// 一个业务层的消费者
    /// </summary>
    /// <typeparam name="M">M</typeparam>
    public sealed class DefaultConsumer<M> : Consumer<M>
        where M :class
    {
        ///// <summary>
        ///// 当收到底层消息时触发的事件
        ///// </summary>

        /// <summary>
        /// 消费者，一个指定了特定queueName上特定routeKey的消费，其他系统默认
        /// </summary>
        /// <param name="queueName">这个队列一定要注意，特定的key，对应特定的接收队列，
        /// 真正决定消息的路由是routeKey,Queue是接收的容器，理论上不同的routeKey要定义不同的Queue,否则不同的key投送到同一个Queue上就乱了</param>
        /// <param name="routeKey">路由key</param>
        /// <param name="autoDelete">是否自动删除</param>
        /// <param name="_objCtx"></param>
        public DefaultConsumer(string routeKey, string queueName, bool autoDelete, object _objCtx = null)
            : this(ExchangeConst.DefaultExchange, routeKey, queueName, autoDelete,_objCtx)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConsumer{M}"/> class.
        /// 消费者，一个完全自定义的消费者
        /// </summary>
        /// <param name="exchange">exchange</param>
        /// <param name="routeKey">routeKey</param>
        /// <param name="queueName">queueName</param>
        /// <param name="autoDelete">autoDelete</param>
        /// <param name="_objCtx"></param>
        public DefaultConsumer(string exchange, string routeKey, string queueName, bool autoDelete, object _objCtx = null)
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
            },_objCtx)
        {
        }
    }
}
