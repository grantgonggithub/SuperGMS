/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.WebSocketEx
 文件名：SuperWebSocketManager
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/3/24 14:35:06

 功能描述：

----------------------------------------------------------------*/

using Microsoft.Extensions.Logging;

using SuperGMS.Cache;
using SuperGMS.Log;
using SuperGMS.Protocol.MQProtocol;
using SuperGMS.Tools;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace SuperGMS.WebSocketEx
{
    /// <summary>
    /// WebSocketManager
    /// </summary>
    public class SuperWebSocketManager
    {
        /// <summary>
        /// 超时时间，单位毫秒，（15分钟）
        /// </summary>
        public const int TimeOutSpan= 15 * 60 * 1000;
        private static readonly ILogger _loger = LogFactory.CreateLogger<SuperWebSocketManager>();
        private static ConcurrentDictionary<string,ComboxClass<UserType,SuperWebSocket>> _sockets = new ConcurrentDictionary<string, ComboxClass<UserType, SuperWebSocket>>();

        /// <summary>
        /// 初始化检查线程
        /// </summary>
       public static void Initlize()
        {
            Thread thread = new Thread(() =>
            {
                initlize();
            });
            thread.Name = "Check.SuperWebSocket.TimeOut";
            thread.IsBackground = true;
            thread.Start();
        }

        private static void initlize()
        { 
            while (true)
            {
                _loger.LogInformation($"开始执行清理线程{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                try {
                    _sockets.Values.ToList().ForEach(s => {
                        s.V2.Close(true);
                    });
                    Thread.Sleep(10 * 60 * 1000); // 每5分钟检查一次
                }
                catch (Exception ex)
                {
                    _loger.LogError(ex, "SuperWebSocketManager.initlize.Error");
                }
            }
        }

        /// <summary>
        /// 客户端是否已经建立了socket，不可以重复创建
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsExistClient(string token)
        {
            return _sockets.ContainsKey(token);
        }

        /// <summary>
        /// 连接创建成功以后需要维护连接和token的关系
        /// </summary>
        /// <param name="webSocket"></param>
        public static void OnConnected(ComboxClass<UserType, SuperWebSocket> webSocket)
        {
            //加入管理器
            if (_sockets.TryAdd(webSocket.V2.Token, webSocket))
            {
                // 进入消息监听
                webSocket.V2.OnConnetcion();
            }
            else // 加入失败，说明已经存在，把新连接close掉
            {
                webSocket.V2.Close();
            }
        }

        /// <summary>
        /// 连接关闭需要从管理器中移除
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool OnClose(string token)
        {
            if (string.IsNullOrEmpty(token)) return true;
            bool isOK = _sockets.TryRemove(token, out _);
            _loger.LogInformation($"当前在线用户：{string.Join(",", _sockets.Keys)}");
            return isOK;
        }

        /// <summary>
        /// 按照消息指定的To数组，群发给对于的客户端
        /// </summary>
        /// <param name="msg"></param>
        public static void SendMessage(EventMsg<object> msg)
        {
            if (msg.Broadcast == Broadcast.None) // 非广播消息，以to为准
            {
                msg?.To?.Distinct().ToList()?.ForEach(to =>
                {
                    if (_sockets.TryGetValue(to, out var webSocket))
                        webSocket.V2.SendMessage(msg);
                });
            }
            else
            {
                _sockets.Values.ToList().ForEach(sc => {
                    if (msg.Broadcast == Broadcast.AllUser && sc.V1 == UserType.User)
                    {
                        sc.V2.SendMessage(msg);
                    }
                    else if (msg.Broadcast == Broadcast.AllEmployee && sc.V1 == UserType.Employee)
                    {
                        sc.V2.SendMessage(msg);
                    }
                    else
                        sc.V2.SendMessage(msg);
                    // 未指定类型将不发送
                });
            }
        }

    }

    /// <summary>
    /// 用户类型
    /// </summary>
   public enum UserType
    { 
        /// <summary>
        /// 用户
        /// </summary>
        User=1,

        /// <summary>
        /// 后台管理
        /// </summary>
        Employee=2,
    }
}
