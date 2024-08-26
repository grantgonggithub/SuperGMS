using SuperGMS.Config;
using SuperGMS.Log;
using SuperGMS.Rpc;
using SuperGMS.WebSocketEx;

using WebSocketService;
{
    ILogger _loger = LogFactory.CreateLogger<SuperWebSocket>();
    var builder = WebApplication.CreateBuilder(args);
    var server = ServerSetting.GetRpcServer(SuperWebSocketProxy.WebSocketProxy);

    // 添加设置允许跨域的配置
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    if (server == null || server.Port < 1) throw new Exception("请配置WebSocket的端口号");
    var url = $"http://*:{server.Port}/";
    _loger.LogInformation($"WebSocket监听地址：{url}");

    // 设置监听端口和包体大小的配置
    builder.WebHost.UseUrls(url).UseKestrel(options =>
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
    });

    var app = builder.Build();

    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseCors("AllowAll");

    //设置支持websocket
    app.UseWebSockets(new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromSeconds(120),
    });

    // 注册WebSocket，接收前端消息，上行消息
    app.UseMiddleware<SuperWebSocketMiddleware>();

    SuperWebSocketManager.Initlize();

    _=Task.Run(() =>
    {
        // 注册WebSocket服务为内部服务提供发送消息的接口（本地Rpc端口注册），接收后端消息，下行消息
        ServerProxy.Register(typeof(Program));
        // 作为代理层，需要注册拉取后端的所有服务的负载地址
        // Websocket只做后端推送，前端调用直接走接口，简化复杂度
       // SuperWebSocketProxy.Register();
    });

    app.Run();
}