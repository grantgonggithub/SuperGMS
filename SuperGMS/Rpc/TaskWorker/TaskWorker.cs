using SuperGMS.Log;

namespace SuperGMS.Rpc.TaskWorker
{
    /// <summary>
    /// 长轮询的一个Base服务
    /// </summary>
    public class TaskWorker : SuperGMSBaseServer
    {
        private SuperGMSServerConfig server;

        /// <summary>
        ///
        /// </summary>
        protected override void Dispose()
        {
        }

        /// <summary>
        /// 定时任务的服务不需要注册，直接通过InitlizeMethodAttribute属性初始化
        /// </summary>
        /// <param name="server"></param>
        protected override void ServerRegister(SuperGMSServerConfig server, string[] args)
        {
            this.server = server;
        }
    }
}