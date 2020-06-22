﻿/*----------------------------------------------------------------
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
using System.Linq;
using System.Text;
using SuperGMS.Config;

namespace SuperGMS.Config
{
    /// <summary>
    /// Configuration
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// ConfigPath
        /// </summary>
        public string ConfigPath { get; set; }
        /// <summary>
        /// 基础配置
        /// </summary>
        public GrantConfig GrantConfig { get; set; }

        /// <summary>
        /// 数据库
        /// </summary>
        public DataBase DataBase { get; set; }

        /// <summary>
        /// 日志
        /// </summary>
        public LogConfig.LogConfig ConfigLog { get; set; }

        /// <summary>
        /// 用户自定义常量 特殊处理
        /// </summary>
        public ConstKeyValue ConstKeyValue { get; set; }

        /// <summary>
        /// RabbitMQ
        /// </summary>
        public RabbitMQ RabbitMQ { get; set; }

        /// <summary>
        /// Redis
        /// </summary>
        public RedisConfig RedisConfig { get; set; }

        /// <summary>
        /// RpcClients
        /// </summary>
        public RpcClients RpcClients { get; set; }

        /// <summary>
        /// HttpProxy
        /// </summary>
        public HttpProxy HttpProxy { get; set; }

        /// <summary>
        /// FileServer
        /// </summary>
        public FileServer FileServer { get; set; }

        //非拷贝，屏蔽敏感字符串
        public Configuration SimpleClone()
        {
            List<HostItem> his = new List<HostItem>();
            List<RedisNode> rns = new List<RedisNode>();
            this.RabbitMQ?.Host?.ForEach(x =>
            {
                his.Add(new HostItem
                {
                    Ip = x.Ip,
                    Name = x.Name,
                    PassWord = "******",
                    Port = x.Port,
                    UserName = x.UserName,
                });
            });
            this.RedisConfig?.Nodes?.ForEach(x =>
            {
                rns.Add(new RedisNode
                {
                    NodeName = x.NodeName,
                    IsMasterSlave = x.IsMasterSlave,
                    Items = x.Items?.Select(o=>new RedisItem
                    {
                        AllowAdmin = o.AllowAdmin,
                        ConnectTimeout = o.ConnectTimeout,
                        IsMaster = o.IsMaster,
                        Pool = o.Pool,
                        Port = o.Port,
                        Server= o.Server,
                        Ssl=o.Ssl,
                        Pwd = "******"
                    }).ToList()
                });
            });
            var cfg = new Configuration
            {
                GrantConfig = this.GrantConfig,
                DataBase = this.DataBase,
                ConfigLog = this.ConfigLog,
                ConstKeyValue = this.ConstKeyValue,
                RabbitMQ = new RabbitMQ
                {
                    Host = his
                },
                RedisConfig = new RedisConfig
                {
                    Nodes = rns
                },
                ConfigPath = this.ConfigPath,
                RpcClients = this.RpcClients,
                HttpProxy = this.HttpProxy,
                FileServer = this.FileServer,
            };

            return cfg;
        }
    }
}