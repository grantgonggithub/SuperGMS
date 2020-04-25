/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.Protocol
 文件名：MQProtocol
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 10:50:07

 功能描述：定义一个MQ的协议外壳

----------------------------------------------------------------*/
using Newtonsoft.Json;

namespace GrantMicroService.Protocol.MQProtocol
{
    /// <summary>
    /// MQ的消息
    /// </summary>
    /// <typeparam name="M"></typeparam>
    public class MQProtocol<M> : EventMsg<M> where M:class
    {
        private string _exChange;

        /// <summary>
        /// 所属交换机
        /// </summary>
        public string ExChange { get { return _exChange; } set { _exChange = value; } }

        private string _routerKey;

        /// <summary>
        /// 路由关键字
        /// </summary>
        public string RouterKey { get { return _routerKey; } set { _routerKey = value; } }
        /// <summary>
        /// MQ的消息
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        public MQProtocol(string eventName, M msg, string from)
            : base(eventName, msg, null, from)
        {

        }

        /// <summary>
        /// 将字符串转换成MQProtocol
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static MQProtocol<M> Parse(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return null;
            return JsonConvert.DeserializeObject<MQProtocol<M>>(msg);
        }
        /// <summary>
        /// 将对象转出Json字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
