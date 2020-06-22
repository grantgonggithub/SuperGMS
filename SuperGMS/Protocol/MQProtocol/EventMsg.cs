/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Protocol.MQProtocol
 文件名：EventMsg
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 11:07:36

 功能描述：定义一个消息的外壳

----------------------------------------------------------------*/
using System;

namespace SuperGMS.Protocol.MQProtocol
{
    /// <summary>
    /// 消息外壳
    /// </summary>
    /// <typeparam name="M"></typeparam>
    [Serializable]
    public class EventMsg<M> where M : class
    {
        /// <summary>
        /// 消息
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="msg">消息实体</param>
        /// <param name="to">接受者，一个消息可以不标记接受者如广播消息</param>
        /// <param name="from">发送者，一个消息必须标记发送者即来源</param>
        public EventMsg(string eventName, M msg, string to, string from)
        {
            this._eventName = eventName;
            this._msg = msg;
            this._to = to;
            this._from = from;
            this._msg_id = Guid.NewGuid().ToString("N");//消息生成时产生唯一编号
        }

        private string _msg_id;
        /// <summary>
        /// 消息唯一编号
        /// </summary>
        public string Msg_id
        {
            get { return _msg_id; }
            set { _msg_id = value; }
        }

        private string _eventName;

        /// <summary>
        /// 事件或消息名称
        /// </summary>
        public string EventName
        {
            get { return _eventName; }
            set { _eventName = value; }
        }

        private M _msg;
        /// <summary>
        /// 消息内容
        /// </summary>
        public M Msg
        {
            get { return _msg; }
            set { _msg = value; }
        }

        private string _to;
        /// <summary>
        /// 消息接收者
        /// </summary>
        public string To
        {
            get { return _to; }
            set { _to = value; }
        }

        private string _from;
        /// <summary>
        /// 消息发送者
        /// </summary>
        public string From
        {
            get { return _from; }
            set { _from = value; }
        }

        private object _ctx;
        /// <summary>
        /// 需要传递的上下文内容
        /// </summary>
        public object Context
        {
            get { return _ctx; }
            set { _ctx = value; }
        }
    }
}
