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


        /// <summary>
        /// 发布一条默认配置的消息
        /// </summary>
        /// <typeparam name="M"></typeparam>
        /// <param name="msg">msg</param>
        /// <returns>bool</returns>
        // public static bool Publish(MQProtocol<M> msg)
        // {
        //    // GrantDefaultPublisher<MQProtocol<M>> p = new GrantDefaultPublisher<MQProtocol<M>>(msg,false);
        //    // return p.Publish();
        //    return Publish(msg, RouterKeyConst.DefaultRouterKey);
        // }

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
        /// 指定虚拟机发送默认消息
        /// </summary>
        /// <param name="msg">msg</param>
        /// <param name="host">host</param>
        /// <returns>bool</returns>
        // public static bool Publish(MQProtocol<M> msg, VirtualHost host)
        // {
        //   return Publish(msg, RouterKeyConst.DefaultRouterKey, host);
        // }

        /// <summary>
        /// 注册一个默认配置的消费者，等待消息的到来
        /// </summary>
        /// <typeparam name="M">返回消息的实体类型</typeparam>
        /// <param name="autoDelete">是否自动删除,true为自动删除，false不会自动删除，需要业务自己删除</param>
        /// <param name="fn">
        /// 处理返回消息的回调，注意如果autoDelete指定为false说明调用方需要自己确定
        /// 消息什么时候删除，这里就需要fn返回是否删除，autoDelete指定为true时，fn这个返回值将不起作用
        /// </param>
        /// <returns></returns>
        // public static void ConsumeRegister(bool autoDelete,Func<MQProtocol<M>, Exception, bool> fn)
        // {
        //    // GrantDefaultConsumer<MQProtocol<M>> c = new GrantDefaultConsumer<MQProtocol<M>>(autoDelete);
        //    // c.OnGrantMsgReceive += (MQProtocol<M> m, Exception ex) =>
        //    // {
        //    //   return fn(m, ex);
        //    // };
        //    // consumers.Add(c);
        //    // c.Register();
        //    ConsumeRegister(RouterKeyConst.DefaultRouterKey, autoDelete, fn);
        // }

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
            using (Publisher<MQProtocol<M>> p = new Publisher<MQProtocol<M>>(msg, ExchangeConst.DefaultExchange, routerKey, MQueueConst.DefaultGrantMQ, false, host))
            {
                // consumers.Add(p);
                return p.Publish();
            }                
        }

        /// <summary>
        /// 注册一个指定了routerKey的消费者
        /// </summary>
        /// <param name="routerKey">特定的routerKey这个用户特定标识特殊消息</param>
        /// <param name="autoDelete">是否自动删除,true为自动删除，false不会自动删除，需要业务自己删除</param>
        /// <param name="fn">/// 处理返回消息的回调，注意如果autoDelete指定为false说明调用方需要自己确定
        /// 消息什么时候删除，这里就需要fn返回是否删除，autoDelete指定为true时，fn这个返回值将不起作用
        /// </param>
        public static void ConsumeRegister(string routerKey, string queueName, bool autoDelete, Func<MQProtocol<M>, Exception, bool> fn)
        {
            DefaultConsumer<MQProtocol<M>> c = new DefaultConsumer<MQProtocol<M>>(routerKey, queueName, autoDelete);
            c.OnGrantMsgReceive += (MQProtocol<M> m, Exception ex) =>
            {
                return fn(m, ex);
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
        public static void ConsumeRegister(string routerKey, string queueName, VirtualHost host, bool autoDelete, Func<MQProtocol<M>, Exception, bool> fn)
        {
            Consumer<MQProtocol<M>> c = new Consumer<MQProtocol<M>>(ExchangeConst.DefaultExchange, routerKey, queueName, autoDelete, host);
            c.OnGrantMsgReceive += (MQProtocol<M> m, Exception ex) =>
            {
                return fn(m, ex);
            };
            consumers.Add(c);
            c.Register();
        }

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
        /// 注册一个指定了Host的消费者
        /// </summary>
        /// <param name="host">host</param>
        /// <param name="autoDelete">autoDelete</param>
        /// <param name="fn">fn</param>
        // public static void ConsumeRegister(VirtualHost host, bool autoDelete, Func<MQProtocol<M>, Exception, bool> fn)
        // {
        //    ConsumeRegister(RouterKeyConst.DefaultRouterKey, host, autoDelete, fn);
        // }

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
