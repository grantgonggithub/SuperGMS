/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.RabbitMQ.Publisher
 文件名：Publisher
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/20 17:13:25

 功能描述：实现一个事件源

----------------------------------------------------------------*/
using System;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace SuperGMS.MQ.RabbitMQ
{
    /// <summary>
    /// 事件源发布一个消息
    /// </summary>
    internal class Publisher : MQEndPoint
    {
        private string _msg;

        /// <summary>
        /// Initializes a new instance of the <see cref="Publisher"/> class.
        /// 发布一个消息
        /// </summary>
        /// <param name="mQueue">队列</param>
        /// <param name="msg">消息内容</param>
        public Publisher(MQueue mQueue, string msg)
            : base(mQueue)
        {
            this._msg = msg;
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <returns>bool</returns>
        public bool Publish()
        {
            return Run(publish);
        }

        protected override bool Run(Func<IModel, bool> fn)
        {
            try
            {
                using (MQConnection connection = MQConnectionManager.GetMqConnection(MQueue.Host.Host, MQueue.Host.Port, MQueue.Host.Username, MQueue.Host.Password))
                {
                    using (IModel channel = connection.Connection.CreateModel())
                    {
                        return fn(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"消息发送失败,Ip:{MQueue.Host.ToString()}");
                return false;
            }
        }
        /// <summary>
        /// 断线重连
        /// </summary>
        /// <param name="sender">sender</param>
        protected override void ReConnection(object sender)
        {
            //发布消息每次会新建连接
        }
        public override void Dispose()
        {
            //无非托管资源
        }
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="channel">channel</param>
        /// <returns>bool</returns>
        private bool publish(IModel channel)
        {
            try
            {
                // 定义一个Exchange
                channel.ExchangeDeclare(MQueue.Exchange.ExchangeName, MQueue.Exchange.ExchangeType, MQueue.Exchange.Durable, MQueue.Exchange.AutoDelete, null);
                IBasicProperties properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2;
                var body = Encoding.UTF8.GetBytes(_msg);

                // 将这个消息发布到Exchange
                channel.BasicPublish(
                    exchange: MQueue.Exchange.ExchangeName,
                    routingKey: MQueue.RouteKey,
                    basicProperties: properties,
                    body: body);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Publisher.publish.Error");
                return false;
            }
        }
    }
}
