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

                        // 不能在这里判断，因为及时不对，也必须接受websocket，然后返回websocket的状态码才行，要不前端接不到任何状态码
                        // https://stackoverflow.com/questions/21762596/how-to-read-status-code-from-rejected-websocket-opening-handshake-with-javascrip
                        //if (userCtx == null || userCtx.UserInfo == null)
                        //{
                        //    httpContext.Response.StatusCode = 403; // tk错误或者用户未登录
                        //}

                        var websocket = httpContext.WebSockets.AcceptWebSocketAsync().Result;
                        a.Headers = SuperHttpProxy.GetRequestIp(httpContext);
                        var superSocket = new SuperWebSocket(websocket, DateTime.Now, DateTime.Now, a);
                        if (userCtx == null || userCtx.UserInfo == null)
                        {
                            // httpContext.Response.StatusCode = 403; // tk错误或者用户未登录
                            superSocket.Close(closeStatus: System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, msgPrifx: "登录失败，客户端token超时");// tk超时：1000
                        }
                        else
                        {
                            SuperWebSocketManager.IsExistClient(token);

                            SuperWebSocketManager.OnConnected(new ComboxClass<UserType, SuperWebSocket> { V1 = (UserType)userCtx.UserInfo.UserType, V2 = superSocket });
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
