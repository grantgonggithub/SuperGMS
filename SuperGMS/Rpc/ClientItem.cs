/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Thrift.Client
 文件名：ClientConfig
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:15:52

 功能描述：

----------------------------------------------------------------*/

namespace SuperGMS.Rpc
{
    /// <summary>
    /// 客户端配置
    /// </summary>
    public class ClientItem
    {
        /// <summary>
        /// Gets or sets ip
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets 服务器发布类型
        /// </summary>
        public ServerType ServerType { get; set; }

        /// <summary>
        /// Gets or sets 如果是Wcf配置到根路径
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets 服务编号
        /// </summary>
        public int Pool { get; set; }

        /// <summary>
        /// Timeout
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// 是否被下线
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets 所在的logic Server
        /// </summary>
        public ClientServer Server { get; set; }

        /// <summary>
        /// 拼接所有属性
        /// </summary>
        /// <returns>所有属性</returns>
        public override string ToString()
        {
            return string.Format("Ip={0},Port={1},ServerType={2},Url={3},Server={4}", Ip, Port, ServerType, Url, Server.ToString());
        }
    }
}