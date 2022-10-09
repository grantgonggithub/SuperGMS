using AutoMapper;

namespace SuperGMS.DB.MapperEx
{
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
                        });
                        mapper = new Mapper(configuration);
                    }
                }
        }

        /// <summary>
        /// 自定义Automaper 属性Name转换,全属性转换
        /// </summary>
        internal class MyConvention : INamingConvention
        {
            /// <summary>
            /// Grant没意义, 因为写成空或者Null 会报错.
            /// </summary>
            public Regex SplittingExpression { get; } = new Regex(@"Grant");

            public string SeparatorCharacter => string.Empty;

            public string ReplaceValue(Match match)
            {
                return match.Value;
            }
        }
    }
}