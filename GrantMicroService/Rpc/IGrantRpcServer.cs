/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.GrantRpc
 文件名：IGrantRpcServer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/5/8 15:14:32

 功能描述：

----------------------------------------------------------------*/

namespace GrantMicroService.Rpc
{
    /// <summary>
    /// 微服务接口
    /// </summary>
    public interface IGrantRpcServer : Iface
    {
        /// <summary>
        /// 程序停止时，回收系统资源，包括调用应用标记的回收方法
        /// </summary>
        /// <returns>回收结果</returns>
        void QtDispose();

        /// <summary>
        /// 注册一个本地的服务
        /// </summary>
        /// <param name="server">服务配置</param>
        void RpcServerRegister(GrantServerConfig server);
    }

    /// <summary>
    /// 消息接口, Thrift 直接调用此接口的 Send 方法
    /// </summary>
    public interface ISync
    {
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="my_args">参数</param>
        /// <param name="appContext">调用上下文</param>
        /// <returns>返回结果</returns>
        string Send(string my_args, object appContext);
    }

    public interface Iface : ISync
    {
    }
}