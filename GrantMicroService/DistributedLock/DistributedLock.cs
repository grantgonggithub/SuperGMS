using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GrantMicroService.Cache;
using GrantMicroService.Log;

namespace GrantMicroService.GrantLock
{
    /// <summary>
    /// 分布式锁，提供全局分布式锁支持，以resource redis为基础
    /// 这个锁只能通过RpcContext来获取，通过自己手动释放
    /// </summary>
    public sealed class DistributedLock
    {
        private static readonly TimeSpan DefaultAbandonmentCheckFrequency = TimeSpan.FromSeconds(2);
        private ILogger logger = LogFactory.CreateLogger<DistributedLock>();
        public readonly string lockName;
        private readonly string lockValue;
        private readonly int  checkTimeSpan = 50; //ms
        private readonly int autoDelete;

        private DistributedLock()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lockName"></param>
        /// <param name="autoDelete">自动删除，ms,默认 60s</param>
        /// <param name="checkTimeSpan">如果不能获取锁，重复检查间隔:默认 50ms</param>
        internal DistributedLock(string lockName, int autoDelete = 60000,int checkTimeSpan = 50)
        {
            // note that just Global\ is not a valid name
            if (string.IsNullOrEmpty(lockName))
                throw new ArgumentNullException("lockName不能为空");

            if (null == ResourceCache.Instance)
                throw new Exception(@"ResourceCache 没有配置或无法连接");

            this.checkTimeSpan = Math.Max(checkTimeSpan,1);
            this.autoDelete = Math.Max(autoDelete,1);
            this.lockName = lockName;
            this.lockValue = lockName;
        }

       
        /// <summary>
        /// 获取锁
        /// </summary>
        /// <param name="timeout">超时为null，则尝试一次即返回</param>
        /// <returns>获取锁成功?</returns>
        internal bool Acquire(TimeSpan? timeout = null)
        {
            bool bLock = false;
            var dtStart = DateTime.Now.Ticks;
            while (!bLock)
            {
                bLock = TryAcquireOnce();
                if (timeout == null)
                {
                    break;
                }
                if (!bLock)
                {
                    Thread.Sleep(this.checkTimeSpan);
                }

                var ts = new TimeSpan(DateTime.Now.Ticks - dtStart);
                if (ts >= timeout)
                {
                    break;
                }
            }

            return bLock;
        }

        //public void Dispose()
        //{
        //    GrantLockManager.ReleaseLock(this);
        //}

        internal void Release()
        {
            try
            {
                var bRtn = ResourceCache.Instance.LockRelease(this.lockName, this.lockValue);
                Trace.WriteLine($"释放锁 {this.lockName}:{bRtn}");
            }
            catch (Exception e)
            {
                logger.LogError(e, $"释放锁失败，系统自动超时释放:{this.lockName}");
            }
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        public void ReleaseLock()
        {
            GrantLockManager.ReleaseLock(this);
        }

        private bool TryAcquireOnce()
        {
            try
            {
                Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId}:TryAcquireOnce");
                var @lock = ResourceCache.Instance.LockTake(this.lockName, this.lockValue, new TimeSpan(0, 0, 0, 0, this.autoDelete));
                return @lock;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
    
}
