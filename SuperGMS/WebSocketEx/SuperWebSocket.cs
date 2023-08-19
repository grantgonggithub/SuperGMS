/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.WebSocketEx
 文件名：SuperWebSocket
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/3/24 14:35:06

 功能描述：

----------------------------------------------------------------*/

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SuperGMS.HttpProxy;
using SuperGMS.Log;
using SuperGMS.Protocol.MQProtocol;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc.Server;

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperGMS.WebSocketEx
{
    /// <summary>
    /// SuperWebSocket
    /// </summary>
    public class SuperWebSocket
    {
        private readonly ILogger _loger = LogFactory.CreateLogger<SuperWebSocket>();
        private WebSocket _socket;

        private string _token;

        private DateTime _loginDateTime;

        private DateTime _lastActiveTime;

        private Args<object> _loginArgs;

        private object _lock=new object();

        /// <summary>
        /// 存储这个主要是想判断用户是否在线
        /// </summary>
        public Args<object> LoginArgs
        {
            get { return _loginArgs; }
        }

        /// <summary>
        /// 当前的WebSocket
        /// </summary>
        public WebSocket Socket
        {
            get { return _socket; }
            //set { _socket = value; }
        }

        /// <summary>
        /// 当前的Token
        /// </summary>
        public string Token {
            get { return _token; }
            //set { _token = value; }
        }

        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime LoginDateTime {
            get { return _loginDateTime; }
            //set { _loginDateTime = value; }
        }

        /// <summary>
        /// 最后活跃时间
        /// </summary>
        public DateTime LastActiveTime { 
          get { return _lastActiveTime; }
          //set { _lastActiveTime = value; }
        }

        private bool _isClear = false;

        /// <summary>
        /// 构造WebSocket对象
        /// </summary>
        /// <param name="_socket"></param>
        /// <param name="_loginDateTime"></param>
        /// <param name="_lastActiveTime"></param>
        /// <param name="_loginArgs"></param>
        public SuperWebSocket(WebSocket _socket, DateTime _loginDateTime, DateTime _lastActiveTime, Args<object> _loginArgs)
        {
            this._socket = _socket;
            this._token = _loginArgs.tk;
            this._loginDateTime = _loginDateTime;
            this._lastActiveTime = _lastActiveTime;
            this._loginArgs = _loginArgs;
            _isClear = false;
        }

        /// <summary>
        /// 是否心跳超时,给外部调用
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            lock (_lock) {
                if (DateTime.Now.Subtract(_lastActiveTime).TotalMilliseconds > SuperWebSocketManager.TimeOutSpan)
                {
                    close(WebSocketCloseStatus.EndpointUnavailable, "客户端连接被清理断开");
                    _isClear = true;
                }
            }
        }

        /// <summary>
        /// 关闭并清理
        /// </summary>
        /// <param name="webSocketCloseStatus"></param>
        /// <param name="msgPrifx"></param>
        private Task close(WebSocketCloseStatus webSocketCloseStatus,string msgPrifx)
        {
            if(!_isClear)
            {
                SuperWebSocketManager.OnClose(Token);
                _loger.LogInformation($"{msgPrifx}：{JsonConvert.SerializeObject(_loginArgs, SuperHttpProxy.jsonSerializerSettings)}");
                return this._socket?.CloseAsync(webSocketCloseStatus, null, CancellationToken.None);
            }
            return Task.CompletedTask;
        }



        /// <summary>
        ///  当前WebSocket客户端发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(EventMsg<string> msg)
        {
            sendMessage(msg.Msg);
        }

        /// <summary>
        /// 监听连接数据
        /// </summary>
        /// <returns></returns>
        public Task OnConnetcion()
        {
            _loger.LogInformation($"客户端连接成功：{JsonConvert.SerializeObject(_loginArgs, SuperHttpProxy.jsonSerializerSettings)}");
            sendMessage("pong"); // 连接成功给客户端发个消息
            try
            {
                var buffer = new byte[1024 * 4];
                var socketResult = this._socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
                while (!socketResult.CloseStatus.HasValue)
                {

                    lock (_lock)
                    {
                        this._lastActiveTime = DateTime.Now;
                    }

                    var msg = Encoding.UTF8.GetString(buffer);
                    msg = msg.Trim('\0', ' ');
                    _loger.LogInformation($"收到客户端消息(tk={Token})：{msg}");
                    Task.Run(() =>
                    {
                        var respone = "";//检测心跳，客户端定时发ping服务器响应pong
                        if (msg == null || msg.ToLower() == "ping")
                            respone = "pong";
                        else
                            respone = SuperWebSocketProxy.Send(msg, _loginArgs.Headers); // 发送到后端服务器
                        sendMessage(respone);
                    });
                    buffer = new byte[1024 * 4];
                    socketResult = this._socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;

                }
                // 如果是主动被清理线程关闭的，已经执行过了此操作，不重复操作
                //if(!_isClear)
                //{
                //    SuperWebSocketManager.OnClose(Token);
                //    _loger.LogInformation($"：{JsonConvert.SerializeObject(_loginArgs, SuperHttpProxy.jsonSerializerSettings)}");
                //     this._socket?.CloseAsync(socketResult.CloseStatus.Value, null, CancellationToken.None);
                //}
               return close(socketResult.CloseStatus.Value, "客户端连接丢失断开");
            }
            catch (Exception e)
            {
                _loger.LogError(e, "SuperWebSocket.OnConnetcion.Error");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        private void sendMessage(string msg)
        {
            try {
                _loger.LogInformation($"发送给客户端消息(tk={Token}): {msg}");
                var ms = Encoding.UTF8.GetBytes(msg);
                lock (_lock)// 不能并发的发送，要不消息就乱了,必须依次发送
                {
                    _socket?.SendAsync(new ArraySegment<byte>(ms, 0, ms.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _loger.LogError(ex, "SuperWebSocket.sendMessage.Error");
            }

        }
    }
}
