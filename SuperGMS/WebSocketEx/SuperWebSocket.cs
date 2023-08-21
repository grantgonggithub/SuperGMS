/*----------------------------------------------------------------
Copyright (C) 2021 顺兴文旅版权所有

项目名称：SuperGMS.SuperWebSocket
文件名：  SuperWebSocket
创建者：  grant(巩建春)
CLR版本： 4.0.30319.42000
时间：    2021/8/11 星期三 17:12:21

功能描述：
----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace SuperGMS.WebSocketEx
{
    public delegate void ConnectionDelegate(WebSocketReceiveResult receiveResult);
    /// <summary>
    /// SuperWebSocket
    /// </summary>
    //public class SuperWebSocket
    //{
    //    private WebSocket _webSocket;
    //    private string _token;

    //    public string Token
    //    {
    //        get { return _token; }
    //    }

    //    public event ConnectionDelegate OnConnection;

    //    public SuperWebSocket(WebSocket webSocket,string token)
    //    {
    //        this._webSocket = webSocket;
    //        this._token = token;
    //    }

    //    public bool SendMessage(string msg)
    //    {
    //        return true;
    //    }
    //}
}
