/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.GrantMQ
 文件名：ConfigManager
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 13:59:36

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using GrantMicroService.Log;
using GrantMicroService.MQ.RabbitMQ;

namespace GrantMicroService.MQ
{
    public class MQHostConfigManager
    {
        public const string RabbitMQ = "RabbitMQ";
        private static Dictionary<string,VirtualHost> _host=new Dictionary<string, VirtualHost>();

        private static ReaderWriterLock readerWriterLock = new ReaderWriterLock();

        private readonly static ILogger logger = LogFactory.CreateLogger<MQHostConfigManager>();

        public static void Initlize(Config.RabbitMQ el)
        {
            try
            {
                readerWriterLock.AcquireWriterLock(80);
                if (el != null && el.Host != null && el.Host.Count > 0)
                {
                    for (int i = 0; i < el.Host.Count; i++)
                    {
                        int port = 0;
                        VirtualHost h = new VirtualHost();
                        h.HostName = el.Host[i].Name;
                        h.Host = el.Host[i].Ip;
                        h.Port = el.Host[i].Port;
                        h.Username = el.Host[i].UserName;
                        h.Password = el.Host[i].PassWord;
                        h.NoAckMsgCount = el.Host[i].NoAckMsgCount;

                        // 如果配置了同名，跳过
                        if (!_host.ContainsKey(h.HostName.ToLower()))
                        {
                            _host.Add(h.HostName.ToLower(), h);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "初始化RabbbitMQ异常");
            }
            finally
            {
                if (readerWriterLock.IsWriterLockHeld)
                {
                    readerWriterLock.ReleaseWriterLock();
                }
            }
        }

        /// <summary>
        /// 获取指定的Host
        /// </summary>
        /// <param name="host">host</param>
        /// <returns>VirtualHost</returns>
        public static VirtualHost GetHost(string host)
        {
            try
            {
                readerWriterLock.AcquireReaderLock(80);
                if (_host.ContainsKey(host.ToLower()))
                {
                    return _host[host.ToLower()];
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MQHostConfigManager.GetHost().Error");
            }
            finally
            {
                if (readerWriterLock.IsReaderLockHeld)
                {
                    readerWriterLock.ReleaseReaderLock();
                }
            }

            return null;

            /*   new VirtualHost() {
                Host= "192.168.100.102",
                HostName=VirtualHostConst.DefaultVirtualHost,
                Username= "Grant",
                Password= "Grant123",
                Port= 5672
           };*/
        }

        /// <summary>
        /// 获取默认Host
        /// </summary>
        /// <returns>VirtualHost</returns>
        public static VirtualHost GetDefaultHost()
        {
            return GetHost("Default");
        }
    }
}