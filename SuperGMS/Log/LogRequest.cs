using Microsoft.AspNetCore.Http;
using SuperGMS.Config;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Tools;
using System;
using System.Linq;

namespace SuperGMS.Log
{
    public class LogRequest : LogBase
    {
        public LogRequest()
        {
            CreatedDate = DateTime.Now;
            var rpc = ServerSetting.GetRpcServer();
            this.ComputerIp = $"{rpc.Ip}:{rpc.Port}";
            this.CreatedDate = DateTime.Now;
            this.ServiceName = ServerSetting.AppName;
        }
        /// <summary>
        /// token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 登录名
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// ttid
        /// </summary>
        public string Ttid { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// 客户类型
        /// </summary>
        public string ClientType { get; set; }

        /// <summary>
        /// 客户版本
        /// </summary>
        public string ClientVersion { get; set; }

        /// <summary>
        /// 客户端ip
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        /// 客户端信息
        /// </summary>
        public string ClientInfo { get; set; }

        /// <summary>
        /// 整体执行时间 ms
        /// </summary>
        public int ExecuteTime { get; set; }

        /// <summary>
        /// 代理类处理的服务IP和端口
        /// </summary>
        public string ComputerIp { get; set; }

        /// <summary>
        /// 源地址
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string CodeMsg { get; set; }
        /// <summary>
        /// 传入参数
        /// </summary>
        public string Parameters { get; set; }
        /// <summary>
        /// 返回结果
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 设置http 请求信息
        /// </summary>
        /// <param name="context">Http上下文</param>
        public void SetInfo(HttpContext context)
        {
            try
            {
                this.SourceUrl = $"{context.Request.Scheme}://{context.Request.Host.ToString().TrimEnd('/')}/{context.Request.Path.ToString().TrimStart('/')}";
                if (context.Request.QueryString.HasValue)
                {
                    this.SourceUrl += $"?{context.Request.QueryString.Value}";
                }

                if (string.IsNullOrEmpty(this.ServiceName))
                {
                    var p = context.Request.Path.ToString();
                    var ms = p.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (ms != null && ms.Length > 2)
                    {
                        this.ServiceName = ms[ms.Length - 2];
                        this.ApiName = ms[ms.Length - 1];
                    }
                }

                this.ClientInfo = context.Request.Headers["User-Agent"].FirstOrDefault();
                this.ClientIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(this.ClientIp))
                {
                    this.ClientIp = context.Request.Headers["X-Original-For"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(this.ClientIp))
                {
                    this.ClientIp = context.Connection.RemoteIpAddress?.ToString();
                }

            }
            catch (Exception ex)
            {
                //暂不处理SetInfo赋值异常
            }
        }

        /// <inheritdoc cref="LogBase"/>
        public override void SetInfo<T>(Args<object> args, Result<T> result = null, Exception ex = null)
        {
            try
            {
                if (result == null && args == null)
                {
                    return;
                }

                this.ExecuteTime = (int)(DateTime.Now - this.CreatedDate).TotalMilliseconds;
                this.TransactionId = result?.rid ?? args?.rid;
                this.Code = result?.c ?? 0;
                this.CodeMsg = result?.msg;

                this.ClientType = args?.ct;
                this.ClientVersion = args?.cv;
                this.Lang = args?.lg;
                this.Token = args?.tk;

                if (string.IsNullOrEmpty(this.ServiceName))
                {
                    this.ServiceName = ServerSetting.AppName;
                }

                if (string.IsNullOrEmpty(this.ApiName))
                {
                    this.ApiName = args?.m;
                }
            }
            catch (Exception e)
            {
                //暂不处理SetInfo赋值异常
            }
        }

        /// <summary>
        /// 日志格式化字符串
        /// </summary>
        /// <returns>字符串</returns>
        public override string ToString()
        {
            return $"Execute Time:{(int)(DateTime.Now - this.CreatedDate).TotalMilliseconds}ms, Url:/{this.ServiceName}/{this.ApiName}, Code:{this.Code}, CodeMsg:{this.CodeMsg}\r\n\t" +
                $"User:{this.UserId}, Lang:{this.Lang}, Token:{this.Token}, ServerIp:{this.ComputerIp}, SrcUrl:{this.SourceUrl}\r\n\t" +
                $"Parameters:{this.Parameters}, Client:{this.ClientIp},cv:{this.ClientVersion},ct:{this.ClientType},client info:{this.ClientInfo},desc:{this.Desc}\r\n\t" +
                $"Result:{this.Result}";
        }
    }
}
