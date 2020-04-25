/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.RabbitMQ.Consumer
 文件名：Consumer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/20 16:44:45

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GrantMicroService.Log;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrantMicroService.MQ.RabbitMQ
{
    /// <summary>
    /// 实现一个主动拉取的消费者
    /// </summary>
    internal class PullConsumer : MQEndPoint
    {
        private IConnection consumerConnection;
        private IModel consumerChannel;
        private Func<IModel, bool> _fn;
        /// <summary>
        /// Initializes a new instance of the <see cref="Consumer"/> class.
        /// 实现一个消费者
        /// </summary>
        /// <param name="mQueue">mQueue</param>
        public PullConsumer(MQueue mQueue)
            : base(mQueue)
        {
        }

        /// <summary>
        /// 注册一个Consumer
        /// </summary>
        /// <returns>bool</returns>
        public bool Register()
        {
            return Run(initlize);
        }

        protected override bool Run(Func<IModel, bool> fn)
        {
            this._fn = fn;
            try
            {
                ConnectionFactory connectionFactory = new ConnectionFactory()
                {
                    HostName = MQueue.Host.Host,
                    Port = MQueue.Host.Port,
                    UserName = MQueue.Host.Username,
                    Password = MQueue.Host.Password
                };
                // 订阅者不会主动去重新注册连接的，所以如果连接断了，需要自己尝试连接
                consumerConnection = connectionFactory.CreateConnection();
                consumerConnection.ConnectionBlocked += Connection_ConnectionBlocked;
                consumerConnection.ConnectionShutdown += Connection_ConnectionShutdown;
                consumerChannel = consumerConnection.CreateModel();
                logger.LogDebug($"客户端连接成功,Ip:{_mQueue.Host.ToString()},注册的queueName={_mQueue.QueueName},RouterKey={_mQueue.RouteKey}");
                return fn(consumerChannel);
            }
            catch (Exception ex)
            {
                // 在线程中去重试，不能挡住主线程了，因为是递归的，所以不会出现多线程同时重连的情况
                Task.Run(() =>
                {
                    ReConnection(consumerConnection);
                });
            }
            return false;
        }
        /// <summary>
        /// 断线重连
        /// </summary>
        /// <param name="sender">sender</param>
        protected override void ReConnection(object sender)
        {
            logger.LogWarning($"连接断开,重新尝试连接,请您检查MQ的连接是否正常...");
            if (sender != null)
            {
                try
                {
                    IConnection conn = sender as IConnection;
                    if (conn.IsOpen)
                    {
                        conn.Close();
                    }
                    conn.Dispose();
                }
                catch
                {
                    //关闭、释放MQ连接异常，暂不做处理
                }
            }

            Random r = new Random(DateTime.Now.Millisecond);
            // 防止所有MQ的客户端同时重连服务器，通过随机数，错开时间，减少MQ服务器重启瞬间的并发
            int sleepTime = r.Next(2, 15);
            // 如果连接异常，会等待10s 重新连接
            System.Threading.Thread.Sleep(sleepTime * 1000);
            // 打算重连必须把这个置为非主动释放，这个值对应上面的那个this.isDispose = true;
            if (this._fn != null)
            {
                this.Run(this._fn);
            }
        }
        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="isAck"></param>
        /// <returns></returns>
        public PullMsg Get()
        {
            try
            {
                if (this.consumerChannel != null)
                {
                    var result = this.consumerChannel.BasicGet(this.MQueue.QueueName, this.MQueue.AutoDelete);
                    if (result == null)
                    {
                        return null;
                    }
                    else
                    {
                        var body = result.Body;
                        string msg;
                        if (body == null || body.Length < 1)
                        {
                            msg = null;
                        }
                        else
                        {
                            msg = Encoding.UTF8.GetString(body);
                        }

                        return new PullMsg()
                        {
                            Msg = msg,
                            Tag = result.DeliveryTag,
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "拉取消息异常");
                return null;
            }
        }

        /// <summary>
        /// 确认消息
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool AckOk(ulong tag)
        {
            if (this.MQueue.AutoDelete)
            {
                return true;
            }

            try
            {
                this.consumerChannel?.BasicAck(tag, false);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PullConsumer.AckOk().Error");
                return false;
            }

        }

       /// <summary>
        /// initlize
        /// </summary>
        /// <param name="channel">channel</param>
        /// <returns>bool</returns>
        private bool initlize(IModel channel)
        {
            try
            {
                // 定义一个Exchange
                channel.ExchangeDeclare(MQueue.Exchange.ExchangeName, MQueue.Exchange.ExchangeType, MQueue.Exchange.Durable, MQueue.Exchange.AutoDelete, null);

                // 声明一个队列
                channel.QueueDeclare(
                    queue: MQueue.QueueName, // 指定发送消息的queue，和生产者的queue匹配
                    durable: MQueue.Durable,
                    exclusive: MQueue.Exclusive,
                    autoDelete: false,
                    arguments: null);

                // 将此队列绑定到Exchange上
                channel.QueueBind(MQueue.QueueName, MQueue.Exchange.ExchangeName, MQueue.RouteKey, null);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"PullConsumer.initlize 异常");
            }

            return false;
         }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {

                if (this.consumerConnection != null)
                {
                    this.consumerConnection.Dispose();
                }

                if (this.consumerChannel != null)
                {
                    this.consumerChannel.Dispose();
                }

            }
            // Release unmanaged resource
            disposed = true;
        }
        /// <summary>
        /// 析构
        /// </summary>
        ~PullConsumer()
        {
            Dispose(false);
        }
    }

    public class PullMsg
    {
        public string Msg { get; set; }
        public ulong Tag { get; set; }
    }
}
