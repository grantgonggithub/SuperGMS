using System;

namespace SuperGMS.ExceptionEx
{
    /// <summary>
    /// 框架异常,上层业务类应该捕获框架类异常并做进一步处理
    /// </summary>
    public class FrameworkException : Exception
    {
        /// <summary>
        /// 构造的时候给一个异常说明
        /// </summary>
        /// <param name="message"></param>
        protected FrameworkException(string message) : base(message)
        {

        }
        /// <summary>
        /// 构造的时候给一个异常说明
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        protected FrameworkException(string message, Exception inner = null) : base(message, inner)
        {

        }

        public static FrameworkException CreateNew(string message)
        {
            return new FrameworkException(message);
        }
        public static FrameworkException CreateNew(string message,Exception ex)
        {
            return  new FrameworkException(message, ex);
        }
        
    }
}
