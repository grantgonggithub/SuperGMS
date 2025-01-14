/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Rpc.HttpWebApi
 文件名：   GrantWebApiClient
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/8/21 17:42:01

 功能描述：

----------------------------------------------------------------*/
using SuperGMS.Rpc.Client;
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
        private WebClientEx webClient;
        private bool isConnected = false;

        /// <inheritdoc />
        public ClientItem Item
        {
            get { return clientItem; }
        }

        public void Close()
        {
            if (webClient != null)
            {
                isConnected = false;
                webClient.Dispose();
                webClient = null;
            }
        }


        /// <summary>
        /// 连接可用时,把用完的连接放入连接池
        /// 否则释放掉
        /// </summary>
        public void Dispose()
        {
            if (isConnected)
            {
                //客户端连接可用时,释放回连接池
                ClientConnectionManager.ReleaseClient(this);
            }
            else
            {
                // 释放掉
                Close();
            }
        }

        public bool IsConnected => isConnected;

        public WebApiClient(ClientItem server)
        {
            clientItem = server;
            webClient = new WebClientEx { Encoding = Encoding.UTF8, TimeOut = clientItem.TimeOut <= 0 ? 15 * 1000 : clientItem.TimeOut };
            webClient.Headers.Add("Content-Type", "application/json;charset=utf-8");
            isConnected = true;
        }

        public bool Send(string args, string m, out string result)
        {
            result = string.Empty;
            try
            {
                string url = $"http://{clientItem.Ip}:{clientItem.Port}/{clientItem.Server.ServerName}";
                if (!string.IsNullOrEmpty(clientItem.Url))
                {
                    url = clientItem.Url.StartsWith("http://") ? clientItem.Url : $"http://{clientItem.Url}";
                }
                url = url.EndsWith("/") ? $"{url}{m}" : $"{url}/{m}";

                var postData = Encoding.UTF8.GetBytes(args);
                byte[] responseData = webClient.UploadData(url, "POST", postData);
                result = Encoding.UTF8.GetString(responseData);
                isConnected = true;
                return true;
            }
            catch {
                isConnected = false;
                throw;
            }
        }
    }
}