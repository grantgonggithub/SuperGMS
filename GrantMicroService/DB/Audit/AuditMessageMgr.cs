using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using GrantMicroService.Log;
using GrantMicroService.MQ;
using GrantMicroService.MQ.Consumer;
using GrantMicroService.Protocol.MQProtocol;

namespace GrantMicroService.DB.Audit
{
    /// <summary>
    /// 接到审计消息的时间
    /// </summary>
    /// <param name="m">m</param>
    /// <param name="ex">ex</param>
    /// <returns>返回成功或者失败</returns>
    public delegate bool AuditMessageReceive<T>(MQProtocol<T> m, Exception ex) where T : GrantAudit;
    /// <summary>
    /// 审计消息管理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AuditMessageMgr<T> where T : GrantAudit
    {
        private readonly static ILogger logger = LogFactory.CreateLogger("AuditMessageMgr<T>");
        private GrantPullConsumer<MQProtocol<T>> _consumer = null;
        /// <summary>
        /// Initializes a new instance of the .
        /// 创建接受消息实例
        /// </summary>
        /// <param name="groups">接受消息的所有Ttid</param>
        public AuditMessageMgr(params string[] groups) : this(false, groups)
        {

        }

        /// <summary>
        /// Initializes a new instance of the  class.
        /// 创建接受消息实例
        /// </summary>
        /// <param name="groups">接受消息的所有Ttid</param>
        /// <param name="pull">拉消息</param>
        public AuditMessageMgr(bool pull, params string[] groups)
        {
            var host = MQHostConfigManager.GetHost("Audit");
            if (host == null)
            {
                throw new Exception("RabbitMQ 没有配置  Audit 主机!!");
            }

            if (!pull)
            {
                // 订阅所有的routerKey
                foreach (var item in groups)
                {
                    GrantMQManager<T>.ConsumeRegister(
                        GetRouter(item),
                        GetQueue(item),
                        host,
                        true,
                        (MQProtocol<T> m, Exception ex) =>
                        {
                            if (this.OnAuditMessageReceive != null)
                            {
                                return this.OnAuditMessageReceive(m, ex);
                            }
                            return false; // 如果没有回调，不能随意删除消息
                        });
                }
            }
            else
            {
                if (groups.Length > 0)
                {
                    Debug.Assert(true);
                }

                var item = typeof(T).Name;

                // 订阅所有的routerKey                
                this._consumer = GrantMQManager<T>.PullConsumeRegister(
                    GetRouter(item),
                    GetQueue(item),
                    host,
                    true);
            }
        }

        /// <summary>
        /// 处理拉消息
        /// </summary>
        /// <param name="fn"></param>
        public void DealPullMessage(Func<T, Exception, bool> fn)
        {
            if (this._consumer != null)
            {
                this._consumer.DealPullMsg((msg, ex) => {
                    if (msg == null)
                    {
                        return true;
                    }

                    if (fn == null)
                    {
                        return false;
                    }
                    try
                    {
                        return fn(msg.Msg, ex);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, exception.Message);
                        return false;
                    }
                });
            }
        }

        /// <summary>
        /// 回调事件,注意返回值是true将会删除消息，false不删除
        /// </summary>
        public event AuditMessageReceive<T> OnAuditMessageReceive;

        /// <summary>
        /// 保存异步消息，并设置初始处理状态
        /// </summary>
        /// <param name="valueArgs">参数</param>
        /// <returns>是否成功</returns>
        public static bool SetMessage(T valueArgs)
        {
            if (valueArgs == null)
            {
                return true;
            }

            var msg = new MQProtocol<T>("AuditMessage", valueArgs, Guid.NewGuid().ToString("N"));
            var routeKey = GetRouter(valueArgs.Ttid);
            var host = MQHostConfigManager.GetHost("Audit");
            var mq = GrantMQManager<T>.Publish(msg, routeKey, host);
            return mq;
        }

        /// <summary>
        /// 获取router
        /// </summary>
        /// <param name="ttid">ttid</param>
        /// <returns>routerkey</returns>
        private static string GetRouter(string ttid)
        {
            if (ttid == null)
            {
                ttid = string.Empty;
            }

            return "route-AuditMessageMgr-" + ttid.Trim().ToLower();
        }

        private static string GetQueue(string ttid)
        {
            if (ttid == null)
            {
                ttid = string.Empty;
            }

            return "queue-AuditMessageMgr-" + ttid.Trim().ToLower();
        }
    }
}
