/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantMQ.Consumer
 文件名：GrantConsumer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/6/5 17:00:15

 功能描述：

----------------------------------------------------------------*/
using System;
using Newtonsoft.Json;
using SuperGMS.MQ.RabbitMQ;

namespace SuperGMS.MQ
{
    /// <summary>
    /// 封装一个底层消息的委托
    /// </summary>
    /// <param name="msg">msg</param>
    /// <param name="ex">ex</param>
    /// <returns>返回处理成功还是失败</returns>
    public delegate bool GrantMsgReceiveHandle<M>(M msg, Exception ex);

    /// <summary>
    /// 所有消费者的基类
    /// </summary>
    /// <typeparam name="M">M</typeparam>
    public class GrantConsumer<M> : IDisposable
        where M : class
    {
        /// <summary>
        /// 当收到底层消息时触发的事件
        /// </summary>
        public event GrantMsgReceiveHandle<M> OnGrantMsgReceive;

        private RabbitMQ.Consumer consumer;

        /// <summary>
        /// 注册一个消费者
        /// </summary>
        /// <returns>bool</returns>
        public bool Register()
        {
            return this.consumer.Register();
        }

        /// <summary>
        /// 手动释放消费者
        /// </summary>
        public void Dispose()
        {
            this.consumer.OnMessageReceive -= Consumer_OnMessageReceive;
            this.consumer.Dispose();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantConsumer{M}"/> class.
        /// 消费者 最全构造
        /// </summary>
        /// <param name="queue">queue</param>
        public GrantConsumer(MQueue queue)
        {
            if (this.consumer == null)
            {
                this.consumer = new RabbitMQ.Consumer(queue);
                this.consumer.OnMessageReceive += Consumer_OnMessageReceive;
            }
        }

        ///// <summary>
        ///// 订阅系统全部默认的消息
        ///// </summary>
        ///// <param name="autoDelete"></param>
        // public GrantConsumer(bool autoDelete)
        //    :this(RouterKeyConst.DefaultRouterKey,autoDelete)
        // {

        // }

        ///// <summary>
        ///// 消费者，一个指定了特定routerKey的消费订阅，其他系统默认
        ///// </summary>
        ///// <param name="routerKey"></param>
        ///// <param name="autoDelete"></param>
        // public GrantConsumer(string routerKey, bool autoDelete)
        //    : this(routerKey, MQueueConst.DefaultGrantMQ, autoDelete)
        // {

        // }

        ///// <summary>
        ///// 消费者，一个指定了特定queueName上特定routeKey的消费，其他系统默认
        ///// </summary>
        ///// <param name="routeKey">路由key</param>
        ///// <param name="queueName">队列</param>
        ///// <param name="autoDelete"></param>
        // public GrantConsumer(string routeKey, string queueName, bool autoDelete)
        //    : this(ExchangeConst.DefaultExchange, routeKey, queueName, autoDelete)
        // {

        // }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantConsumer{M}"/> class.
        /// 消费者，一个完全自定义的消费者
        /// </summary>
        /// <param name="exchange">exchange</param>
        /// <param name="routeKey">routeKey</param>
        /// <param name="queueName">queueName</param>
        /// <param name="autoDelete">autoDelete</param>
        /// <param name="host">虚拟主机(可以用HostConfigManager.GetHost获取)</param>
        public GrantConsumer(string exchange, string routeKey, string queueName, bool autoDelete, VirtualHost host)
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

        private bool Consumer_OnMessageReceive(string args, Exception ex)
        {
            if (OnGrantMsgReceive != null)
            {
                M m = default(M);
                if (ex != null)
                {
                    return OnGrantMsgReceive(null, ex);
                }
                else
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(args))
                        {
                            m = JsonConvert.DeserializeObject<M>(args);
                        }
                    }
                    catch (Exception ex1)
                    {
                        ex = new Exception("Consumer_OnMessageReceive.JsonConvert.DeserializeObject.Error", ex1);
                    }

                    return OnGrantMsgReceive(m, ex);
                }
            }

            return true; // 没有传回调直接删除掉
        }
    }
}
