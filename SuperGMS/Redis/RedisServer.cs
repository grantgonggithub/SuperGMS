/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Redis
 文件名：RedisServer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/3/24 15:51:12

 功能描述：redis服务器信息

----------------------------------------------------------------*/

namespace SuperGMS.Redis
{
    /// <summary>
    /// 定义一个redis服务器对象
    /// </summary>
   public class RedisServer
    {
        /// <summary>
        /// 所在业务节点
        /// </summary>
        public RedisNode Node { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        public int Pool { get; set; }

        /// <summary>
        /// 是否主
        /// </summary>
        public bool IsMaster { get; set; }

        /// <summary>
        /// 服务器名称或者ip
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否可以执行带有风险性指令
        /// </summary>
        public bool AllowAdmin { get; set; }

        /// <summary>
        /// 链接超时时间，一般用默认（配置为0表示默认）
        /// </summary>
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// 同步时间，这个会影响redis做get操作的是否会超时
        /// </summary>
        public int SyncTimeout { get; set; }

        /// <summary>
        /// 是否需要密码
        /// </summary>
        public bool Ssl { get; set; }

        /// <summary>
        /// 新加redis ssl配置，为了兼容老的ssl 默认是false
        /// </summary>
        public bool Ssl2 { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// redis 数据库索引
        /// </summary>
        public int DbIndex { get; set; }
    }
}
