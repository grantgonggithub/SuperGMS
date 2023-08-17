using SuperGMS.Protocol.MQProtocol;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc;
using SuperGMS.Rpc.Server;
using SuperGMS.WebSocketEx;

namespace WebSocketService
{
    /// <summary>
    /// 给指定的WebSocket客户端发送消息
    /// </summary>
    public class SendMessageToUser : RpcBaseServer<EventMsg<string>, Nullables>
    {
        protected override Nullables Process(EventMsg<string> valueArgs, out StatusCode code)
        {
            code = StatusCode.OK;
            SuperWebSocketManager.SendMessage(valueArgs);
            return Nullables.NullValue;
        }

        protected override bool Check(EventMsg<string> args, out StatusCode code)
        {
            if (Context.Args.ct == ClientType.InnerRpc.ToString()||base.CheckLogin(args,out code))
            {
                code = StatusCode.OK;
                return true;
            }

            code = StatusCode.Unauthorized;
            return false;
        }
    }
}
