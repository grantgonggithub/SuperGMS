using SuperGMS.Protocol.RpcProtocol;
using System;

namespace SuperGMS.Log
{
    /// <summary>
    /// 日志基类，抽象类，实现 公共方法
    /// </summary>
    public abstract class LogBase
    {
        /// <summary>
        /// 设置结果信息日志
        /// </summary>
        /// <param name="args">参数信息</param>
        /// <param name="result">结果信息</param>
        /// <param name="ex">异常</param>
        public abstract void SetInfo<T>(Args<object> args, Result<T> result = null, Exception ex = null);

        /// <summary>
        /// 自增序号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 事务ID
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 微服务名
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// api名称
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}
