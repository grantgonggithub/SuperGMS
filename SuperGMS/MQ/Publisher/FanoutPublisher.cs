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
   public sealed class FanoutPublisher<TM> : Publisher<TM>
       where TM : class
   {
        /// <summary>
        /// 消息发布者  非默认，一个自定义的消息交换机、队列、路由key
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="exchange">交换机</param>
        /// <param name="queueName">队列</param>
        /// <param name="autoDelete">是否自动删除</param>
        public FanoutPublisher(TM msg, string exchange, string queueName, bool autoDelete)
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
                    RouteKey=queueName,
                    Exclusive = false,
                    QueueName = queueName,
                    Host = MQHostConfigManager.GetDefaultHost(),
                    }, msg: msg)
        {

        }

        /// <summary>
        /// 添加发布者广播消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routeKey"></param>
        /// <param name="queueName"></param>
        /// <param name="autoDelete"></param>
        /// <param name="host"></param>
        public FanoutPublisher(TM msg, string exchangeName, string routeKey, string queueName, bool autoDelete, VirtualHost host)
            : base(msg, exchangeName, routeKey, queueName, autoDelete, host,ExchangeType.Fanout)
        { }
    }
}
