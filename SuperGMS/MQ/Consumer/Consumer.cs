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
    /// <param name="objCtx"></param>
    /// <returns>返回处理成功还是失败</returns>
    public delegate bool GrantMsgReceiveHandle<M>(M msg, Exception ex,object objCtx);

    /// <summary>
    /// 所有消费者的基类
    /// </summary>
    /// <typeparam name="M">M</typeparam>
    public class Consumer<M> : IDisposable
        where M : class
    {
        /// <summary>
        /// 当收到底层消息时触发的事件
        /// </summary>
        public event GrantMsgReceiveHandle<M> OnGrantMsgReceive;

        private RabbitMQ.Consumer consumer;

        private object objCtx;

        /// <summary>
        /// 获取用户自定义的要传递的上下文信息
        /// </summary>
        public object ObjCtx {
            get { return objCtx; }
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
        /// 手动释放消费者
        /// </summary>
        public void Dispose()
        {
            this.consumer.OnMessageReceive -= Consumer_OnMessageReceive;
            this.consumer.Dispose();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Consumer{M}"/> class.
        /// 消费者 最全构造
        /// </summary>
        /// <param name="queue">queue</param>
        /// <param name="_objCtx"></param>
        public Consumer(MQueue queue,object _objCtx=null)
        {
            if (this.consumer == null)
            {
                this.consumer = new RabbitMQ.Consumer(queue);
                this.consumer.OnMessageReceive += Consumer_OnMessageReceive;
            }
            this.objCtx = _objCtx;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantConsumer{M}"/> class.
        /// 消费者，一个完全自定义的消费者
        /// </summary>
        /// <param name="exchange">exchange</param>
        /// <param name="routeKey">routeKey</param>
        /// <param name="queueName">queueName</param>
        /// <param name="autoDelete">autoDelete</param>
        /// <param name="host">虚拟主机(可以用HostConfigManager.GetHost获取)</param>
        /// <param name="exChangeType"></param>
        /// <param name="_objCtx"></param>
        public Consumer(string exchange, string routeKey, string queueName, bool autoDelete, VirtualHost host, object _objCtx = null, string exChangeType=ExchangeType.Direct)
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
                    ExchangeType = exChangeType,
                    ExchangeName = exchange,
                },
                Exclusive = false,
                QueueName = queueName,
                RouteKey = routeKey,
                Host = host,
            },_objCtx)
        {
        }

        private bool Consumer_OnMessageReceive(string args, Exception ex)
        {
            if (OnGrantMsgReceive != null)
            {
                M m = default(M);
                if (ex != null)
                {
                    return OnGrantMsgReceive(null, ex,this.objCtx);
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

                    return OnGrantMsgReceive(m, ex,this.objCtx);
                }
            }

            return true; // 没有传回调直接删除掉
        }
    }
}
