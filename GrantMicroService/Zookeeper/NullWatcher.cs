/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Zookeeper
 文件名：  NullWatcher
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/3/9 15:43:55

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using org.apache.zookeeper;

namespace GrantMicroService.Zookeeper
{
    /// <summary>
    /// NullWatcher
    /// </summary>
    public class NullWatcher : Watcher
    {
        internal static readonly Task CompletedTask = Task.FromResult(1);
        public static readonly NullWatcher Instance = new NullWatcher();

        private NullWatcher()
        {
        }

        /// <inheritdoc/>
        public override Task process(WatchedEvent @event)
        {
            return CompletedTask;
        }
    }
}
