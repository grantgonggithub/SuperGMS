/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.GrantMQ.Consumer
 文件名：FanoutConsumer
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
    public sealed class FanoutConsumer<M> : Consumer<M>
        where M :class
    {

        /// <summary>
        /// 消费者，一个完全自定义的消费者
        /// </summary>
        /// <param name="exchange">exchange</param>
        /// <param name="queueName">queueName</param>
        /// <param name="autoDelete">autoDelete</param>
        /// <param name="_objCtx"></param>
        public FanoutConsumer(string exchange, string queueName, bool autoDelete, object _objCtx = null)
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
                    ExchangeType = ExchangeType.Fanout,
                    ExchangeName = exchange,
                },
                Exclusive = false,
                QueueName = queueName,
                RouteKey ="",
                Host = MQHostConfigManager.GetDefaultHost(),
            },_objCtx)
        {
        }

        /// <summary>
        /// 接受广播消息的消费者
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queueName"></param>
        /// <param name="autoDelete"></param>
        /// <param name="host"></param>
        /// <param name="_objCtx"></param>
        public FanoutConsumer(string exchange, string queueName, bool autoDelete, VirtualHost host,object _objCtx=null)
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
                    ExchangeType = ExchangeType.Fanout,
                    ExchangeName = exchange,
                },
                Exclusive = false,
                QueueName = queueName,
                RouteKey = "",
                Host = host,
            }, _objCtx)
        {
        }
    }
}
