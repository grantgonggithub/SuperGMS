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
    public class SendMessageToUser : RpcBaseServer<EventMsg<object>, Nullables>
    {
        protected override Nullables Process(EventMsg<object> valueArgs, out StatusCode code)
        {
            code = StatusCode.OK;
            SuperWebSocketManager.SendMessage(valueArgs);
            return Nullables.NullValue;
        }

        protected override bool Check(EventMsg<object> args, out StatusCode code)
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
