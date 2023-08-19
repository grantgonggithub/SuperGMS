/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.WebSocketEx
 文件名：SuperWebSocketProxy
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/3/24 14:35:06

 功能描述：WebSocketProxy代理接入层

----------------------------------------------------------------*/
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SuperGMS.HttpProxy;
using SuperGMS.Log;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc.Client;

using System;
using System.Collections.Generic;

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
            //mainLog.SetInfo(context);
            // var content = string.Empty;
            Args<object> a = null;
            string rst = null;
            try
            {
                a = JsonConvert.DeserializeObject<Args<object>>(content, SuperHttpProxy.jsonSerializerSettings);
                a.Headers = headers;
                //}

                if (string.IsNullOrEmpty(a.rid))
                {
                    // 提前端产生一个rid
                    a.rid = Guid.NewGuid().ToString("N");
                }

                (var rtn,var rt) = RpcClientManager.Send(a, a.m);
                rst = rtn;
                mainLog.SetInfo(a, rt);
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
