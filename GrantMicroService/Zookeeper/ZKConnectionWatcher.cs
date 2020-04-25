/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： GrantMicroService.Zookeeper
 文件名：   ZKConnectionWatcher
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/16 14:57:20

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using org.apache.zookeeper;
using GrantMicroService.Config;
using GrantMicroService.Log;

namespace GrantMicroService.Zookeeper
{
    /// <summary>
    /// ZKConnectionWatcher
    /// </summary>
    public class ZKConnectionWatcher : BaseWatcher
    {
        public override Task process(WatchedEvent @event)
        {
            Event.KeeperState state = @event.getState();
            switch (state)
            {
                case Event.KeeperState.AuthFailed:
               // case Event.KeeperState.Disconnected:
                case Event.KeeperState.Expired:
                //case Event.KeeperState.SyncConnected:
                    var task = new Task(() => { Thread.Sleep(1000); this.CallBack(null); });
                    task.Start();
                    return task;
            }

            return Task.FromResult(1);
        }
    }
}