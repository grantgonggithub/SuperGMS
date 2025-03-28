/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Rpc.Grpc
 文件名：GrpcServer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2025/1/14 15:11:38

 功能描述：

----------------------------------------------------------------*/
using Grpc.Core;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using SuperGMS.Config;

using System.Linq;
using System.Threading.Tasks;

namespace SuperGMS.Rpc.Grpc.Server
{
    /// <summary>
    /// 注意GrpcService.GrpcServiceBase继承了 SuperGMSBaseServer 保证了通信层和逻辑层绑定
    /// SuperGMSBaseServer是逻辑层  
    /// GrpcService.GrpcServiceBase 是通信层
    /// 这里的GrpcServer通过两次构造创建的对象进行了绑定，第一次通过无参构造初始化配置和逻辑层创建，
    /// 第二次通过通信 层注册走有参构造创建（无参对象作为有参对象的构造参数）
    /// </summary>
    internal class GrpcServer : GrpcService.GrpcServiceBase
    {
        private readonly SuperGMSBaseServer _server;

        public GrpcServer(SuperGMSBaseServer server)
        { 
            this._server = server; // 注入业务对象，在通讯层注册的时候使用这个构造
        }

        // 在刚开始初始化的时候通过New 构造这个对象
        public GrpcServer() { }

        public override Task<GrpcResponse> Send(GRpcRequest request, ServerCallContext context)
        {
            // 调用业务层，处理业务返回业务结果
            var rspArgs = _server.Send(request.MyArgs, context).Result;
            return Task<GrpcResponse>.FromResult(new GrpcResponse { MyResult=rspArgs });
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
                options.ConfigureEndpointDefaults(configureOptions => {
                    // 指定http2协议否则调用报错
                    configureOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });
                options.ListenAnyIP(server.Port);
            });
            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            })
            // 这里很重要，因为app.MapGrpcService<GrpcServer>() 这里是AspNetCore框架注入的（前面是new GrpcServer（）无参构造，只组织了配置和业务对象RpcDistributer），跟这里Register进来的是两个实例
            // 这里声明注入要走有参构造，把这个无参构造传进来（this），相当于业务层和通讯层绑定了
            .AddSingleton(new GrpcServer(this))
            .AddGrpc();

            var app = builder.Build();
            app.UseCors("AllowAll");

            // 获取gRPC服务器实例
            // 注册服务实例，这里会根据上面指定的有参构造注入
            app.MapGrpcService<GrpcServer>();

            app.Run();
        }
    }
}
