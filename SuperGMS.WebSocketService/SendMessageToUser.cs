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
    public class SendMessageToUser : RpcBaseServer<EventMsgEx, Nullables>
    {
        protected override Nullables Process(EventMsgEx valueArgs, out StatusCode code)
        {
            code = StatusCode.OK;
            SuperWebSocketManager.SendMessage(valueArgs);
            return Nullables.NullValue;
        }

        protected override bool Check(EventMsgEx args, out StatusCode code)
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
