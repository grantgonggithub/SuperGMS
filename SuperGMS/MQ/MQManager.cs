/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantMQ
 文件名：GrantMQManager
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 11:31:08

 功能描述：一个提供给上层的业务MQ消息管理类

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperGMS.MQ.Consumer;
using SuperGMS.MQ.RabbitMQ;
using SuperGMS.Protocol.MQProtocol;

namespace SuperGMS.MQ
{
    /// <summary>
    /// 一个默认配置消息的管理类
    /// </summary>
    public class MQManager<M>
        where M : class
    {
        private static List<IDisposable> consumers = new List<IDisposable>();

        #region 点对点消息 Direct 

        /// <summary>
        ///  发布一条指定了特定routerKey的消息
        /// </summary>
        /// <param name="msg">消息内容</param>
        /// <param name="routerKey">消息特定标识</param>
        /// <returns>bool</returns>
        public static bool Publish(MQProtocol<M> msg, string routerKey)
        {
            msg.ExChange = ExchangeConst.DefaultExchange;
            msg.RouterKey = routerKey;
            using (DefaultPublisher<MQProtocol<M>> p = new DefaultPublisher<MQProtocol<M>>(msg, routerKey, false))
            {
                return p.Publish();
            }                
        }

        /// <summary>
        /// 指定虚拟机，发送特定routerKey的消息
        /// </summary>
        /// <param name="msg">msg</param>
        /// <param name="routerKey">routerKey</param>
        /// <param name="host">host</param>
        /// <returns>bool</returns>
        public static bool Publish(MQProtocol<M> msg, string routerKey, VirtualHost host)
        {
            msg.ExChange = ExchangeConst.DefaultExchange;
            msg.RouterKey = routerKey;
            using (DefaultPublisher<MQProtocol<M>> p = new DefaultPublisher<MQProtocol<M>>(msg, ExchangeConst.DefaultExchange, routerKey, MQueueConst.DefaultGrantMQ, false, host))
            {
                // consumers.Add(p);
                return p.Publish();
            }                
        }


        /// <summary>
        /// 注册一个指定了routerKey的消费者
        /// </summary>
        /// <param name="routerKey">特定的routerKey这个用户特定标识特殊消息</param>
        /// <param name="queueName"></param>
        /// <param name="autoDelete">是否自动删除,true为自动删除，false不会自动删除，需要业务自己删除</param>
        /// <param name="fn">/// 处理返回消息的回调，注意如果autoDelete指定为false说明调用方需要自己确定
        /// 消息什么时候删除，这里就需要fn返回是否删除，autoDelete指定为true时，fn这个返回值将不起作用
        /// </param>
        /// <param name="_objCtx"></param>
        public static void ConsumeRegister(string routerKey, string queueName, bool autoDelete, Func<MQProtocol<M>, Exception, object, bool> fn,object _objCtx=null)
        {
            DefaultConsumer<MQProtocol<M>> c = new DefaultConsumer<MQProtocol<M>>(routerKey, queueName, autoDelete,_objCtx);
            c.OnGrantMsgReceive += (MQProtocol<M> m, Exception ex,object objCtx) =>
            {
                return fn(m, ex,_objCtx);
            };
            consumers.Add(c);
            c.Register();
        }

        /// <summary>
        /// 注册一个指定了Host的特定routerKey的消费者
        /// </summary>
        /// <param name="routerKey">注册接受特定的routerKey的消息</param>
        /// <param name="queueName">将特定的routerKey的消息接受到自己的queueName中</param>
        /// <param name="host">特定的host服务器</param>
        /// <param name="autoDelete">是否自动删除autoDelete，一般不要在注册的时候设置为true,在处理完消息的回调中删除消息</param>
        /// <param name="fn">回调fn</param>
        /// <param name="_objCtx"></param>
        public static void ConsumeRegister(string routerKey, string queueName, VirtualHost host, bool autoDelete, Func<MQProtocol<M>, Exception,object, bool> fn, object _objCtx = null)
        {
            Consumer<MQProtocol<M>> c = new Consumer<MQProtocol<M>>(ExchangeConst.DefaultExchange, routerKey, queueName, autoDelete, host,_objCtx);
            c.OnGrantMsgReceive += (MQProtocol<M> m, Exception ex,object objCtx) =>
            {
                return fn(m, ex, objCtx);
            };
            consumers.Add(c);
            c.Register();
        }

        #endregion

        #region 广播消息 Fanout

        /// <summary>
        /// 发布一条广播的消息的消息
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="exChangeName">交换机名称</param>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        public static bool PublishFanout(MQProtocol<M> msg, string exChangeName, string queueName = MQueueConst.DefaultGrantMQ)
        {
            msg.ExChange = exChangeName;
            using (FanoutPublisher<MQProtocol<M>> p = new FanoutPublisher<MQProtocol<M>>(msg, exChangeName, queueName, false))
            {
                return p.Publish();
            }
        }

        /// <summary>
        /// 指定虚拟机，发送特定routerKey的消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="exChangeName"></param>
        /// <param name="host"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public static bool PublishFanout(MQProtocol<M> msg, string exChangeName, VirtualHost host, string queueName = MQueueConst.DefaultGrantMQ)
        {
            msg.ExChange = exChangeName;
            using (FanoutPublisher<MQProtocol<M>> p = new FanoutPublisher<MQProtocol<M>>(msg, exChangeName, queueName, queueName, false, host))
            {
                return p.Publish();
            }
        }


        /// <summary>
        /// 注册一个接受广播消费者
        /// </summary>
        /// <param name="exChangeName">交换机名称</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="autoDelete">是否自动删除</param>
        /// <param name="fn">接收到消息后的回调函数</param>
        /// <param name="_objCtx"></param>
        public static void FanoutConsumeRegister(string exChangeName,string queueName, bool autoDelete, Func<MQProtocol<M>, Exception,object, bool> fn,object _objCtx)
        {
            FanoutConsumer<MQProtocol<M>> c = new FanoutConsumer<MQProtocol<M>>(exChangeName, queueName, autoDelete,_objCtx);
            c.OnGrantMsgReceive += (MQProtocol<M> m, Exception ex,object objCtx) =>
            {
                return fn(m, ex, objCtx);
            };
            consumers.Add(c);
            c.Register();
        }

        /// <summary>
        /// 注册一个接受广播消费者
        /// </summary>
        /// <param name="exChangeName">交换机名称</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="host"></param>
        /// <param name="autoDelete"></param>
        /// <param name="fn"></param>
        /// <param name="_objCtx"></param>
        public static void FanoutConsumeRegister(string exChangeName, string queueName, VirtualHost host, bool autoDelete, Func<MQProtocol<M>, Exception,object, bool> fn,object _objCtx)
        {
            FanoutConsumer<MQProtocol<M>> c = new FanoutConsumer<MQProtocol<M>>(exChangeName, queueName, autoDelete, host,_objCtx);
            c.OnGrantMsgReceive += (MQProtocol<M> m, Exception ex,object objCtx) =>
            {
                return fn(m, ex, objCtx);
            };
            consumers.Add(c);
            c.Register();
        }

        #endregion

        /// <summary>
        /// 注册一个指定了Host的特定routerKey的拉消费者
        /// </summary>
        /// <param name="routerKey">注册接受特定的routerKey的消息</param>
        /// <param name="queueName">将特定的routerKey的消息接受到自己的queueName中</param>
        /// <param name="host">特定的host服务器</param>
        /// <param name="autoDelete">是否自动删除autoDelete，一般不要在注册的时候设置为true,在处理完消息的回调中删除消息</param>
        public static PullConsumer<MQProtocol<M>> PullConsumeRegister(string routerKey, string queueName, VirtualHost host, bool autoDelete)
        {
            PullConsumer<MQProtocol<M>> c = new PullConsumer<MQProtocol<M>>(ExchangeConst.DefaultExchange, routerKey, queueName, autoDelete, host);
            
            consumers.Add(c);
            c.Register();
            return c;
        }

        /// <summary>
        /// 凡是注册过消费者(ConsumeRegister)的，一定要在程序退出时调用此方法
        /// 释放掉所有的消费者,这里不释放会导致底层连接一直占着
        /// </summary>
        public static void Dispose()
        {
            if (consumers.Count > 0)
            {
                foreach (IDisposable c in consumers)
                {
                    c.Dispose();
                }
            }
        }
    }
}
