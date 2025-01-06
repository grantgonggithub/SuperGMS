/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Rpc.HttpWebApi
 文件名：   GrantWebApiClient
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/8/21 17:42:01

 功能描述：

----------------------------------------------------------------*/
using SuperGMS.Tools;

using System.Text;

namespace SuperGMS.Rpc.HttpWebApi
{
    /// <summary>
    /// GrantWebApiClient
    /// </summary>
    public class WebApiClient : ISuperGMSRpcClient
    {
        private ClientItem clientItem;

        /// <inheritdoc />
        public ClientItem Item
        {
            get { return clientItem; }
        }

        public void Close()
        {
            return;
        }

        public void Dispose()
        {
            return;
        }

        public bool IsConnected => true;

        public WebApiClient(ClientItem server)
        {
            clientItem = server;
        }

        public bool Send(string args,string m, out string result)
        {
            string url =$"http://{clientItem.Ip}:{clientItem.Port}/{clientItem.Server.ServerName}";
            if (!string.IsNullOrEmpty(clientItem.Url))
            {
                url = clientItem.Url.StartsWith("http://") ? clientItem.Url:$"http://{clientItem.Url}";
            }
            url = url.EndsWith("/") ? $"{url}{m}" : $"{url}/{m}";
            using (var webClient = new WebClientEx { Encoding = Encoding.UTF8, TimeOut = clientItem.TimeOut })
            {         
                webClient.Headers.Add("Content-Type", "application/json;charset=utf-8");
                var postData = Encoding.UTF8.GetBytes(args);
                byte[] responseData = webClient.UploadData(url, "POST", postData);
                result = Encoding.UTF8.GetString(responseData);
                return true;
            }
        }
    }
}