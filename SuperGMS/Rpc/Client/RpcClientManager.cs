/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantRpc.Client
 文件名：GrantRpcClientManager
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 17:19:17

 功能描述：注册Rpc客户端

----------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SuperGMS.Config;
using SuperGMS.Log;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Router;
using SuperGMS.Rpc.Server;
using SuperGMS.Tools;

namespace SuperGMS.Rpc.Client
{
    /// <summary>
    /// 注册Rpc客户端
    /// </summary>
    public class RpcClientManager
    {
        private static Dictionary<string, ClientServer> cls = new Dictionary<string, ClientServer>();
        private static ReaderWriterLock readerWriterLock = new ReaderWriterLock();
        private readonly static ILogger logger = LogFactory.CreateLogger<RpcClientManager>();

        /// <summary>
        /// 根据微服务的名字拉取配置地址
        /// </summary>
        /// <param name="appName">appName</param>
        public static void Register(string appName)
        {
            var rpcClients = ServerSetting.GetAppClient(appName, updateAppClinet);
            if (rpcClients == null || rpcClients.RpcClients == null)
            {
                return;
            }

            Parser(rpcClients.RpcClients);
        }

        private static void updateAppClinet(Configuration rpcClients)
        {
            Parser(rpcClients.RpcClients);
        }

        /// <summary>
        /// 更新本地
        /// </summary>
        /// <param name="server">rpc 远端配置</param>
        private static void Register(ClientServer server)
        {
            try
            {
                readerWriterLock.AcquireWriterLock(100);
                var ss = server;
                string key = server.ServerName.ToLower();
                if (!cls.ContainsKey(key))
                {
                    var clientServer = new ClientServer() { RouterType = ss.RouterType, ServerName = ss.ServerName };
                    cls.Add(key, clientServer);
                }

                cls[key].UpdateClient(ss.Client, true);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "SuperGMS.GrantRpc.Client.GrantRpcClientManager.Register.Error");
            }
            finally
            {
                if (readerWriterLock.IsWriterLockHeld)
                {
                    readerWriterLock.ReleaseWriterLock();
                }
            }
        }

        private static void Parser(RpcClients rpcClients)
        {
            if (rpcClients != null && rpcClients.Clients != null && rpcClients.Clients.Count > 0)
            {
                foreach (var cl in rpcClients.Clients)
                {
                    ClientServer s = new ClientServer()
                    {
                        RouterType = cl.RouterType,
                        ServerName = cl.ServerName,
                    };
                    List<ClientItem> cs = new List<ClientItem>();
                    foreach (var it in cl.Items)
                    {
                        if (!it.Enable)
                        {
                            logger.LogInformation($"服务{s.ServerName}的Ip={it.Ip},Port={it.Port}的服务被下线，路由将被忽略....");
                            continue;
                        }
                        cs.Add(new ClientItem
                        {
                            Ip = it.Ip,
                            Port = it.Port,
                            Server = s,
                            ServerType =it.ServerType,
                            Pool = it.Pool,
                            Enable = it.Enable,
                            TimeOut = it.TimeOut,
                        });
                    }

                    s.UpdateClient(cs.ToArray(), true);
                    Register(s);
                }
            }

        }

        /// <summary>
        /// 客户端Rpc调用
        /// </summary>
        /// <typeparam name="R">R</typeparam>
        /// <typeparam name="A">A</typeparam>
        /// <param name="server">server</param>
        /// <param name="m">m</param>
        /// <param name="args">args</param>
        /// <param name="code">code</param>
        /// <returns>RR</returns>
        public static R Send<A, R>(string server, string m, A args, RpcContext context, out StatusCode code)
        {
            Args<A> a = new Args<A>()
            {
                ct = ClientType.InnerRpc.ToString(), // 只有内部的rpc请求才能如此标记
                cv = string.Empty,
                m = m,
                v = args,
                rid = Guid.NewGuid().ToString("N"),
                Headers = new Dictionary<string, HeaderValue>(),
            };
            EventId eventId = new EventId(0, a.rid);
            try
            {
                #region 构造路由

                // 添加自己的头信息
                string sk = $"{ServerSetting.AppName}(idx:{ServerSetting.Pool})_{ServiceEnvironment.ComputerName}_{ServiceEnvironment.ComputerAddress}";
                string key = $"{ServerSetting.AppName}_{HeaderValue.INNERIP}"; // 强制在一个请求链上一个微服务只能被调用一次，防止出现来回循环调用的情况出现
                a.Headers.TryAdd(key, new HeaderValue(sk, HeaderValue.INNERIP, ServiceEnvironment.ComputerAddress));

                // 原样透传rpcContext
                if (context != null)
                {
                    a.ct = string.IsNullOrEmpty(context.Args.ct) ? a.ct : context.Args.ct;
                    a.cv = string.IsNullOrEmpty(context.Args.cv) ? a.cv : context.Args.cv;
                    a.rid = string.IsNullOrEmpty(context.Args.rid) ? a.rid : context.Args.rid;
                    a.lg = string.IsNullOrEmpty(context.Args.lg) ? a.lg : context.Args.lg;
                    a.tk = string.IsNullOrEmpty(context.Args.tk) ? a.tk : context.Args.tk;
                    a.uri = string.IsNullOrEmpty(context.Args.uri) ? a.uri : context.Args.uri;
                    if (context.Headers != null && context.Headers.Any())
                    {
                        foreach (var item in context.Headers)
                        {
                            a.Headers.TryAdd(item.Key, item.Value);
                        }
                    }
                }

                int error = 0;
                gotoHere:
                ClientServer cServer = null;
                ClientItem cItem = null;
                Result<R> rr = new Result<R>();
                try
                {
                    readerWriterLock.AcquireReaderLock(80);
                    string s = server.ToLower();
                    if (cls.ContainsKey(s))
                    {
                        cServer = cls[s];
                        var clients = cServer.Client;
                        int idx = 0;
                        switch (cServer.RouterType)
                        {
                            case RouterType.Hash:
                                idx = RouterManager.GetPool(a.uri);
                                break;
                            case RouterType.Polling:
                                idx = RouterManager.GetPolling(0, clients.Length);
                                break;
                            default:
                            case RouterType.Random:
                                idx = RouterManager.GetRandom(0, clients.Length);
                                break;
                        }

                        cItem = clients[idx];
                    }
                    else
                    {
                        StatusCode cc = new StatusCode(402, $"server ：{server} not found rid = {a.rid}");
                        rr.c = cc.code;
                        rr.msg = cc.msg;
                        rr.v = default(R);
                        logger.LogError(eventId, new LogInfo()
                        {
                            ServiceName = server,
                            ApiName = a.m,
                            CreatedBy = "FrameWork",
                            TransactionId = a.rid,
                            UseChain = JsonConvert.SerializeObject(a.Headers),
                            CodeMsg = rr.msg,
                            Code = rr.c,
                            Desc = $"server ：{server} not found rid = {a.rid}",
                        }.ToString());
                    }
                }
                catch (Exception e)
                {
                    string msg = $"获取路由信息异常{server}{e.Message} rid={a.rid}";
                    StatusCode cc = new StatusCode(502, msg);
                    rr.c = cc.code;
                    rr.msg = cc.msg;
                    rr.v = default(R);
                    logger.LogCritical(eventId, new LogInfo()
                    {
                        ServiceName = server,
                        ApiName = a.m,
                        CreatedBy = "FrameWork",
                        TransactionId = a.rid,
                        UseChain = JsonConvert.SerializeObject(a.Headers),
                        CodeMsg = rr.msg,
                        Code = rr.c,
                        Desc = $"获取路由信息异常{server}{e.Message} rid={a.rid}",
                    }.ToString());

                    // GrantLogTextWriter.Write(new Exception(msg, e));
                }
                finally
                {
                    if (readerWriterLock.IsReaderLockHeld)
                    {
                        readerWriterLock.ReleaseReaderLock();
                    }
                }

                #endregion 构造路由

                #region 发送请求
                var jSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                jSetting.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                jSetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                var sendValue = JsonConvert.SerializeObject(a,jSetting);
                if (cItem == null)
                {
                    code = new StatusCode(403, $"{server}的路由信息无法找到 rid={a.rid}");
                    rr.v = default(R);
                }
                else
                {
                    logger.LogInformation(eventId, $"Client发送请求\r\n\tserver={cServer.ServerName},ip={cItem.Ip},port={cItem.Port}\r\n\targs={sendValue}");
                    bool isOk = false;
                    using (ISuperGMSRpcClient rpcClient = ClientConnectionManager.GetClient(cItem))
                    {
                        string result = null;
                        if (rpcClient.Send(sendValue,m, out result))
                        {
                            rr = JsonConvert.DeserializeObject<Result<R>>(result);
                            code = new StatusCode(rr.c, rr.msg);
                            isOk = true;
                        }
                    }

                    if (!isOk)
                    {
                        error += 1;
                        System.Threading.Thread.Sleep(100);
                        // 重试3次
                        if (error < 3)
                        {
                            goto gotoHere; // 失败了就重新去找一个连接重试，如果是单台短时间内重试就没有意义了
                        }

                        rr = new Result<R> { v = default(R) };
                        code = new StatusCode(501, "client_error无法访问远程服务器 rid=" + a.rid);
                    }
                    else
                    {
                        code = new StatusCode(rr.c, rr.msg);
                    }
                }
                logger.LogInformation(eventId, $"Client返回结果\r\n\tServiceName:{server},ApiName:{a.m},Code:{code.code},CodeMsg:{code.msg},\r\n\tUseChain{JsonConvert.SerializeObject(a.Headers)}\r\n\tResult:{JsonConvert.SerializeObject(rr)}");
                return rr.v;

                #endregion 发送请求
            }
            catch (Exception e)
            {
                code = new StatusCode(504, $"未知异常:{e.Message}");
                logger.LogError(eventId, e, $"未知异常");
                return default(R);
            }
        }

        /// <summary>
        /// 代理层使用
        /// </summary>
        /// <param name="args">参数</param>
        /// <param name="url">前端要求将M值体现在路径上，接入层自己来做个转换处理</param>
        /// <returns>string,参数</returns>
        internal static (string c, Result<object> r) Send(Args<object> args, string url)
        {
            var serverName = "";
            ClientServer cServer = null;
            Result<object> rr = new Result<object>();
            EventId eventId = new EventId(0, args.rid);
            try
            {
                // 这里需要重构，统一做成一个Fliter
                ClientType ct = ClientTypeParser.Parser(args.ct);
                switch (ct)
                {
                    case ClientType.InnerRpc: // 需要拦截
                        throw new Exception("你想干什么？");
                    case ClientType.QtApp:
                    case ClientType.QtWeb: // 需要要做来源ip和reffer网站，都来着grant的域名
                    case ClientType.Unkunwn: // 未知的和第三方都必须验证appkey
                    case ClientType.ThirdPart:
                        break;
                }

                string[] motheds = url.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (motheds == null || motheds.Length < 2)
                {
                    rr.msg = "HttpProxy.Error:Request args Error,Server name Could not found rid=" + args.rid;
                    rr.c = 503;
                    rr.v = default(object);
                }
                else
                {
                    #region 重构，获取路由配置

                    string server = motheds[motheds.Length - 2];
                    serverName = server;
                    args.m = motheds[motheds.Length - 1];
                    int error = 0;
                    gotoHere:
                    readerWriterLock.AcquireReaderLock(80);
                    ClientItem cItem = null;
                    try
                    {
                        string s = server.ToLower();
                        if (cls.ContainsKey(s))
                        {
                            cServer = cls[s];
                            var clients = cServer.Client;
                            int idx = 0;
                            switch (cServer.RouterType)
                            {
                                case RouterType.Hash:
                                    idx = RouterManager.GetPool(args.uri);
                                    break;
                                case RouterType.Polling:
                                    idx = RouterManager.GetPolling(0, clients.Length);
                                    break;
                                default:
                                case RouterType.Random:
                                    idx = RouterManager.GetRandom(0, clients.Length);
                                    break;
                            }
                            cItem = clients[idx];
                        }
                        else
                        {
                            StatusCode cc = new StatusCode(402, $"HttpProxy.Error:server :{server} not found rid={args.rid}");
                            rr.c = cc.code;
                            rr.msg = cc.msg;
                            rr.v = default(object);
                            logger.LogError(eventId, new LogInfo()
                            {
                                ServiceName = serverName,
                                ApiName = args.m,
                                CreatedBy = "httpProxy",
                                TransactionId = args.rid,
                                UseChain = JsonConvert.SerializeObject(args.Headers),
                                CodeMsg = rr.msg,
                                Code = rr.c,
                                Desc = $"HttpProxy.Error:server :{server} not found rid={args.rid}",
                            }.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = $"获取路由信息异常{server}{e.Message} rid = {args.rid}";
                        StatusCode cc = new StatusCode(502, msg);
                        rr.c = cc.code;
                        rr.msg = cc.msg;
                        rr.v = default(object);
                        logger.LogError(eventId, e, $"获取Server:{server}路由信息异常");
                    }
                    finally
                    {
                        if (readerWriterLock.IsReaderLockHeld)
                        {
                            readerWriterLock.ReleaseReaderLock();
                        }
                    }

                    #endregion 重构，获取路由配置

                    #region 发送请求

                    if (cItem == null)
                    {
                        string msg = $"HttpProxy.Error:无法获取路由信息:{server} rid = {args.rid}";
                        StatusCode cc = new StatusCode(503, msg);
                        rr.c = cc.code;
                        rr.msg = cc.msg;
                        rr.v = default(object);

                        logger.LogError(eventId, new LogInfo
                        {
                            ServiceName = server,
                            ApiName = args.m,
                            CreatedBy = "httpProxy",
                            TransactionId = args.rid,
                            Desc = msg,
                            Code = rr.c,
                            CodeMsg = rr.msg,
                            UseChain = JsonConvert.SerializeObject(args.Headers),
                        }.ToString());
                    }
                    else
                    {
                        cItem.Url = args.m;
                        var jSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                        jSetting.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                        jSetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                        var msg = JsonConvert.SerializeObject(args,jSetting);
                        logger.LogTrace(eventId, new LogInfo
                        {
                            ServiceName = server,
                            ApiName = args.m,
                            CreatedBy = "httpProxy",
                            TransactionId = args.rid,
                            Desc = string.Format("开始访问服务器:server={0},ip={1},port={2},args={3},rid={4}",
                                cServer.ServerName, cItem.Ip, cItem.Port, msg, args.rid),
                            Code = StatusCode.OK.code,
                            CodeMsg = StatusCode.OK.msg,
                            UseChain = JsonConvert.SerializeObject(args.Headers),
                        }.ToString());
                        using (ISuperGMSRpcClient rpcClient = ClientConnectionManager.GetClient(cItem))
                        {
                            string result = null;
                            if (rpcClient.Send(msg,args.m, out result))
                            {
                                var resultObj = JsonConvert.DeserializeObject<Result<object>>(result);
                                logger.LogTrace(eventId, new LogInfo
                                {
                                    ServiceName = server,
                                    ApiName = args.m,
                                    CreatedBy = "httpProxy",
                                    TransactionId = args.rid,
                                    Desc = string.Format(
                                        "服务器返回:server={0},ip={1},port={2},result={3},rid={4}",
                                        cServer.ServerName, cItem.Ip, cItem.Port, result, args.rid),
                                    Code = resultObj.c,
                                    CodeMsg = resultObj.msg,
                                    UseChain = JsonConvert.SerializeObject(args.Headers),
                                }.ToString());
                                return (result, resultObj); // 直接返回结果
                            }
                        }

                        // 走到这里就说明失败了
                        error += 1;
                        System.Threading.Thread.Sleep(100);
                        // 重试3次
                        if (error < 3)
                        {
                            goto gotoHere; // 失败了就重新去找一个连接重试
                        }

                        // rr = new Result<R> { v = default(R) };
                        // 要知道请求的rid这里只能反解出来
                        StatusCode ccc = new StatusCode(502, "HttpProxy.Error:无法访问远程服务器 rid=" + args.rid);
                        rr = new Result<object>()
                        {
                            c = ccc.code,
                            msg = ccc.msg,
                            rid = args.rid,
                            uri = args.uri,
                            v = null,
                        };
                    }
                    #endregion 发送请求
                }
            }
            catch (Exception ex)
            {
                StatusCode cc = new StatusCode(501, "HttpProxy.Error:" + ex.Message);
                rr.c = cc.code;
                rr.msg = cc.msg;
                rr.v = default(object);
                logger.LogError(eventId, ex, new LogInfo()
                {
                    ServiceName = serverName,
                    ApiName = args.m,
                    CreatedBy = "httpProxy",
                    TransactionId = args.rid,
                    UseChain = JsonConvert.SerializeObject(args.Headers),
                    CodeMsg = rr.msg,
                    Code = rr.c,
                    Desc = "GrantRpcClientManager.Send<R, A>.error",
                }.ToString());
            }
            var js = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            js.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
            js.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            var rst = JsonConvert.SerializeObject(rr, js);
            logger.LogInformation(eventId, new LogInfo()
            {
                ServiceName = serverName,
                ApiName = args.m,
                CreatedBy = "httpProxy",
                TransactionId = args.rid,
                UseChain = JsonConvert.SerializeObject(args.Headers),
                CodeMsg = rr.msg,
                Code = rr.c,
                Desc = string.Format("返回结果是:{0}", rst),
            }.ToString());
            return (rst, rr);
        }

        /// <summary>
        /// 释放全部客户端
        /// </summary>
        public static void Dispose()
        {
            try
            {
                cls.Clear();
                ClientConnectionManager.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "GrantRpcClientManager.Dispose.Error");
            }
        }
    }
}