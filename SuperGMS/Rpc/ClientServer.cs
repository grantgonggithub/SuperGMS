/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantRpc.Client
 文件名：GrantServer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/5/8 16:15:39

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using SuperGMS.Log;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Router;

namespace SuperGMS.Rpc
{
    /// <summary>
    /// 客户端所对应的所有的可调用的服务端
    /// </summary>
    public class ClientServer
    {
        private ReaderWriterLock readerWriterLock = new ReaderWriterLock();
        private Dictionary<string, ClientItem> clients = new Dictionary<string, ClientItem>();
        private readonly static ILogger logger = LogFactory.CreateLogger<ClientServer>();
        private int nextIndex = 0;
        /// <summary>
        /// Gets or sets logic ServerName
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets 负载策略
        /// </summary>
        public RouterType RouterType { get; set; }

        /// <summary>
        /// Gets 所有负载节点
        /// </summary>
        public ClientItem[] Client
        {
            get
            {
                try
                {
                    readerWriterLock.AcquireReaderLock(80);
                    return clients.Values.ToArray();
                }
                catch
                {
                    return new ClientItem[0];
                }
                finally
                {
                    if (readerWriterLock.IsReaderLockHeld)
                    {
                        readerWriterLock.ReleaseReaderLock();
                    }
                }
            }
        }

        /// <summary>
        /// 根据路由规则获取一个客户端
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public ClientItem GetOne(Args<object> args)
        {
            try
            {
                readerWriterLock.AcquireReaderLock(80);
                var cls = clients.Values.ToArray();
                int idx = 0;
                switch (this.RouterType)
                {
                    case RouterType.Hash:
                        idx = RouterManager.GetPool(args.uri);
                        break;
                    default:
                    case RouterType.Polling:
                        Interlocked.Increment(ref nextIndex);
                        Interlocked.CompareExchange(ref nextIndex, 0, cls.Length);
                        idx = nextIndex;
                        break;
                    case RouterType.Random:
                        idx = RouterManager.GetRandom(0, cls.Length);
                        break;
                }
                return cls[idx];
            }
            catch {
                throw;
            }
            finally
            {
                if (readerWriterLock.IsReaderLockHeld)
                {
                    readerWriterLock.ReleaseReaderLock();
                }
            }

        }

        /// <summary>
        /// 更新客户端列表
        /// </summary>
        /// <param name="li">更新列表</param>
        /// <param name="update">update=true,delete =false</param>
        public void UpdateClient(ClientItem[] li, bool update)
        {
            try
            {
                readerWriterLock.AcquireWriterLock(100);
                clients.Clear();
                for (int i = 0; i < li.Length; i++)
                {
                    string key = string.Format("{0}_{1}", li[i].Ip, li[i].Port);
                    clients[key] = li[i];
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ClientServer.UpdateClient.Error");
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
        /// 输出ServerName 和 RouterType
        /// </summary>
        /// <returns>输出内容</returns>
        public override string ToString()
        {
            return string.Format("ServerName={0},RouterType={1}", ServerName, RouterType);
        }
    }
}