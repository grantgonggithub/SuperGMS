using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SuperGMS.Config;
using SuperGMS.HttpProxy;
using SuperGMS.Rpc;
using Microsoft.Extensions.Logging;

namespace Grant.HttpProxy.Middleware
{
    public class ProxyMiddleware
    {
        private readonly RequestDelegate m_Next;
        private readonly static ILogger logger=SuperGMS.Log.LogFactory.CreateLogger<ProxyMiddleware>();

        public ProxyMiddleware(RequestDelegate next)
        {
            this.m_Next = next;
        }

        public Task Invoke(HttpContext context)
        {
            try
            {
                if (context.Request.Method?.ToUpper() == "GET")
                {
                    var path = context.Request.Path.HasValue ? context.Request.Path.ToString().ToLower() : string.Empty;
                    if (string.IsNullOrEmpty(path) || path.Equals("/") || path.Equals("/api") || path.Equals("/api/"))
                    {
                        return this.SendConstRespond(context);
                    }
                    //增加一个代理网关接口，返回微服务列表
                    else if (path.EndsWith("/getallservices"))
                    {
                        return this.SendServiceListRespond(context);
                    }

                    return this.SendStringRespond(context);
                }

                //if (context.Request.ContentType == null || context.Request.ContentType.IndexOf("application/json") < 0)
                //{
                //    context.Response.StatusCode = 403;
                //    context.Response.ContentType += "application/json;charset=utf-8;";
                //    return context.Response.WriteAsync("Please set ContentType=application/json");
                //}

                return this.SendStringRespond(context);
            }
            catch (Exception ex)
            { 
                logger.LogError(ex, $"ProxyMiddleware Invoke error, request path: {context.Request.Path.HasValue}, method: {context.Request.Method}");
                return Task.CompletedTask;
            }
        }

        private Task SendStringRespond(HttpContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType += "application/json;charset=utf-8;";

            Task task = new Task(() =>
            {
                try
                {
                    if (ServerSetting.Config.ServerConfig.RpcService.ServerType == ServerType.HttpWebApi)
                    {
                        string constResp = SuperGMS.HttpProxy.SuperHttpProxy.SendHttp(context);
                        using (var strStream = new StreamWriter(context.Response.Body))
                        {
                            strStream.Write(constResp);
                            strStream.Flush();
                        }
                    }
                    else
                    {
                        SuperGMS.HttpProxy.SuperHttpProxy.Send(context);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"ProxyMiddleware Invoke error, request path: {context.Request.Path.HasValue}, method: {context.Request.Method}");
                }
            });
            task.Start();

            return task;
        }

        private Task SendConstRespond(HttpContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType += "application/json;charset=utf-8;";

            Task task = new Task(() =>
            {
                try
                {
                    var constResp = new
                    {
                        rid = string.Empty,
                        c = 200,
                        msg = "Access api gateway success!"
                    };
                    using (var strStream = new StreamWriter(context.Response.Body))
                    {
                        strStream.Write(JsonConvert.SerializeObject(constResp));
                        strStream.Flush();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"ProxyMiddleware Invoke error, request path: {context.Request.Path.HasValue}, method: {context.Request.Method}");
                }
            });
            task.Start();
            return task;
        }
        private Task SendServiceListRespond(HttpContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType += "application/json;charset=utf-8;";

            Task task = new Task(() =>
            {
                try
                {
                    var svrs = ServerSetting.Config?.HttpProxy?.Items?.Select(x => x.Name)?.ToList() ?? new List<string>();
                    var constResp = new
                    {
                        rid = Guid.NewGuid().ToString("N"),
                        c = 200,
                        v = svrs,
                    };
                    using (var strStream = new StreamWriter(context.Response.Body))
                    {
                        strStream.Write(JsonConvert.SerializeObject(constResp));
                        strStream.Flush();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"ProxyMiddleware Invoke error, request path: {context.Request.Path.HasValue}, method: {context.Request.Method}");
                }
            });
            task.Start();
            return task;
        }
    }
}