using SuperGMS.Config;
using SuperGMS.Log;
using SuperGMS.Rpc;
using SuperGMS.WebSocketEx;

using WebSocketService;
{
    ILogger _loger = LogFactory.CreateLogger<SuperWebSocket>();
    var builder = WebApplication.CreateBuilder(args);
    var server = ServerSetting.GetRpcServer(SuperWebSocketProxy.WebSocketProxy);

    // �������������������
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    if (server == null || server.Port < 1) throw new Exception("������WebSocket�Ķ˿ں�");
    var url = $"http://*:{server.Port}/";
    _loger.LogInformation($"WebSocket������ַ��{url}");

    // ���ü����˿ںͰ����С������
    builder.WebHost.UseUrls(url).UseKestrel(options =>
    {
        //�������ݳ�������(��λB)
        int maxLength = 0;
        var maxBody = ServerSetting.Config.ConstKeyValue.Items.FirstOrDefault(i => i.Key == "MaxHttpBody")?.Value;
        if (!string.IsNullOrEmpty(maxBody))
        {
            int.TryParse(maxBody, out maxLength);
        }
        if (maxLength < 4194304) maxLength = 4194304; // ��С4M
        if (maxLength > 104857600) maxLength = 104857600; // ���100M
        options.Limits.MaxRequestBodySize = maxLength;
        options.AllowSynchronousIO = true;
    });

    var app = builder.Build();

    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseCors("AllowAll");

    //����֧��websocket
    app.UseWebSockets(new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromSeconds(120),
    });

    // ע��WebSocket����Ϊ�ڲ������ṩ������Ϣ�Ľӿڣ�����Rpc�˿�ע�ᣩ�����պ����Ϣ��������Ϣ
    ServerProxy.Register(typeof(Program));

    // ��Ϊ����㣬��Ҫע����ȡ��˵����з���ĸ��ص�ַ
    SuperWebSocketProxy.Register();

    // ע��WebSocket������ǰ����Ϣ��������Ϣ
    app.UseMiddleware<SuperWebSocketMiddleware>();

    SuperWebSocketManager.Initlize();

    app.Run();
}