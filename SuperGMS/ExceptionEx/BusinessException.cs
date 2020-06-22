using System;
using SuperGMS.Protocol.RpcProtocol;

namespace SuperGMS.ExceptionEx
{
    /// <summary>
    ///  业务异常,用来区分是代码级别的异常还是逻辑类的异常
    ///  业务的状态码必须大于600
    /// </summary>
    public class BusinessException : Exception
    {
        private StatusCode _code;

        public StatusCode Code { get { return _code; } }

        /// <summary>
        /// 构造的时候给一个异常说明
        /// </summary>
        /// <param name="message"></param>
        public BusinessException(StatusCode code) : base(code.msg)
        {
            _code = code;
        }

        /// <summary>
        /// 默认返回600，如果有特殊流程自己定义大于600的即可
        /// </summary>
        /// <param name="msg"></param>
        public BusinessException(string msg) : base(msg)
        {
            _code = new StatusCode(600, msg);
        }

        /// <summary>
        /// 默认返回600，如果有特殊流程自己定义大于600的即可
        /// </summary>
        /// <param name="msg"></param>
        public BusinessException(string msg, Exception exception) : base(msg, exception)
        {
            _code = new StatusCode(600, msg);
        }
    }
}