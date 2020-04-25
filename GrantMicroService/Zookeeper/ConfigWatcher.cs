/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Zookeeper
 文件名：  DataWatcher
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/2/23 17:05:53

 功能描述：监听数据的变化，主要是配置信息

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using org.apache.zookeeper;
using GrantMicroService.Log;

namespace GrantMicroService.Zookeeper
{
    /// <summary>
    /// DataWatcher  监控数据节点变化，全部都是永久性节点，不存在动态增加和删除的功能，所以这里只关注Changed的节点
    /// </summary>
    public class ConfigWatcher : BaseWatcher
    {
        /// <summary>
        /// process
        /// </summary>
        /// <param name="event">event</param>
        /// <returns>Task</returns>
        public override Task process(WatchedEvent @event)
        {
            // 写日志
            Event.EventType eventType = @event.get_Type();
            if (eventType != Event.EventType.None)
            {
                string path = @event.getPath();
                // Event.KeeperState keeperState = @event.getState();
                switch (eventType)
                {
                    case Event.EventType.NodeChildrenChanged:
                    case Event.EventType.NodeCreated:
                    case Event.EventType.NodeDataChanged:
                        var tsk = new Task(() => { this.CallBack(path); });
                        tsk.Start();
                        return tsk;
                    case Event.EventType.NodeDeleted:
                        // 写日志忽略变更
                        break;
                }
            }

            return Task.CompletedTask;
        }
    }
}