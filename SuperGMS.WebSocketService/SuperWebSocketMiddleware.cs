using SuperGMS.HttpProxy;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc.Server;
using SuperGMS.Tools;
using SuperGMS.WebSocketEx;

namespace WebSocketService
{
    public class SuperWebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public SuperWebSocketMiddleware(RequestDelegate next)
        { 
            this._next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var task = new Task(() => {
                if (httpContext.Request.Path == "/superwebsocket" && httpContext.WebSockets.IsWebSocketRequest)
                {
                    var token = httpContext.Request.Query["tk"].FirstOrDefault();
                    if (string.IsNullOrEmpty(token))
                    {
                        httpContext.Response.StatusCode = 402; // 参数错误
                    }
                    else
                    {
                        var clientType = httpContext.Request.Query["ct"].FirstOrDefault();
                        var a = new Args<object> { tk = token, ct = clientType };
                        // 查找redis的token是否存在，构建字典
                        RpcContext ctx = new RpcContext(null, a);
                        var userCtx = ctx.GetUserContext();
                        if (userCtx == null || userCtx.UserInfo == null)
                        {
                            httpContext.Response.StatusCode = 403; // tk错误或者用户未登录
                        }
                        else
                        {
                            if (!SuperWebSocketManager.IsExistClient(token))
                            {
                                var websocket = httpContext.WebSockets.AcceptWebSocketAsync().Result;
                                a.Headers = SuperHttpProxy.GetRequestIp(httpContext);
                                var superSocket = new SuperWebSocket(websocket, DateTime.Now, DateTime.Now, a);
                                SuperWebSocketManager.OnConnected(new ComboxClass<UserType, SuperWebSocket> { V1 = (UserType)userCtx.UserInfo.UserType, V2 = superSocket });
                            }
                            else
                            {
                                httpContext.Response.StatusCode = 401; // 同一个客户端不可以重复创建socket连接
                            }
                        }
                    }
                }
                else
                {
                    httpContext.Response.StatusCode = 400;// 协议和url格式错误
                }
            });
            task.Start();
            return task;
        }
    }
}
