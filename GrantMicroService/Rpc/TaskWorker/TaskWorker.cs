using GrantMicroService.Log;

namespace GrantMicroService.Rpc.TaskWorker
{
    /// <summary>
    /// 长轮询的一个Base服务
    /// </summary>
    public class TaskWorker : GrantBaseServer
    {
        private GrantServerConfig server;

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
        protected override void ServerRegister(GrantServerConfig server)
        {
            this.server = server;
        }
    }
}