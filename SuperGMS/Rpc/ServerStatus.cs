namespace SuperGMS.Rpc
{
    public enum ServerStatus
    {
        /// <summary>
        /// 运行
        /// </summary>
        Running=1,
        /// <summary>
        /// 不存在，要么没安装，要么已卸载
        /// </summary>
        NotExist=2,
        /// <summary>
        /// 异常停止
        /// </summary>
        ErrorStop=3,

    }
}
