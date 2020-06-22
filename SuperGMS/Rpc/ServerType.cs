/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.GrantRpc
 文件名：ServerType
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/5/8 15:41:24

 功能描述：

----------------------------------------------------------------*/
using System;

namespace SuperGMS.Rpc
{
    /// <summary>
    /// 服务器发布类型
    /// </summary>
   public enum ServerType
    {
        /// <summary>
        /// 服务器接口发布为
        /// </summary>
        WCF=1,
        /// <summary>
        /// 服务器接口发布为ThriftRpc
        /// </summary>
        Thrift=2,
        /// <summary>
        /// 服务接口发布为Grpc
        /// </summary>
        Grpc=3,

        /// <summary>
        /// 不用发布接口，只执行定时任务
        /// </summary>
        TaskWorker=4,

        /// <summary>
        /// 代理层
        /// </summary>
        HttpProxy = 5,

        /// <summary>
        /// 跨网关http
        /// </summary>
        Http =6,

        /// <summary>
        /// 做为WebApi直接处理业务
        /// </summary>
        HttpWebApi=7,

    }

    /// <summary>
    /// 服务器发布类型转换类
    /// </summary>
    public class ServerTypeParse
    {
        public static ServerType Parse(string serverType)
        {
            ServerType r =ServerType.Thrift;
            if (!Enum.TryParse<ServerType>(serverType,true, out r))
                r = ServerType.WCF;
            return r;
        }
    }
}
