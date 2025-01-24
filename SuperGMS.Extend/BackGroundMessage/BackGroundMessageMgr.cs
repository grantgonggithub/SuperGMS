/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Extend.BackGroundMessage
 文件名：  BackGroundMessageMgr
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/30 11:23:08

 功能描述：

----------------------------------------------------------------*/

using System.Threading.Tasks;

namespace SuperGMS.Extend.BackGroundMessage
{
    using Microsoft.Extensions.Logging;

    using SuperGMS.Log;
    using SuperGMS.MQ;
    using SuperGMS.Protocol.MQProtocol;
    using SuperGMS.Tools;

    using System;
    using System.Linq;

    /// <summary>
    /// 接到消息的时间
    /// </summary>
    /// <param name="m">m</param>
    /// <param name="ex">ex</param>
    /// <returns>返回成功或者失败</returns>
    public delegate bool BackGroundMessageReceive(MQProtocol<SetBackGroudMessageArgs> m, Exception ex,object objCtx);

    public class MessageRouterMap { 
        /// <summary>
        /// 处理业务的接口名称
        /// </summary>
        public Type BussinessApi { get; set; }

        /// <summary>
        /// MQ的消息路由名，如果是点对点的消息，这个值是消息发布者定义Router,如果是扇波消息这个值是消息发布者定义的ExChangeName
        /// </summary>
        public string MQRouterName { get; set; }

        /// <summary>
        /// 在fanout消息时，自己定义的queue可以自定义，如果不自定义框架会用BussinessApi.Name+ServiceEnvironment.ComputerAddress，保证每台机器的Queue不重复，
        /// 如果自定义要考虑不重复的问题，重复了，最后定义的机器才能收到消息
        /// </summary>
        public string QueueName { get; set; } = null;
    }
    /// <summary>
    /// 点对点消息
    /// </summary>
    public class BackGroundDirectMessage
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<BackGroundDirectMessage>();
        /// <summary>
        /// Initializes a new instance of the <see cref="BackGroundDirectMessage"/> class.
        /// 创建接受消息实例
        /// </summary>
        /// <param name="bussinessTypes">接受消息的所有bussinessTypes</param>
        public BackGroundDirectMessage(params MessageRouterMap[] messageRouterMap)
        {
            if (messageRouterMap?.Any(x => x.BussinessApi==null || string.IsNullOrEmpty(x.MQRouterName)) ?? true)
                throw new Exception("BackGroundDirectMessage构造函数的参数不合法");
            Task.Run(() =>
            {
                // 订阅所有的routerKey
                foreach (var item in messageRouterMap)
                {
                    MQManager<SetBackGroudMessageArgs>.ConsumeRegister(
                        GetRouter(item.MQRouterName), //点对点的消息，必须安装消息发布者的router和队列
                        GetQueue(item.MQRouterName), //同上
                        false,
                        (MQProtocol<SetBackGroudMessageArgs> m, Exception ex,object objCtx) =>
                        {
                            if (this.OnBackGroundMessageReceive != null)
                            {
                                return this.OnBackGroundMessageReceive(m, ex,objCtx);
                            }
                            return false; // 如果没有回调，不能随意删除消息
                        },item.BussinessApi.Name);
                }
            });
        }

        /// <summary>
        /// 回调事件,注意返回值是true将会删除消息，false不删除
        /// </summary>
        public event BackGroundMessageReceive OnBackGroundMessageReceive;


        /// <summary>
        /// 获取router
        /// </summary>
        /// <param name="bussinessType">业务类型</param>
        /// <returns>routerkey</returns>
        public static string GetRouter(string mqRouterName)
        {
            return $"route-direct-{mqRouterName.Trim().ToLower()}";
        }

        /// <summary>
        /// 点对的消息是所有机器共享一个队列，所有Queue是一个
        /// </summary>
        /// <param name="mqRouterName"></param>
        /// <returns></returns>
        public static string GetQueue(string mqRouterName)
        {
            return $"queue-direct-{mqRouterName.Trim().ToLower()}";
        }
    }


    /// <summary>
    /// 扇波消息
    /// </summary>
    public class BackGroundFanoutMessage
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<BackGroundFanoutMessage>();
        /// <summary>
        /// Initializes a new instance of the <see cref="BackGroundFanoutMessage"/> class.
        /// 创建和初始化扇波的订阅方
        /// </summary>
        /// <param name="bussinessTypes">接受消息的所有bussinessTypes</param>
        public BackGroundFanoutMessage(params MessageRouterMap[] messageRouterMap)
        {
            if (messageRouterMap?.Any(x =>x.BussinessApi==null || string.IsNullOrEmpty(x.MQRouterName)) ?? true)
                throw new Exception("BackGroundFanoutMessage构造函数的参数不合法");
            Task.Run(() =>
            {
                // 订阅所有的routerKey
                foreach (var item in messageRouterMap)
                {
                    MQManager<SetBackGroudMessageArgs>.FanoutConsumeRegister(
                        GetExchange(item.MQRouterName),
                        GetQueue(item.MQRouterName,item.BussinessApi.Name,item.QueueName),// 这里特殊，因为是扇波消息，所以这个队列是自己定义的特有的，所以用MQRouterName+ApiName 相当于是按MQRouterName+ApiName进行分组的，相同分组多节点，只有一个节点收到
                        false,
                        (MQProtocol<SetBackGroudMessageArgs> m, Exception ex,object objCtx) =>
                        {
                            if (this.OnBackGroundMessageReceive != null)
                            {
                                return this.OnBackGroundMessageReceive(m, ex, objCtx);
                            }
                            return false; // 如果没有回调，不能随意删除消息
                        },item.BussinessApi.Name);
                }
            });
        }

        /// <summary>
        /// 回调事件,注意返回值是true将会删除消息，false不删除
        /// </summary>
        public event BackGroundMessageReceive OnBackGroundMessageReceive;


        /// <summary>
        /// 获取扇波的交换机
        /// </summary>
        /// <param name="bussinessType">业务类型</param>
        /// <returns>routerkey</returns>
        public static string GetExchange(string mqRouterName)
        {
            return $"exchange-fanout-{mqRouterName.Trim().ToLower()}";
        }

        /// <summary>
        /// 获取扇波的队列,这个队列是每个消费者自己定义的，由交互机投递到这个队列的,多台机器要区分队列，每台机器对应一个队列，区别点对点消息，要不变成一个Queue了
        /// </summary>
        /// <param name="bussinessType"></param>
        /// <returns></returns>
        public static string GetQueue(string mqRouterName,string bussinessApiName,string queueName)
        {
            if (!string.IsNullOrEmpty(queueName))
            {
                return $"queue-fanout-{mqRouterName.Trim().ToLower()}-{queueName}";
            }
            else
            {
                return $"queue-fanout-{mqRouterName.Trim().ToLower()}-{bussinessApiName.Trim().ToLower().Replace(".", "_")}-{ServiceEnvironment.ComputerAddress}";
            }
        }
    }
}
