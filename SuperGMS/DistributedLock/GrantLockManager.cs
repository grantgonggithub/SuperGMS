/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.DistributedLock
 文件名：   GrantLockManager
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/7/18 15:37:54

 功能描述：

----------------------------------------------------------------*/
using Microsoft.Extensions.Logging;
using SuperGMS.Log;
using System;

namespace SuperGMS.GrantLock
{
    /// <summary>
    /// GrantLockManager
    /// </summary>
    internal static class GrantLockManager
    {
        private readonly static ILogger logger = LogFactory.CreateLogger(typeof(GrantLockManager).FullName);
        /// <summary>
        /// 如果返回结果为null，说明获取锁失败
        /// </summary>
        /// <param name="lockKey"></param>
        /// <param name="timeOut"></param>
        /// <param name="autoReleaseTime"></param>
        /// <returns></returns>
        internal static DistributedLock TryGetLock(string lockKey, int timeOut = 0, int autoReleaseTime = 60 * 1000)
        {
            DistributedLock l = new DistributedLock(lockKey, autoReleaseTime, 50);
            TimeSpan? ts = null;
            if (timeOut > 0)
            {
                ts = new TimeSpan(0, 0, 0, 0, timeOut);
            }
            bool isOk = l.Acquire(ts);
            if (isOk)
            {
                //Stopwatch s = new Stopwatch();
                //var obj = new ComboxClass<Stopwatch, DistributedLock, int>() {V1 = s, V2 = l, V3 = autoReleaseTime};
                //Locks[lockKey]= obj; // 有可能存在，有可能新建
                //s.Start(); // 开始本地计时
                return l;
            }
            else
            {
                // 这里有两种情况，1、本地超时了，但是redis没超时，本地的还继续留着，后面就走redis，跟本地没关系了；2、这个超时是别的机器建的，也只能走redis，因为我本地不知道当时对方锁的释放时间
                //if (Locks.ContainsKey(lockKey))
                //{
                ;
                //}
            }
            return null;
        }

        internal static void ReleaseLock(DistributedLock dis)
        {
            try
            {
                dis.Release();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"GrantLockManager.ReleaseLock.Error.{dis.lockName}");
            }
        }
    }
}