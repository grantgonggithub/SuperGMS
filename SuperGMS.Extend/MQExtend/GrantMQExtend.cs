/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Extend.MQExtend
 文件名：  GrantMQExtend
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/2/7 13:54:24

 功能描述：

----------------------------------------------------------------*/
using System;
using SuperGMS.Config;
using SuperGMS.Extend.BackGroundMessage;
using SuperGMS.MQ;
using SuperGMS.Protocol.MQProtocol;

namespace SuperGMS.Extend.MQExtend
{
    /// <summary>
    /// 接到消息的时间
    /// </summary>
    /// <param name="m">m</param>
    /// <param name="ex">ex</param>
    /// <returns>返回成功或者失败</returns>
    public delegate bool GrantMQMessageReceive<M>(MQProtocol<M> m, Exception ex)
        where M : class;

    /// <summary>
    /// 一个消息队列的封装
    /// </summary>
    /// <typeparam name="T">T消息类型</typeparam>
    public class GrantMQExtend<M>
        where M : class
    {
        /// <summary>
        /// 回调事件,注意返回值是true将会删除消息，false不删除
        /// </summary>
        public event GrantMQMessageReceive<M> OnGrantMQMessageReceive;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantMQExtend{M}"/> class.
        ///  初始化一个消息队列
        /// </summary>
        /// <param name="routerKeys">routerKeys</param>
        /// <param name="hostName">hostName</param>
        public GrantMQExtend(string hostName, params string[] routerKeys)
        {
            foreach (var itemKey in routerKeys)
            {
                GrantMQManager<M>.ConsumeRegister(
                    GetRouter(itemKey, hostName),
                    GetQueue(itemKey, hostName),
                    false,
                    (MQProtocol<M> m, Exception ex) =>
                    {
                        // if (ex != null)
                        // {
                        //    if (m != null)
                        //    {
                        // string msg = m.ToString();
                        // Console.WriteLine(msg);
                        // return true;
                        if (this.OnGrantMQMessageReceive != null)
                        {
                            return this.OnGrantMQMessageReceive(m, ex);
                        }

                        // }
                        // }
                        return false; // 如果没有回调，不能随意删除消息
                    });
            }
        }

        public bool SendMessage(M message, string eventName, string routerKey, string hostName)
        {
            var msg = new MQProtocol<M>(eventName, message, ServerSetting.AppName);
            msg.RouterKey = routerKey;
            return SendMessage(msg, hostName);
        }

        public bool SendMessage(MQProtocol<M> message, string hostName)
        {
            var routeKey = GetRouter(message.RouterKey, hostName);
            var host = MQHostConfigManager.GetHost(hostName);
            return GrantMQManager<M>.Publish(message, routeKey, host);
        }

        /// <summary>
        /// 获取router
        /// </summary>
        /// <param name="bussinessType">业务类型</param>
        /// <returns>routerkey</returns>
        private static string GetRouter(string bussinessType, string host)
        {
            return $"route-{host}_{bussinessType.Trim().ToLower()}";
        }

        private static string GetQueue(string bussinessType, string host)
        {
            return $"queue-{host}_{bussinessType.Trim().ToLower()}";
        }
    }
}
