/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Thrift.Server
 文件名：GrantServerConfig
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 14:52:35

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace SuperGMS.Rpc
{
    /// <summary>
    /// 服务配置
    /// </summary>
    public class GrantServerConfig : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets 如果是基于Socket的调用需要这个参数（如Thrift）
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 集中配置
        /// </summary>
        public Dictionary<string, int> PortList { get; set; }

        /// <summary>
        /// Gets or sets server类型
        /// </summary>
        public ServerType ServerType { get; set; }

        /// <summary>
        /// Gets or sets 需要初始化程序集的路径
        /// </summary>
        public string AssemblyPath { get; set; }

        /// <summary>
        /// Gets or sets 服务的逻辑名称，如message,kpi
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets 服务编号
        /// </summary>
        public int Pool { get; set; }

        /// <summary>
        /// 预留一个Ip，因为在复杂的网络环境中，一个主机可能会有多个网卡和ip，这样就只能手工指定某个ip了
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 客户端超时时间
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// 是否被下线
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 配置信息序列化
        /// </summary>
        /// <returns>拼接字符串</returns>
        public override string ToString()
        {
            return string.Format("ServerName={0},Port={1},ServerType={2},AssemblyPath={3}，Pool={4},Ip={5},TimeOut={6},Enable={7}", ServerName, Port, ServerType, AssemblyPath,Pool,Ip,TimeOut,Enable);
        }
    }
}