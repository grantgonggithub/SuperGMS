/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Rpc.HttpWebApi
 文件名：   GrantWebApi
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/8/21 17:26:05

 功能描述：

----------------------------------------------------------------*/
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using SuperGMS.Config;
using SuperGMS.HttpProxy;
using SuperGMS.Protocol.RpcProtocol;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGMS.Rpc.HttpWebApi
{
    /// <summary>
    /// GrantWebApi
    /// </summary>
    public class WebApiServer : SuperGMSBaseServer, IMiddleware
    {
        private readonly WebApiServer server;

        public WebApiServer(WebApiServer server)
        {
            this.server = server;
        }

        public WebApiServer() { }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method?.ToUpper() != "POST")
            {
                throw new NotSupportedException();
            }
            var a = SuperHttpProxy.PaserProto(context, out var isUdf, out var serviceName);
            var rst = server.Send(null, a).Result;
            var rsp = JsonConvert.DeserializeObject<Result<object>>(rst);
            SuperHttpProxy.PaserResult(context, rsp, rst, isUdf);
            return next(context);
        }

        protected override void Dispose()
        {
           return;
        }

        protected override void ServerRegister(SuperGMSServerConfig server, string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseKestrel(options =>
            {
                //请求内容长度限制(单位B)
                int maxLength = 0;
                var maxBody = ServerSetting.Config.ConstKeyValue.Items.FirstOrDefault(i => i.Key == "MaxHttpBody")?.Value;
                if (!string.IsNullOrEmpty(maxBody))
                {
                    int.TryParse(maxBody, out maxLength);
                }
                if (maxLength < 4194304) maxLength = 4194304; // 最小4M
                if (maxLength > 104857600) maxLength = 104857600; // 最大100M
                options.Limits.MaxRequestBodySize = maxLength;
                options.AllowSynchronousIO = true;
                options.ListenAnyIP(server.Port);
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            })
            // 这里很重要，因为app.UseMiddleware<WebApiServer>() 这里是AspNetCore框架注入的（前面是new WebApiServer（）无参构造，只组织了配置和业务对象RpcDistributer），跟这里Register进来的是两个实例
            // 这里声明注入要走有参构造，把这个无参构造传进来（this），相当于业务层和通讯层绑定了
            .AddSingleton(new WebApiServer(this));

            var app = builder.Build();
            app.UseCors("AllowAll");

            // 获取gRPC服务器实例
            // 注册服务实例，这里会根据上面指定的有参构造注入
            app.UseMiddleware<WebApiServer>();

            app.Run();
        }
    }
}