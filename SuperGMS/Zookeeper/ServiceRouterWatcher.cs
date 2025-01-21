/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Zookeeper
 文件名：  ServiceWatcher
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/2/23 17:05:26

 功能描述：监听微服务状态，包括注册，离线

----------------------------------------------------------------*/
using org.apache.zookeeper;

using System.Threading.Tasks;

namespace SuperGMS.Zookeeper
{
    /// <summary>
    /// ServiceRouterWatcher
    /// </summary>
    public class ServiceRouterWatcher : BaseWatcher
    {
        public override Task process(WatchedEvent @event)
        {
            Event.EventType eventType = @event.get_Type();
            string path = @event.getPath();
            if (eventType != Event.EventType.None)
            {
                switch (eventType)
                {
                    case Event.EventType.NodeChildrenChanged:
                    case Event.EventType.NodeCreated:
                    case Event.EventType.NodeDataChanged:
                    case Event.EventType.NodeDeleted:

                        var tsk = new Task(() => { this.CallBack(path); });
                        tsk.Start();
                        return tsk;
                }
            }

            return Task.FromResult(1);
        }
    }
}
