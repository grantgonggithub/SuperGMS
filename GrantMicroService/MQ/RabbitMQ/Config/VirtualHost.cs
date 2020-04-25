/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.RabbitMQ.Config
 文件名：VirtualHost
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/20 14:23:04

 功能描述：一个MQ的虚拟Host主要是用于用户隔离

----------------------------------------------------------------*/

namespace GrantMicroService.MQ.RabbitMQ
{
    /// <summary>
    /// 虚拟主机
    /// </summary>
    public class VirtualHost
    {
        /// <summary>
        /// 虚拟主机名
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// 物理主机，ip或者机器名称，用于建立连接
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 登录名
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 最大允许未Ack的消息数量
        /// </summary>
        public ushort NoAckMsgCount { get; set; }


        public override string ToString()
        {
            return $"HostName:{HostName},Host:{Host},Port:{Port},NoAckMsgCount:{NoAckMsgCount}";
        }
    }
}
