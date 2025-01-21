/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Zookeeper
 文件名：   BaseWatcher
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/18 11:25:52

 功能描述：

----------------------------------------------------------------*/
using org.apache.zookeeper;

namespace SuperGMS.Zookeeper
{
    public delegate void WatcherCallBack(string path);

    /// <summary>
    /// BaseWatcher
    /// </summary>
    public abstract class BaseWatcher : Watcher
    {
        public event WatcherCallBack OnChange;

        public void CallBack(string path)
        {
            if (OnChange != null)
            {
                OnChange(path);
            }
        }
    }
}