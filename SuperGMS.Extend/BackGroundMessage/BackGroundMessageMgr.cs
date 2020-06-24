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
    using System;
    using Microsoft.Extensions.Logging;
    using SuperGMS.Cache;
    using SuperGMS.Log;
    using SuperGMS.MQ;
    using SuperGMS.Protocol.MQProtocol;

    /// <summary>
    /// 接到消息的时间
    /// </summary>
    /// <param name="m">m</param>
    /// <param name="ex">ex</param>
    /// <returns>返回成功或者失败</returns>
    public delegate bool BackGroundMessageReceive(MQProtocol<SetBackGroudMessageArgs> m, Exception ex);

    /// <summary>
    /// BackGroundMessageMgr
    /// </summary>
    public class BackGroundMessageMgr
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<BackGroundMessageMgr>();
        /// <summary>
        /// Initializes a new instance of the <see cref="BackGroundMessageMgr"/> class.
        /// 创建接受消息实例
        /// </summary>
        /// <param name="bussinessTypes">接受消息的所有bussinessTypes</param>
        public BackGroundMessageMgr(params string[] bussinessTypes)
        {
            Task.Run(() =>
            {
                // 订阅所有的routerKey
                foreach (var item in bussinessTypes)
                {
                    MQManager<SetBackGroudMessageArgs>.ConsumeRegister(
                        GetRouter(item),
                        GetQueue(item),
                        false,
                        (MQProtocol<SetBackGroudMessageArgs> m, Exception ex) =>
                        {
                            // if (ex != null)
                            // {
                            //    if (m != null)
                            //    {
                            // string msg = m.ToString();
                            // Console.WriteLine(msg);
                            // return true;
                            if (this.OnBackGroundMessageReceive != null)
                            {
                                return this.OnBackGroundMessageReceive(m, ex);
                            }

                            // }
                            // }
                            return false; // 如果没有回调，不能随意删除消息
                        });
                }
            });
        }

        /// <summary>
        /// 回调事件,注意返回值是true将会删除消息，false不删除
        /// </summary>
        public event BackGroundMessageReceive OnBackGroundMessageReceive;

        /// <summary>
        /// 保存异步消息，并设置初始处理状态
        /// </summary>
        /// <param name="valueArgs">参数</param>
        /// <returns>是否成功</returns>
        public static bool SetMessage(SetBackGroudMessageArgs valueArgs)
        {
            var msg = new MQProtocol<SetBackGroudMessageArgs>("SetBackGroudMessage", valueArgs, valueArgs.Args.rid);
            var routeKey = GetRouter(valueArgs.BussinessType);
            var mq = MQManager<SetBackGroudMessageArgs>.Publish(msg, routeKey);

            if (!mq)
            {
                logger.LogError($"BackGroundMessageMgr.SetMessage.MQ.Error.rid = {valueArgs.Args.rid}");
                return false;
            }

            logger.LogInformation($"BackGroundMessageMgr.SetMessage.MQ.Success.rid = {valueArgs.Args.rid}");

            var processMsg = new BackGroundMessageProcessResult() // 初始化一个未开始状态，所有值都为-1，表示还在队列中，未开始
            {
                ProcessNum = -1,
                SuccessNum = -1,
                TotalNum = -1,
                Data = string.Empty,
                Rid = valueArgs.Args.rid,
            };

            return SetProcessStatus(processMsg);
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="processMsg">信息</param>
        /// <returns>是否成功</returns>
        public static bool SetProcessStatus(BackGroundMessageProcessResult processMsg)
        {
           return ResourceCache.Instance.Set(processMsg.Rid, processMsg);
        }

        /// <summary>
        /// 获取处理进度信息
        /// </summary>
        /// <param name="taskGuid">taskGuid</param>
        /// <returns>BackGroundMessageProcessResult</returns>
        public static BackGroundMessageProcessResult GetProcessStatus(string taskGuid)
        {
            return ResourceCache.Instance.Get<BackGroundMessageProcessResult>(taskGuid);
        }

        /// <summary>
        /// 获取router
        /// </summary>
        /// <param name="bussinessType">业务类型</param>
        /// <returns>routerkey</returns>
        private static string GetRouter(string bussinessType)
        {
            return $"route-BackGroundMessageMgr-{bussinessType.Trim().ToLower()}";
        }

        private static string GetQueue(string bussinessType)
        {
            return $"queue-BackGroundMessageMgr-{bussinessType.Trim().ToLower()}";
        }
    }
}
