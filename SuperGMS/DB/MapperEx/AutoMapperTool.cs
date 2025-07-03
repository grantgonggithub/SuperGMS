using AutoMapper;

namespace SuperGMS.DB.MapperEx
{
    using Microsoft.Extensions.Logging;
    using SuperGMS.Log;
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Auto Mapper 注册工具类
    /// </summary>
    public static class AutoMapperTool
    {
        /// <summary>
        /// AutoMapper实例
        /// </summary>
        public static Mapper Mapper => mapper == null ? throw new Exception("请先在系统启动时调用AutoMapperTool.Initlize方法"): mapper;
        private static Mapper mapper = null;
        private static object lockObj = new object();

        /// <summary>
        /// 初始化AutoMapper
        /// </summary>
        /// <param name="profiles"></param>
        public static void Initlize(Profile profiles)
        {
            if (mapper == null)
                lock (lockObj)
                {
                    if (mapper == null)
                    {
                        var configuration = new MapperConfiguration(cfg =>
                        {
                            cfg.AddProfile(profiles);
                        }, LogFactory.LoggerFactory);
                        mapper = new Mapper(configuration);
                    }
                }
        }
    }
}