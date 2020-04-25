/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： GrantMicroService.Config
 文件名：   ConfigCenter
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/19 12:01:39

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.Config
{
    /// <summary>
    /// ConfigCenter
    /// </summary>
    public class ConfigCenter
    {
        /// <summary>
        /// 配置类型 
        /// </summary>
        public ConfigType ConfigType { get; set; }

        /// <summary>
        /// Gets or sets ip   zk的ip和端口是统一配置在一起的，主要考虑到zk集群部署会有多个节点  ip1:port1,ip2:port2
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 如果配置是zk  这个表示的zk连接的超时时间
        /// </summary>
        public int SessionTimeout { get; set; }
    }

    /// <summary>
    /// 配置类型
    /// </summary>
    public enum ConfigType
    {
        /// <summary>
        /// 本地配置
        /// </summary>
        Local = 1,

        /// <summary>
        /// zookeeper配置
        /// </summary>
        Zookeeper=2,

        /// <summary>
        /// 文件配置中心
        /// </summary>
        HttpFile=3,
    }
}