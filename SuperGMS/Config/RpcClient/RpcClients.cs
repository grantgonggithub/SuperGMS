/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Config
 文件名：   Configuration
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/19 11:56:47

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using SuperGMS.Router;
using SuperGMS.Rpc;

namespace SuperGMS.Config
{
    public class RpcClients
    {
        /// <summary>
        /// 
        /// </summary>
        public List<Client> Clients { get; set; }
    }
    public class Client
    {
        /// <summary>
        /// 
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RouterType RouterType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ClientItem> Items { get; set; }
    }
    public class ClientItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int Pool { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// TimeOut
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// ServerType
        /// </summary>
        public ServerType ServerType { get; set; }

        private bool enable = true;

        /// <summary>
        /// 是否启用 ，本地配置不用这个，缺省为true
        /// </summary>
        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }
    }

}