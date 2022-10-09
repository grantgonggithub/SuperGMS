using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperGMS.Config;
using SuperGMS.HttpProxy;
using SuperGMS.Rpc;
using Grant.HttpProxy.Middleware;
using System;

namespace SuperGMS.HttpProxy
{
    public class Startup
    {
        public Startup(IWebHostEnvironment configuration)
        {
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            // services.AddResponseCompression();

            // services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseResponseCompression();
            var server = ServerSetting.GetRpcServer(SuperGMS.HttpProxy.SuperHttpProxy.HttpProxyName);
            if (server.ServerType == ServerType.HttpWebApi)
            {
                ServerProxy.Register(typeof(Program));
            }
            else
            {
                SuperGMS.HttpProxy.SuperHttpProxy.Register();
            }
            app.UseCors("AllowAll");
            //WebSocketOptions socketOptions = new WebSocketOptions
            //{
            //    KeepAliveInterval = TimeSpan.FromSeconds(120),
            //    ReceiveBufferSize=4*1024,
            //};
            //app.UseWebSockets(socketOptions);
            app.UseMiddleware<ProxyMiddleware>();

            // app.UseMvc();
        }
    }
}
