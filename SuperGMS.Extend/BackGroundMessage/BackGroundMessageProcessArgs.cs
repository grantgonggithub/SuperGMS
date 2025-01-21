/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：grant.RpcProxy.GlobalTools.BackGroundMessage
 文件名：  GetBackGroundMessageProcessArgs
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/30 10:27:02

 功能描述：

----------------------------------------------------------------*/

using SuperGMS.Protocol.RpcProtocol;

namespace SuperGMS.Extend.BackGroundMessage
{
    public class BackGroundMessageProcessArgs
    {
        /// <summary>
        /// 任务Id
        /// </summary>
        public string TaskGuid { get; set; }
    }

    /// <summary>
    /// GetBackGroundMessageProcessArgs
    /// </summary>
    public class BackGroundMessageProcessResult
    {
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalNum { get; set; }

        /// <summary>
        /// 处理记录数
        /// </summary>
        public int ProcessNum { get; set; }

        /// <summary>
        /// 成功数量
        /// </summary>
        public int SuccessNum
        {
            get;
            set;
        }

        /// <summary>
        /// 自定义任务数据
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 请求Rid
        /// </summary>
        public string Rid { get; set; }

        /// <summary>
        /// 执行状态码，用于标识执行情况，正常是200，
        /// 其他，如数据格式错误，如果状态码不为200，就需要解析Data，这个可能是返回的错误Execl下载路径，具体内容需要业务层和前端共同确定
        /// </summary>
        public StatusCode Code { get; set; }
    }
}
