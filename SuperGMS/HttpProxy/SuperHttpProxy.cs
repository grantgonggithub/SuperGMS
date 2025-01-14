/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantHAProxy
 文件名：  GrantHttpProxy
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/9/27 20:21:20

 功能描述：

----------------------------------------------------------------*/

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

using SuperGMS.Config;
using SuperGMS.Log;
using SuperGMS.Protocol.ApiProtocol;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc;
using SuperGMS.Rpc.Client;
using SuperGMS.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;

namespace SuperGMS.HttpProxy
{
    /// <summary>
    /// GrantHttpProxy
    /// </summary>
    public class SuperHttpProxy
    {
        public static string HttpProxyName = HttpProxy; // 这个是做为webApi时，可能就会被改变
        public const string HttpProxy = "HttpProxy"; // 这个指的httpProxy这个服务，是个固定的名称，也指配置
        private readonly static ILogger logger = LogFactory.CreateLogger<SuperHttpProxy>();
        public readonly static JsonSerializerSettings jsonSerializerSettings = GetJsonSerializerSettings();
        public static void Register()
        {
            ServerSetting.Initlize(SuperGMS.HttpProxy.SuperHttpProxy.HttpProxyName, 0);
            string[] server = GetAllServer();
            Reg(server);
            ServerSetting.RegisterRouter(SuperGMS.HttpProxy.SuperHttpProxy.HttpProxyName,
                ServerSetting.Config.ServerConfig.RpcService.Ip,
                ServerSetting.Config.ServerConfig.RpcService.Port,
                ServerSetting.Config.ServerConfig.RpcService.Enable,
                ServerSetting.Config.ServerConfig.RpcService.TimeOut);
        }

        private static void Reg(string[] servers)
        {
            foreach (string app in servers)
            {
                RpcClientManager.Register(app);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings GetJsonSerializerSettings()
        {
            JsonSerializerSettings jSetting = new JsonSerializerSettings
            {
                FloatParseHandling = FloatParseHandling.Decimal,
                NullValueHandling = NullValueHandling.Ignore,
            };
            jSetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            return jSetting;
        }

        /// <summary>
        /// 前端代理处理程序，收和发
        /// </summary>
        /// <param name="context">http上下文</param>
        /// <returns>结果，不会抛出异常</returns>
        public static Result<object> Send(HttpContext context)
        {
            // 构造一个统计日志
            var mainLog = new LogStat() { BusinessType = "http.Proxy" };
            mainLog.SetInfo(context);
            var content = string.Empty;
            Args<object> a = null;
            try
            {
                a = PaserProto(context, out var isUdf,out var serviceName);

                var rtn = RpcClientManager.Send(a, context.Request.Path);
                string body = rtn.c;
                if (isUdf)
                {
                    if (rtn.r.v != null&&!string.IsNullOrEmpty(rtn.r.v.ToString()))
                    {
                        var apiRst =JsonConvert.DeserializeObject<ApiResult>(rtn.r.v.ToString());
                        if (apiRst != null)
                        {
                            body = apiRst.Body;
                            context.Response.StatusCode =(int)apiRst.Code;
                            if (context.Response.StatusCode < 1) context.Response.StatusCode = 200;
                            if (!string.IsNullOrEmpty(apiRst.ContentType)) context.Response.ContentType = apiRst.ContentType;
                        }
                    }
                }
                using (var strStream = new StreamWriter(context.Response.Body))
                {
                    strStream.Write(body);
                    strStream.Flush();
                    strStream.Close();
                }

                mainLog.SetInfo(a, rtn.r);
                logger.LogInformation(new EventId(0, a.rid), mainLog.ToString());
                return rtn.r;
            }
            catch (Exception e)
            {
                Result<object> rr = new Result<object>()
                {
                    c = 500,
                    msg = e.Message,
                };
                // 处理返回值中的rid,解析请求内容获取rid
                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        a = JsonConvert.DeserializeObject<Args<object>>(content);
                        if (!string.IsNullOrEmpty(a.rid))
                        {
                            rr.rid = a.rid;
                        }
                    }
                    catch
                    {
                        //反序列化请求内容异常
                    }
                }
                //如果未获取到原始请求rid,则重新生成
                if(string.IsNullOrEmpty(rr.rid))
                {
                    rr.rid = Guid.NewGuid().ToString("N");
                }

                var rst = JsonConvert.SerializeObject(rr, jsonSerializerSettings);
                using (var strStream = new StreamWriter(context.Response.Body))
                {
                    strStream.Write(rst);
                    strStream.Flush();
                    strStream.Close();
                }
                logger.LogError(new EventId(0, rr.rid), e, $"GrantHttpProxy.Send.Error");
                mainLog.SetInfo(null, rr, e);
                logger.LogInformation(new EventId(0, rr.rid), mainLog.ToString());
                return rr;
            }
        }

        public static Args<object> PaserProto(HttpContext context,out bool isUdf,out string serviceName)
        {
            Args<object> a = null;
            var content = GetRequestValue(context.Request);
            string rid = Guid.NewGuid().ToString("N");
            logger.LogInformation($"收到请求：rid:{rid},{content}");
            isUdf = IsController(context);
            if (isUdf)
            {
                ApiArgs apiArgs = new ApiArgs { Headers = new Dictionary<string, string>(), Params = new Dictionary<string, string>() };
                foreach (var h in context.Request.Headers)
                    apiArgs.Headers.Add(h.Key, h.Value);
                foreach (var p in context.Request.Query)
                    apiArgs.Params[p.Key] = p.Value;
                apiArgs.Body = content;
                a = new Args<object> { v = apiArgs, ct = ClientType.ThirdPart.ToString() };
            }
            else
            {
                a = JsonConvert.DeserializeObject<Args<object>>(content, jsonSerializerSettings);
                a = a == null ? new Args<object>() : a;
            }
            var url = context.Request.Path.HasValue?context.Request.Path.Value:null;
            string[] motheds = url?.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (motheds == null || motheds.Length < 2)
            {
                throw new Exception("Url not content Path");
            }
            serviceName = motheds[motheds.Length - 2];
            a.m = motheds[motheds.Length - 1];

            a.Headers = GetRequestIp(context);
            if (string.IsNullOrEmpty(a.rid))
            {
                // 提前端产生一个rid
                a.rid = rid;
            }
            return a;
        }

        public static Result<object> PaserResult(HttpContext context,Result<object> result,string origon,bool isUdf)
        {
            string body = origon;
            if (isUdf)
            {
                if (result.v != null && !string.IsNullOrEmpty(result.v.ToString()))
                {
                    var apiRst = JsonConvert.DeserializeObject<ApiResult>(result.v.ToString());
                    if (apiRst != null)
                    {
                        body = apiRst.Body;
                        context.Response.StatusCode = (int)apiRst.Code;
                        if (context.Response.StatusCode < 1) context.Response.StatusCode = 200;
                        if (!string.IsNullOrEmpty(apiRst.ContentType)) context.Response.ContentType = apiRst.ContentType;
                    }
                }
            }
            using (var strStream = new StreamWriter(context.Response.Body))
            {
                strStream.Write(body);
                strStream.Flush();
                strStream.Close();
            }
            return result;
        }

        /// <summary>
        /// 是否以Controller结尾，的自定义Api
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static bool IsController(HttpContext context)
        {
            var path = context.Request.Path.HasValue ? context.Request.Path.ToString().ToLower() : string.Empty;
            return path.EndsWith("controller");
        }

        /// <summary>
        /// 直接提供webApi本地直接处理请求，不转发到后端rpcServer
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string SendHttp(HttpContext context)
        {
            var content = GetRequestValue(context.Request);
            Args<object> a = JsonConvert.DeserializeObject<Args<object>>(content);
            a.Headers = GetRequestIp(context);
            if (string.IsNullOrEmpty(a.rid))
            {
                // 提前端产生一个rid
                a.rid = Guid.NewGuid().ToString("N");
            }
            string[] motheds = context.Request.Path.ToString().Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (motheds == null || motheds.Length < 2)
            {
                Result<object> rr = new Result<object>();
                rr.msg = "Uri Error rid=" + a.rid;
                rr.c = 503;
                rr.v = default(object);
                return JsonConvert.SerializeObject(rr, jsonSerializerSettings);
            }
            else
            {
                a.m = motheds[motheds.Length - 1];
                return ServerProxy.HttpSend(JsonConvert.SerializeObject(a, jsonSerializerSettings), a.Headers);
            }
        }

        private static string[] GetAllServer()
        {
            var httpProxy = ServerSetting.GetHttpProxy(HttpProxyName, UpdateHttpProxy);
            if (httpProxy == null || httpProxy.HttpProxy == null)
            {
                throw new System.Exception("你不配置后端服务列表，HttpProxy犯傻了~~~");
            }

            if (httpProxy.HttpProxy.Items == null)
            {
                throw new System.Exception("你不配置后端服务列表Item，HttpProxy犯傻了~~~");
            }

            return httpProxy.HttpProxy.Items.Select(x => x.Name).ToArray();
        }

        /// <summary>
        /// zk 反向更新
        /// </summary>
        /// <param name="proxy">代理层配置</param>
        private static void UpdateHttpProxy(Config.Configuration proxy)
        {
            var items = proxy.HttpProxy.Items.Select(x => x.Name)?.ToArray();
            if (items == null || items.Length < 1)
            {
                logger.LogWarning("代理层配置为空，将不进行更新 GrantHttpProxy.UpdateHttpProxy.items == null || items.Length < 1");
            }
            Reg(items);
        }

        /// <summary>
        /// 获取http请求中的body内容
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetRequestValue(HttpRequest request)
        {
            // 仅支持json解析
            string args = request.Query["args"]; // get,post不区分了
            if (string.IsNullOrEmpty(args) && request.Method.ToUpper() == "GET")
            {
                args = "#{#}#";
            }

            // 如果没有接到，就用流接
            if (string.IsNullOrEmpty(args) && request.ContentLength > 0)
            {
                using (Stream s = request.Body)
                {
                    using (StreamReader reader = new StreamReader(s, System.Text.Encoding.UTF8))
                    {
                        args = reader.ReadToEnd();
                    }
                }
            }

            // 特别处理取得token，解决客户端无法取得cookie问题
            if (request.Path.HasValue && (request.Path.Value.ToLower().Contains("gettokenbyssid") || request.Path.Value.ToLower().Contains("gettokenandrightbyssid")))
            {
                var ssid = string.Empty;
                var lang = string.Empty;
                request.Cookies.TryGetValue("SSID", out ssid);
                request.Cookies.TryGetValue("Lang", out lang);
                args = args?.Replace("\"ssid\":\"\"", $"\"ssid\":\"{ssid}\"");
                args = args?.Replace("\"ssid\":null", $"\"ssid\":\"{ssid}\"");
                args = args?.Replace("\"lang\":\"\"", $"\"lang\":\"{lang}\"");
                args = args?.Replace("\"lang\":null", $"\"lang\":\"{lang}\"");
            }
            return args.Replace("#{#}#","");
        }

        public static Dictionary<string, HeaderValue> GetRequestIp(HttpContext ctx)
        {
            string ipValue = string.Empty;
            try
            {
                ipValue = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(ipValue))
                {
                    ipValue = ctx.Request.Headers["X-Original-For"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(ipValue))
                {
                    ipValue = ctx.Connection.RemoteIpAddress?.ToString();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"GrantHttpProxy.GetRequestIp.Error");
            }

            if (string.IsNullOrEmpty(ipValue))
            {
                ipValue = "unknown";
            }
            var userAgent = ctx.Request.Headers["User-Agent"];
            var host = ctx.Request.Host.Value;
            Dictionary<string, HeaderValue> dic = new Dictionary<string, HeaderValue>();
            var serverName = $"{HttpProxyName}_{ServiceEnvironment.ComputerAddress}_{ServiceEnvironment.ComputerName}";
            dic.Add(HeaderValue.REMOTEIP, // 一个请求只会经过一次接入层
               new HeaderValue(serverName,HeaderValue.REMOTEIP,ipValue));
            dic.Add(HeaderValue.USERAGENT, new HeaderValue(serverName, HeaderValue.USERAGENT, userAgent));
            dic.Add(HeaderValue.HOST, new HeaderValue(serverName, HeaderValue.HOST, host));
            return dic;
        }
    }
}