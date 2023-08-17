/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.WebSocketEx
 文件名：SuperWebSocketProxy
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/3/24 14:35:06

 功能描述：WebSocketProxy代理接入层

----------------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SuperGMS.HttpProxy;
using SuperGMS.Log;
using SuperGMS.Protocol.ApiProtocol;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc.Client;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Dapper.SqlMapper;

namespace SuperGMS.WebSocketEx
{
    public class SuperWebSocketProxy
    {
        /// <summary>
        /// 面向前端的代理层websocket的配置
        /// </summary>
        public const string WebSocketProxy = "WebSocketProxy";
        /// <summary>
        /// 面向后端的rpc的配置
        /// </summary>
        public const string WebSocketService = "WebSocketService";

        private readonly static ILogger logger = LogFactory.CreateLogger<SuperHttpProxy>();

        public static void Register()
        {
            SuperHttpProxy.HttpProxyName = WebSocketService;
            SuperHttpProxy.Register();
        }

        /// <summary>
        /// 前端代理处理程序，收和发
        /// </summary>
        /// <param name="content"></param>
        /// <param name="path"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static string Send(string content, Dictionary<string, HeaderValue> headers)
        {
            // 构造一个统计日志
            var mainLog = new LogStat() { BusinessType = "websocket.Proxy" };
            // mainLog.SetInfo(context);
            // var content = string.Empty;
            Args<object> a = null;
            string rst = null;
            try
            {
                //content = GetRequestValue(context.Request);
                //var isUdf = IsController(context);
                //if (isUdf)
                //{
                //    ApiArgs apiArgs = new ApiArgs { Headers = new Dictionary<string, string>(), Params = new Dictionary<string, string>() };
                //    foreach (var h in context.Request.Headers)
                //        apiArgs.Headers.Add(h.Key, h.Value);
                //    foreach (var p in context.Request.Query)
                //        apiArgs.Params[p.Key] = p.Value;
                //    apiArgs.Body = content;
                //    a = new Args<object> { v = apiArgs, ct = ClientType.ThirdPart.ToString() };
                //}
                //else
                //{
                a = JsonConvert.DeserializeObject<Args<object>>(content, SuperHttpProxy.jsonSerializerSettings);
                //}

                if (string.IsNullOrEmpty(a.rid))
                {
                    // 提前端产生一个rid
                    a.rid = Guid.NewGuid().ToString("N");
                }

                (var rtn, _) = RpcClientManager.Send(a, a.m);
                rst = rtn;
                //string body = rtn.c;
                //if (isUdf)
                //{
                //    if (rtn.r.v != null && !string.IsNullOrEmpty(rtn.r.v.ToString()))
                //    {
                //        var apiRst = JsonConvert.DeserializeObject<ApiResult>(rtn.r.v.ToString());
                //        if (apiRst != null)
                //        {
                //            body = apiRst.Body;
                //            context.Response.StatusCode = (int)apiRst.Code;
                //            if (context.Response.StatusCode < 1) context.Response.StatusCode = 200;
                //            if (!string.IsNullOrEmpty(apiRst.ContentType)) context.Response.ContentType = apiRst.ContentType;
                //        }
                //    }
                //}
                //using (var strStream = new StreamWriter(context.Response.Body))
                //{
                //    strStream.Write(body);
                //    strStream.Flush();
                //    strStream.Close();
                //}

                // mainLog.SetInfo(a, rtn.r);
                logger.LogInformation(new EventId(0, a.rid), mainLog.ToString());
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
                if (string.IsNullOrEmpty(rr.rid))
                {
                    rr.rid = Guid.NewGuid().ToString("N");
                }



                rst = JsonConvert.SerializeObject(rr, SuperHttpProxy.jsonSerializerSettings);
                //using (var strStream = new StreamWriter(context.Response.Body))
                //{
                //    strStream.Write(rst);
                //    strStream.Flush();
                //    strStream.Close();
                //}
                logger.LogError(new EventId(0, rr.rid), e, $"SuperWebSocketProxy.Send.Error");
                mainLog.SetInfo(null, rr, e);
                logger.LogInformation(new EventId(0, rr.rid), mainLog.ToString());
            }
            return rst;
        }

    }
}
