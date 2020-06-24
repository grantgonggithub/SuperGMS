using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace SuperGMS.Log
{
    /// <summary>
    /// 日志记录器工厂
    /// </summary>
    public static class LogFactory
    {
        private readonly static LoggerFactory _fac;

        public static LoggerFactory LoggerFactory
        {
            get { return _fac; }
        }
        /// <summary>
        /// 构造一个文件日志提供者
        /// </summary>
        static LogFactory()
        {
            _fac = new LoggerFactory();
            _fac.AddProvider(new NLogLoggerProvider());
        }
        /// <summary>
        /// 构造日志记录器
        /// </summary>
        /// <param name="categoryName">分类</param>
        /// <returns>日志记录</returns>
        public static ILogger CreateLogger(string categoryName = "FrameWork")
        {
            return _fac.CreateLogger(categoryName);
        }
        /// <summary>
        /// 构造日志记录器
        /// </summary>
        /// <typeparam name="T">记录器使用泛型名称</typeparam>
        /// <returns></returns>
        public static ILogger CreateLogger<T>()
        {
            return _fac.CreateLogger(typeof(T).FullName);
        }
    }
}