using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperGMS.Config;
using SuperGMS.HttpProxy;
using SuperGMS.Rpc;
using Grant.HttpProxy.Middleware;

namespace Grant.HttpProxy
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
            var server = ServerSetting.GetRpcServer(GrantHttpProxy.HttpProxyName);
            if (server.ServerType == ServerType.HttpWebApi)
            {
                ServerProxy.Register(server.AssemblyPath);
            }
            else
            {
                GrantHttpProxy.Register();
            }
            app.UseCors("AllowAll");
            app.UseMiddleware<ProxyMiddleware>();

            // app.UseMvc();
        }
    }
}
