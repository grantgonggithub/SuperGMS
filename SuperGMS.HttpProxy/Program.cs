using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using SuperGMS.Config;
using SuperGMS.HttpProxy;
using System.Linq;

namespace SuperGMS.HttpProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            // 要做weapi就需要指定服务名，因为根据服务名才能知道相关配置，这个是因为webapi在启动前首先要指定端口
            if (args != null && args.Length > 0)
            {
                SuperGMS.HttpProxy.SuperHttpProxy.HttpProxyName = args[0];
            }

            var host = WebHost.CreateDefaultBuilder()
                .UseUrls(
                    $"http://{ServerSetting.GetRpcServer(SuperHttpProxy.HttpProxyName).Ip}:{ServerSetting.GetRpcServer(SuperHttpProxy.HttpProxyName).Port}/")
                .UseStartup<Startup>().UseKestrel(options=> {
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
                }).Build();
            return host;
        }
    }
}
