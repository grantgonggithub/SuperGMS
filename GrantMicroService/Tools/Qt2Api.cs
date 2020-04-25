using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GrantMicroService.Config;
using GrantMicroService.Log;
using GrantMicroService.Protocol.RpcProtocol;
using GrantMicroService.UserSession;
using System;
using System.Text;

namespace GrantMicroService.Tools
{
    /// <summary>
    /// 访问微服务自己的api
    /// </summary>
    public class Qt2Api
    {
        private string _token = string.Empty;
        private string _baseUrl = string.Empty;
        private readonly static ILogger logger = LogFactory.CreateLogger<Qt2Api>();
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="context"></param>
        public Qt2Api(UserContext context)
        {
            _token = context?.Token;
            _baseUrl = ServerSetting.GetConstValue("HttpProxy")?.Value ;
            if (string.IsNullOrEmpty(_baseUrl))
            {
                logger.LogError("服务没有配置HttpProxy常量，默认设置为开发版api网关地址!");
                _baseUrl = "http://192.168.100.121/api/";
            }
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="relatedUrl"></param>
        /// <param name="model"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TR Call<TA, TR>(string relatedUrl, TA model, int timeout = 6000)
        { 
            TR result = CallService<TA, TR>(relatedUrl, model, timeout);
            return result;            
        }

        private TR CallService<TA, TR>(string relatedUrl, TA model,int timeout)
        {
            if (relatedUrl.StartsWith("/"))
            {
                relatedUrl = relatedUrl.Substring(1);
            }

            try
            {
                using (var wc = new WebClientEx { Encoding = Encoding.UTF8, TimeOut = timeout })
                {
                    wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                    var args = Newtonsoft.Json.JsonConvert.SerializeObject(
                        new Args<TA>()
                        {
                            tk = _token,
                            v = model,
                        },
                        new Newtonsoft.Json.JsonSerializerSettings()
                        {
                            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                            DateFormatString = "yyyy-MM-dd HH:mm:ss",
                            Formatting = Newtonsoft.Json.Formatting.Indented
                        });
                    var postData = Encoding.UTF8.GetBytes(args);
                    byte[] responseData = wc.UploadData(string.Format("{0}{1}", _baseUrl, relatedUrl), "POST", postData);
                    var result = Encoding.UTF8.GetString(responseData);


                    Result<TR> apiResult = null;
                    try
                    {
                        apiResult = JsonConvert.DeserializeObject<Result<TR>>(result);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, $"反序列化失败:Request={args},Response={result}");
                        throw e;
                    }

                    if (apiResult.c == StatusCode.OK.code)
                    {
                        return apiResult.v;
                    }
                    else
                    {
                        throw new Exception($"rid:{apiResult.rid};状态码:{apiResult.c};说明:{apiResult.msg}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("url:" + _baseUrl + "/" + relatedUrl + ",token:" + _token + ",状态码:" + ex.Message);
            }
        }
    }
}
