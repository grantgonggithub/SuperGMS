/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.RabbitMQ
 文件名：MQEndPoint
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/20 16:21:05

 功能描述：

----------------------------------------------------------------*/
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using SuperGMS.Log;

using System;

namespace SuperGMS.MQ.RabbitMQ
{
    /// <summary>
    /// 实现一个连接MQ的节点
    /// </summary>
    internal abstract class MQEndPoint : IDisposable
    {
        protected MQueue _mQueue;
        protected readonly static ILogger logger = LogFactory.CreateLogger<MQEndPoint>();
        protected MQueue MQueue
        {
            get { return _mQueue; }
        }
        protected bool disposed = false;
        /// <summary>
        /// Initializes a new instance of the <see cref="MQEndPoint"/> class.
        /// MQ的节点
        /// </summary>
        /// <param name="mQueue">mQueue</param>
        public MQEndPoint(MQueue mQueue)
        {
            _mQueue = mQueue;
        }

        /// <summary>
        /// 建立一个连接，并执行指定的方法
        /// </summary>
        /// <param name="fn">fn</param>
        /// <returns>bool</returns>
        protected abstract bool Run(Func<IModel, bool> fn);
        /// <summary>
        /// 断线重连
        /// </summary>
        /// <param name="sender"></param>
        protected abstract void ReConnection(object sender);
        public abstract void Dispose();
        /// <summary>
        /// 连接关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            ReConnection(sender);
        }
        /// <summary>
        /// 连接阻塞事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Connection_ConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            ReConnection(sender);
        }
    }
}
