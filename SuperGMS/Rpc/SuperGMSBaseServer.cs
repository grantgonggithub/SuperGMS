/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Thrift.Server
 文件名：GrantServer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 14:03:03

 功能描述：实现一个thrift的服务

----------------------------------------------------------------*/

using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using SuperGMS.Config;
using SuperGMS.Log;
using SuperGMS.Rpc.Server;
using SuperGMS.Tools;
using SuperGMS.Zookeeper;

namespace SuperGMS.Rpc
{
    /// <summary>
    /// 实现一个Rpc Base服务 , 该服务提供注册方法, 以及发送消息方法, 以及接受消息的方法
    /// </summary>
    public abstract class SuperGMSBaseServer : ISuperGMSRpcServer
    {
        /// <summary>
        /// 服务发送, distributer 把收到的信息转发给Thrift
        /// </summary>
        private RpcDistributer distributer;
        protected readonly static ILogger logger = LogFactory.CreateLogger<SuperGMSBaseServer>();
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="server">服务配置</param>
        public void RpcServerRegister(SuperGMSServerConfig server)
        {
            try
            {
                distributer = new RpcDistributer(server);  // 这里会初始化serverSetting和zk连接以及其他配置

                // 注册zk登记自己(包括serverName,ip,port,pool)
                server.ServerName = distributer.ShortName; // 兼容集中配置
                if (server.PortList!=null && server.PortList.ContainsKey(server.ServerName))
                {
                    server.Port = server.PortList[server.ServerName];
                    ServerSetting.UpdateRpcPort(server.Port);
                }

                bool IsExit = false;
                // 要先初始化系统相关组件之后才能注册Rpc端口，要不请·求上来了，还没有初始化其他的，会有问题
                // 去zk注册自己
                if (ServerSetting.ConfigCenter.ConfigType == ConfigType.Zookeeper)
                {
                    Task.Run(() =>
                    {
                        Random r = new Random();
                        // rpc注册socket是阻塞的，只能提前通过线程注册， 等待1s 再注册路由，这个过程中，如果rpc端口不成功，将会撤销资源退出，这个注册也就失败了，防止先注册路由，然后再rpc注册异常，导致路由瞬间抖动，造成瞬间无效广播；
                        Thread.Sleep(r.Next(1500,2500));  // 随机等待在1s--2s之间吧，防止集群一瞬间同时重启，容易形成广播风暴，形成雪崩；
                        if (IsExit)
                        {
                            return;
                        }
                        ServerSetting.RegisterRouter(server.ServerName, server.Ip, server.Port,
                            ServerSetting.GetRpcServer().Enable, server.TimeOut);
                        logger.LogInformation($"\r\n服务名：{server.ServerName}，开始监听Ip是:{server.Ip}，端口是：{server.Port}\r\n*******************▄︻┻┳══━一zookeeper路由注册成功，系统启动成功！▄︻┻┳══━一*******************");
                    });
                }
                else
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(1500);
                        if (IsExit)
                        {
                            return;
                        }
                        logger.LogInformation($"\r\n服务名：{server.ServerName}，开始监听Ip是:{server.Ip}，端口是：{server.Port}\r\n*******************▄︻┻┳══━一启用本地配置，系统启动成功！▄︻┻┳══━一*******************");
                    });
                }
                ServerRegister(server); // 底层通讯和业务进行绑定
                // 只有socket才会阻塞
                if (server.ServerType == ServerType.Thrift || server.ServerType == ServerType.Grpc)
                {
                    IsExit = true; // 通知所有注册方，停止注册，系统要撤销所有资源了，防止其他异常，优雅一点点，不能太暴力
                    logger.LogWarning($"\r\n rpc端口监听异常退出：{server.ServerName}{server.Port}{server.AssemblyPath}" + "  \r\n time=" +
                        DateTime.Now.ToString("yy-MM-dd HH:mm:ss:ffff"));

                    if (ServerSetting.ConfigCenter.ConfigType == ConfigType.Zookeeper)
                    {
                        ZookeeperManager.ClearRouter(server.ServerName, server.Ip);
                    }

                    // rpc监听是hold住当前线程的，因为底层吞掉异常了，跑到这里来就说明异常了，要彻底释放
                    // 让他等一下，把日志写完
                    System.Threading.Thread.Sleep(1000);

                    // 如果启动失败，一定要彻底清理退出，因为在线程中，只能这样，要不只是线程退出，主程序还运行
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "\r\n Error 服务异常退出  " + DateTime.Now.ToString("yy-MM-dd HH:mm:ss:ffff"));

                // 让他等一下，把日志写完
                System.Threading.Thread.Sleep(1000);

                // 如果启动失败，一定要彻底清理退出，因为在线程中，只能这样，要不只是线程退出，主程序还运行
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void QtDispose()
        {
            try
            {
                distributer.Dispose(); // 先释放业务
                Dispose(); // 在释放框架
                logger.LogInformation("\r\n 应用程序停止，并成功回收GrantBaseServer.QtDispose");
            }
            catch (Exception e)
            {
                logger.LogError(e, "GrantBaseServer.QtDispose.Error");
            }

            // 让他等一下，把日志写完
            System.Threading.Thread.Sleep(1000);

            Environment.Exit(0);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="my_args">消息参数</param>
        /// <param name="appContext">上下文</param>
        /// <returns>发送结果</returns>
        public string Send(string my_args, object appContext)
        {
            return OnReceive(my_args, appContext);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="server">服务配置</param>
        protected abstract void ServerRegister(SuperGMSServerConfig server);

        /// <summary>
        /// 抽象方法,重写后释放服务
        /// </summary>
        protected abstract void Dispose();

        /// <summary>
        /// 接受到请求之后上层处理是一样的，不需要子类特殊实现，
        /// 但是必须提供这样的可能性
        /// </summary>
        /// <param name="args">通信内容</param>
        /// <param name="appContext">上下文</param>
        /// <returns>返回接收人</returns>
        protected virtual string OnReceive(string args, object appContext)
        {
            try
            {
                // 用当前绑定的分发器进行分发
                return this.distributer.Distributer(args, appContext);
            }
            catch (Exception ex)
            {
                string msg = "{\"rid\": \"" + Guid.NewGuid().ToString("N") + "\",\"c\": 500,\"msg\": \"GrantBaseServer.OnReceive.Error....." + ex.Message + "\"}";
                logger.LogError(ex, $"GrantBaseServer.OnReceive.Error{args}");
                return msg;
            }
        }
    }
}