/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.GrantMQ.Publisher
 文件名：Publisher
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 11:43:12

 功能描述：

----------------------------------------------------------------*/
using SuperGMS.MQ.RabbitMQ;

namespace SuperGMS.MQ
{
    /// <summary>
    /// 扇出消息 发布者
    /// </summary>
    /// <typeparam name="TM">消息类型</typeparam>
   public sealed class GrantFanoutPublisher<TM> : GrantPublisher<TM>
       where TM : class
   {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrantFanoutPublisher{TM}"/> class.
        /// 消息发布者（默认配置）
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="autoDelete">是否自动删除</param>
        public GrantFanoutPublisher(TM msg, bool autoDelete)
            : this(msg, ExchangeConst.DefaultFanoutExchange, MQueueConst.DefaultGrantMQ, autoDelete)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantFanoutPublisher{TM}"/> class.
        /// 消息发布者（默认配置，指定了特定的routeKey和queueName）
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="autoDelete">是否自动删除</param>
        public GrantFanoutPublisher(TM msg, string queueName, bool autoDelete)
            : this(msg, ExchangeConst.DefaultFanoutExchange, queueName, autoDelete)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantFanoutPublisher{TM}"/> class.
        /// 消息发布者  非默认，一个自定义的消息交换机、队列、路由key
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="exchange">交换机</param>
        /// <param name="queueName">队列</param>
        /// <param name="autoDelete">是否自动删除</param>
        public GrantFanoutPublisher(TM msg, string exchange, string queueName, bool autoDelete)
            : base(
                mq: new MQueue()
                {
                    AutoDeclare = false,
                    AutoDelete = autoDelete,
                    Durable = true,
                    Exchange = new Exchange()
                    {
                        AutoDeclare = false,
                        AutoDelete = false,
                        Durable = true,
                        ExchangeType = ExchangeType.Fanout,
                        ExchangeName = exchange,
                    },
                    Exclusive = false,
                    QueueName = queueName,
                    Host = MQHostConfigManager.GetDefaultHost(),
                    }, msg: msg)
        {

        }
   }
}
