/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantRpc.Server
 文件名：AppContext
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 16:24:14

 功能描述：一个用于传递上下文信息的容器

----------------------------------------------------------------*/

using Microsoft.EntityFrameworkCore;

using SuperGMS.DB.EFEx;
using SuperGMS.DB.EFEx.GrantDbContext;
using SuperGMS.GrantLock;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Router;
using SuperGMS.UserSession;

using System;
using System.Collections.Generic;

namespace SuperGMS.Rpc.Server
{
    /// <summary>
    /// 一个用于传递上下文信息的容器
    /// </summary>
    public class RpcContext : IDisposable
    {
        private readonly object rootLock = new object();
        private Dictionary<string, HeaderValue> headers;
        private UserContext userContext;
        private Dictionary<string, IEFDbContext> dbContexts = new Dictionary<string, IEFDbContext>();
        private List<DistributedLock> locks=new List<DistributedLock>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RpcContext"/> class.
        /// 一个用于传递上下文信息的容器
        /// </summary>
        /// <param name="objValue">objValue 这个一定不是RpcContext,而是用户自己想传的自己的值</param>
        /// <param name="args">用户请求的参数</param>
        public RpcContext(object objValue, Args<object> args)
        {
            this.ContextValue = objValue;
            this.Args = args;
            this.Language = this.Args.lg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RpcContext"/> class.
        /// 一个用于传递上下文信息的容器
        /// </summary>
        public RpcContext()
        {
        }

        /// <summary>
        /// Gets or sets 业务自己或者调用方想传递的上下文信息
        /// </summary>
        public object ContextValue { get; set; }

        /// <summary>
        /// Gets or sets 客户端请求的原始参数
        /// </summary>
        public Args<object> Args { get; set; }

        /// <summary>
        /// Gets or sets 可以路由的Uri
        /// </summary>
        public IUri Uri { get; set; }

        /// <summary>
        /// Gets 请求的头信息
        /// </summary>
        public Dictionary<string, HeaderValue> Headers
        {
            get { return headers; }
            internal set { headers = value; }
        }

        /// <summary>
        /// Gets or sets 语言
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 访问一个非上下文用户的Rpc请求时使用
        /// </summary>
        /// <param name="rpc">上下文的RpcContext</param>
        /// <param name="tk">非上下文用户的Token</param>
        /// <returns>一个非上下文的RpcContext</returns>
        public static RpcContext CloneByToken(RpcContext rpc, string tk)
        {
            var rtx = new RpcContext(null, new Args<object>() { tk = tk, rid = rpc.Args?.rid });
            rtx.headers = rpc.headers;
            return rtx;
        }

        /// <summary>
        /// 获取当前用户, 在当前的Rpc请求周期内，只会初始化一次
        /// </summary>
        /// <returns>用户上下文</returns>
        public UserContext GetUserContext()
        {
            if (userContext == null)
            {
                lock (rootLock)
                {
                    if (userContext == null && !string.IsNullOrEmpty(this.Args.tk))
                    {
                        userContext = UserContext.GetUserContext(this.Args.tk); 
                    }
                }
            }
            return userContext;
        }

        /// <summary>
        /// 注意：这个默认取的是EFDbContext,要用DapperDbContext请用GetDapperDbContext（）
        /// 根据当前用户上下文来获取DbContext
        /// 在同一个实现了GrantRpcBaseServer的上下文RpcContext同一个类型的
        /// DBContext共享同一个，在Process方法返回之后将会被全部销毁，所以在同一个
        /// RpcBaseServer中是可以保证事物的
        /// </summary>
        /// <param name="newOne">是否新创建，不共享，在多线程相互隔离的上下文中使用</param>
        /// <param name="dbModelName">自定义的数据库连接模型 如：UserDbContext.0  UserDbContext.1 UserDbContext.2 和数据库配置件相对应</param>
        /// <typeparam name="TContext">tcontext</typeparam>
        /// <returns>IGrantDbContext</returns>
        public IEFDbContext GetDbContext<TContext>(bool newOne = false, string dbModelName = null)
            where TContext : DbContext
        {
            string key = null;
            if (newOne)
            {
                lock (rootLock)
                {
                    key = Guid.NewGuid().ToString("N");
                    IEFDbContext newOneValue = SuperGMSDBContext.GetEFContext<TContext>(this,dbModelName);
                    // 这里不加 lock 放入是为了回收，不用关心脏不脏 , 如果丢了, 则不能手工回收调用Dispose, 只能靠系统自动回收
                    dbContexts.Add(key, newOneValue);
                    return newOneValue;
                }
            }

            key = string.IsNullOrWhiteSpace(dbModelName) ? typeof(TContext).FullName.ToLower() : dbModelName;
            if (dbContexts.ContainsKey(key))
            {
                return dbContexts[key];
            }
            else
            {
                lock (rootLock)
                {
                    // 再检查一次，防止同时进入 else
                    if (dbContexts.ContainsKey(key))
                    {
                        return dbContexts[key];
                    }

                    var dbctx = SuperGMSDBContext.GetEFContext<TContext>(this,dbModelName);
                    dbContexts.Add(key, dbctx);
                    return dbctx;
                }
            }
        }

        /// <summary>
        /// 获取Dapper的DbContext
        /// 之所以要引入DapperDbContext是为了保持和EF在语法结构上的一致性，对上层开发人员不至于变化太大
        /// </summary>
        /// <typeparam name="TContext">TContext</typeparam>
        /// <param name="dbModelName">自定义的数据库连接模型如：UserDbContext.0  UserDbContext.1 UserDbContext.2 和数据库配置件相对应</param>
        /// <returns>IGrantDapperDbContext</returns>
        public IDapperDbContext GetDapperDbContext<TContext>(string dbModelName = null)
        {
            // Dapper的DbContext不需要换成，只是一个空壳引用，没有任何性能问题，唯一的数据库连接使用连接池
            return SuperGMSDBContext.GetDapperContext(this, string.IsNullOrWhiteSpace(dbModelName) ? typeof(TContext).Name : dbModelName);
        }

        /// <summary>
        /// 释放的时候就不锁了，因为只有框架在释放，外部业务类看不到
        /// </summary>
        public void Dispose()
        {
            locks.ForEach(x => { LockManager.ReleaseLock(x);});
            if (dbContexts != null && dbContexts.Count > 0)
            {
                foreach (var db in dbContexts.Keys)
                {
                    var ctx = dbContexts[db];
                    if (ctx != null)
                    {
                        ctx.Dispose();
                    }
                }

                dbContexts.Clear();
                dbContexts = null;
            }
        }

        /// <summary>
        /// 在当前上下文中获取一个分布式锁，第一个获取锁的将执行依赖当前key的完整业务流程（包括多个微服务之间的调用和数据库的访问）
        /// 后来者将无法获取锁，根据返回的结果来判断是否进入流程，如果返回的锁为null将不能执行下面的流程，要么重试等待锁释放，要么返回错误
        /// var qtLock=TryGetLock(lockKey);
        /// if(qtLock==null) 提示不能同时执行操作； return；
        /// else  进行业务流程
        /// 最后别忘了  qtLock.ReleaseLock();
        /// </summary>
        /// <param name="lockKey">要锁定的key，这里可以是单据号，或者业务中不允许并行处理的业务key</param>
        /// <param name="timeOut">获取等待时间，如果锁被占用，等待释放的时间，默认不等待</param>
        /// <param name="autoReleaseTime">自动释放的时间，建议用默认值</param>
        /// <param name="autoRelease">是否需要自动释放，默认是true,在一个rpc生命周期结束时自动释放，如果为false，需要外部记录key并手动释放，调用方法（ResourceCache.Instance.LockRelease）</param>
        /// <returns></returns>
        public DistributedLock TryGetLock(string lockKey, int timeOut = 0,
            int autoReleaseTime = 60 * 1000, bool autoRelease = true)
        {
            var l = LockManager.TryGetLock(lockKey, timeOut, autoReleaseTime);
            if (l == null)
            {
                return null;
            }
            if (autoRelease)
                locks.Add(l);
            return l;
        }
    }
}