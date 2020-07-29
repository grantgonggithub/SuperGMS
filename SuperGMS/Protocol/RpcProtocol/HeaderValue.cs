/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Protocol.RpcProtocol
 文件名：  Header
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/21 15:23:20

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.Protocol.RpcProtocol
{
    /// <summary>
    /// Header
    /// </summary>
   public class HeaderValue
    {
        /// <summary>
        /// 请求客户端ip，这个主要由httpProxy获取前端请求的ip地址，
        /// 注意这里不是ngix或者slb获取前端硬件负载的ip，是一个外网地址
        /// </summary>
        public const string REMOTEIP = "remote_ip";

        /// <summary>
        /// 远程请求的引用url，这个主要有httpProxy获取并添加
        /// 一般用来验证来源url
        /// </summary>
        public const string REFFER = "ref_url";

        /// <summary>
        /// 这个主要记录内部请求客户端的ip
        /// </summary>
        public const string INNERIP = "inner_ip";
        /// <summary>
        /// 客户端设备信息
        /// </summary>
        public const string USERAGENT= "user_agent";

        public HeaderValue(string serverName, string key, object value)
        {
            this.serverName = serverName;
            this.key = key;
            this._value = value;
            ts = DateTime.Now;
        }

        private string serverName;

        private string key;

        private object _value;

        /// <summary>
        /// 用于标记这个值是哪个服务器添加的，组成结构：微服务名称(pool)_ip:port_机器名称
        /// </summary>
        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; }
        }

        /// <summary>
        /// 放入值得key
        /// </summary>
        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        /// <summary>
        /// 放入的value
        /// </summary>
        public object Value {
            get { return _value; }
            set { _value = value; }
        }

        private DateTime ts;

        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTime Ts
        {
            get { return ts; }
            set { ts = value; }
        }
    }
}
