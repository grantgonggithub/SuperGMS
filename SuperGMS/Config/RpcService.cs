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
    public class RpcService
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
        /// 集中配置是的服务和Port列表
        /// </summary>
        public Dictionary<string, int> PortList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ServerType ServerType { get; set; }

        /// <summary>
        /// 路由方式
        /// </summary>
        public RouterType RouterType { get; set; } = RouterType.Random;

        /// <summary>
        /// 
        /// </summary>
        public string AssemblyPath { get; set; }

        private int timeout;

        /// <summary>
        ///  timeout
        /// </summary>
        public int TimeOut
        {
            get { return timeout; }
            set { timeout = value; }
        }

        private bool enable = true;
        /// <summary>
        /// 当前服务是否被标记为可用，默认可用，除非被标记，这个标记变了之后，本地也要修改，下次启动需要根据这个值来处理
        /// </summary>
        public bool Enable {
            get { return enable; }
            set { enable = value; }
        }
    }
}