/*----------------------------------------------------------------
Copyright (C) 2021 顺兴文旅版权所有

项目名称：SuperGMS.WebSocket
文件名：  WebSocketManager
创建者：  grant(巩建春)
CLR版本： 4.0.30319.42000
时间：    2021/8/11 星期三 17:08:50

功能描述：
----------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperGMS.WebSocketEx
{
    /// <summary>
    /// WebSocketManager
    /// </summary>
    public class SuperWebSocketManager
    {
        //private static ConcurrentDictionary<string, SuperWebSocket> _sockets = new ConcurrentDictionary<string, SuperWebSocket>();

        //public bool OnConnected(SuperWebSocket webSocket)
        //{
        //    return _sockets.TryAdd(webSocket.Token, webSocket);
        //}

        //public bool OnClose(string token)
        //{
        //    if (string.IsNullOrEmpty(token)) return true;
        //    var socket=_sockets.TryAdd(token)
        //}

    }
}
