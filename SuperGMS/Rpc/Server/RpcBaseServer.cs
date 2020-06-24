/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantRpc.Server
 文件名：IGrantRpcServer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:41:00

 功能描述：

----------------------------------------------------------------*/

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SuperGMS.ExceptionEx;
using SuperGMS.Log;
using SuperGMS.Protocol.RpcProtocol;
using System;

namespace SuperGMS.Rpc.Server
{
    /// <summary>
    /// 所有分布式微服务的基类，底层封装了自动负载路由和协议，完全控制了对象的自动创建和释放，
    /// 你要注意的是：在这些RpcBaseServer中不能出现长轮训或者不能跟Process方法同步返回的线程，因为Process方法返回后
    /// 底层帮你彻底将对象释放掉了，你的线程也随之回收，要长轮训，请使用TaskWorker类，要使用
    /// </summary>
    /// <typeparam name="A">A</typeparam>
    /// <typeparam name="R">R</typeparam>
    public abstract partial class RpcBaseServer<A, R>
         where A : class
         where R : class
    {
        /// <summary>
        ///
        /// </summary>
        protected StatusCode code = StatusCode.OK;

        private delegate R ProcessDelegate(A valueArgs, out StatusCode code);
        protected readonly static ILogger logger = LogFactory.CreateLogger("GrantRpcBaseServer<A, R>");

        private RpcContext ctx;

        private Args<object> _args;

        private EventId _rpcEventId;

        protected EventId RpcEventId
        {
            get { return _rpcEventId; }
        }

        protected Args<object> args
        {
            get { return _args; }
        }

        private A _valueArgs;

        public A ValueArgs
        {
            get { return _valueArgs; }
        }

        /// <summary>
        /// Gets 当前的请求上下午
        /// </summary>
        protected RpcContext Context
        {
            get { return ctx; }
        }

        /// <summary>
        /// 默认只检查登录，如果额外需要检查权限，请自己覆盖Check方法，返回CheckRights即可
        /// </summary>
        /// <param name="args">args</param>
        /// <param name="code">code</param>
        /// <returns>bool</returns>
        protected virtual bool Check(A args, out StatusCode code)
        {
            return CheckRights(args, out code);
        }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="args">args</param>
        /// <param name="loginCode">code</param>
        /// <returns>bool</returns>
        protected bool CheckLogin(A args, out StatusCode loginCode)
        {
            if (ctx == null)
            {
                loginCode = StatusCode.LoginFailed;
                return false;
            }

            var userCxt = ctx.GetUserContext();
            if (userCxt == null || userCxt.UserId < 1)
            {
                loginCode = StatusCode.LoginFailed;
                return false;
            }

            loginCode = StatusCode.OK;
            return true;
        }

        /// <summary>
        /// 登录验证,并检查权限（服务Check方法不重载时默认调用此方法)
        /// </summary>
        /// <param name="args">args</param>
        /// <param name="rightCode">code</param>
        /// <returns>bool</returns>
        protected bool CheckRights(A args, out StatusCode rightCode)
        {        
            if (ctx == null)
            {
                rightCode = StatusCode.LoginFailed;
                return false;
            }

            var userCxt = ctx.GetUserContext();
            if (userCxt == null || userCxt.UserId < 1)
            {
                rightCode = StatusCode.LoginFailed;
                return false;
            }

            if (!userCxt.HavRights(this.GetType().Name))
            {
                rightCode = StatusCode.Unauthorized;
                return false;
            }

            rightCode = StatusCode.OK;
            return true;
        }
        /// <summary>
        /// 解析参数
        /// </summary>
        /// <param name="args"></param>
        /// <param name="analyzeCode"></param>
        /// <returns></returns>
        private A AnalyzeArgs(Args<object> args, out StatusCode analyzeCode)
        {
            analyzeCode = StatusCode.OK;
            A arg = default(A);
            if (typeof(A) != typeof(Nullables))
            {
                // 服务器要求参数不能为空，但是客户端给传了空，返回参数错误
                if (args.v == null || args.v.ToString() == string.Empty)
                {
                    analyzeCode = StatusCode.ArgesError;
                    return null;
                }
                if (args.v.GetType() == typeof(A))
                {
                    return (A)args.v; // v是A的类型，就不用转了
                }
                try
                {
                    return JsonConvert.DeserializeObject<A>(args.v.ToString(), new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate });
                }
                catch (Exception ex)
                {
                    analyzeCode = StatusCode.ArgesError;
                    logger.LogError(new EventId(0, args.rid), ex, string.Format("Args analyze error, Args={0}", args.v.ToString()));
                    return null;
                }
            }
            else
            {
                // 允许为空，则直接返回空，不管客户端给传啥，都扔掉
                return arg;
            }
        }

        /// <summary>
        /// 内部调用使用此方法可以简化
        /// 本来想命名Run的但是，反射的时候会出现同名二义性，所以只能另起他名
        /// </summary>
        /// <param name="args"></param>
        /// <param name="c"></param>
        /// <param name="rpcContext"></param>
        /// <param name="objValue"></param>
        /// <returns></returns>
        public R RunInner(A args, out StatusCode c, RpcContext rpcContext, object objValue = null)
        {
           var a = rpcContext.Args.Copy();
           a.v = args;
           return Run(a, out c, objValue).v;
        }

        /// <summary>
        /// 接口运行的方法入口
        /// </summary>
        /// <param name="args">args</param>
        /// <param name="c">c</param>
        /// <param name="objValue">objValue 这个一定不是RpcContext 这个是用户想传的自己的数据，通常情况下可以传null，tk和其他参数全部在args中</param>
        /// <returns>Result</returns>
        public Result<R> Run(Args<object> args, out StatusCode c, object objValue = null)
        {
            R r = default(R);
            Result<R> rt = new Result<R>();
            code = StatusCode.OK;
            rt.rid = args.rid; // 请求id原样返回
            EventId eventId = new EventId(0, args.rid);
            if (args.m == null)
            {
                args.m = this.GetType().Name;
            }

            try
            {
                #region 解析参数
                StatusCode analyzeCode = null;
                A a = _valueArgs = AnalyzeArgs(args, out analyzeCode);
                if (!analyzeCode.IsSuccess)
                {
                    code = c = analyzeCode;
                    rt.c = c.code;
                    rt.msg = c.msg;
                    rt.MsgParam = c.MsgParam;
                    return rt;
                }
                if (string.IsNullOrEmpty(args.uri))
                {
                    args.uri = "ttid@Grant.com;u=321233;p=1000";
                }
                ctx = new RpcContext(objValue, args);
                ctx.Headers = args.Headers;
                _args = args;
                _rpcEventId = new EventId(0, args.rid);
                #endregion

                #region 校验登陆状态及权限
                StatusCode checkCode = null;
                if (!Check(a, out checkCode))
                {
                    code = c = checkCode;
                    rt.c = c.code;
                    rt.msg = c.msg;
                    rt.MsgParam = c.MsgParam;
                    return rt;
                }
                #endregion

                #region 执行业务
                r = Process(a, out code);
                #endregion
            }
            catch (BusinessException e)
            {
                code = c = new StatusCode(603, e.Message);
                rt.c = 603;
                rt.msg = e.Message;
                return rt;
            }
            catch (Exception ex)
            {
                code = c = StatusCode.ServerError;
                rt.c = c.code;
                rt.msg = c.msg;
                rt.error = $"{ex.Message},{ex.StackTrace}";
                logger.LogError(eventId, ex, "Executing GrantRpcBaseServer.Run() Error.");
                return rt;  
            }
            finally
            {
                try
                {
                    ctx?.Dispose(); // 如果参数不正确，这个ctx就是null
                }
                catch (Exception e)
                {
                    logger.LogError(eventId, e, "RpcContext.Dispose() Error.");
                }
            }

            if (r is Nullables)
            {
                r = null;
            }

            c = code;

            rt.c = c.code;
            rt.msg = c.msg;
            rt.MsgParam = c.MsgParam;
            rt.v = r;
            rt.rid = args.rid;
            return rt;
        }

        /// <summary>
        ///  需要具体业务的实现类，完成具体的业务
        /// </summary>
        /// <param name="valueArgs">业务类中定义的目标参数值_Value</param>
        /// <param name="code">Process处理过程中的状态码，为了强制子类给code赋值，这里写成out</param>
        /// <returns>R</returns>
        protected abstract R Process(A valueArgs, out StatusCode code);
    }
}