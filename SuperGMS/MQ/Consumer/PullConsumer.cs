using Newtonsoft.Json;

using SuperGMS.MQ.RabbitMQ;

using System;

namespace SuperGMS.MQ.Consumer
{
    /// <summary>
    /// 拉模式消费者
    /// </summary>
    /// <typeparam name="M">消息内容</typeparam>
    public class PullConsumer<M> : IDisposable
        where M : class
    {
        private RabbitMQ.PullConsumer consumer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Consumer{M}"/> class.
        /// 消费者 最全构造
        /// </summary>
        /// <param name="queue">queue</param>
        public PullConsumer(MQueue queue)
        {
            if (this.consumer == null)
            {
                this.consumer = new RabbitMQ.PullConsumer(queue);
            }
        }

        /// <summary>
        /// 注册一个消费者
        /// </summary>
        /// <returns>bool</returns>
        public bool Register()
        {
            return this.consumer.Register();
        }

        /// <summary>
        /// 拉取并处理消息
        /// </summary>
        /// <param name="action"></param>
        public void DealPullMsg(Func<M, Exception, bool> action)
        {
            try
            {
                var msg = this.consumer.Get();
                if (msg != null)
                {
                    if (action != null)
                    {
                        var o = JsonConvert.DeserializeObject<M>(msg?.Msg ?? "{}");
                        if (action((M) o, null))
                        {
                            this.consumer.AckOk(msg.Tag);
                        }
                    }
                }
                else
                {
                    if (action != null)
                    {
                        action(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                action?.Invoke(null, ex);
            }
        }

        public PullConsumer(string exchange, string routeKey, string queueName, bool autoDelete, VirtualHost host)
            : this(new MQueue()
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
                Host = host,
            })
        {
        }
        public void Dispose()
        {
            this.consumer.Dispose();
        }
    }
}
