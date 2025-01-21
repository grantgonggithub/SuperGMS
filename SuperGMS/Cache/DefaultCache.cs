/*----------------------------------------------------------------
Copyright (C) 2021 顺兴文旅版权所有

项目名称：SuperGMS.Cache
文件名：  DefaultCache
创建者：  grant(巩建春)
CLR版本： 4.0.30319.42000
时间：    2021/8/5 星期四 13:45:50

功能描述：
----------------------------------------------------------------*/

namespace SuperGMS.Cache
{
    /// <summary>
    /// DefaultCache
    /// </summary>
    public class DefaultCache
    {
        private static ICache _instance;

        /// <summary>
        /// 操作redis的实例
        /// </summary>
        public static ICache Instance
        {
            get { return _instance; }
        }

        //[InitlizeMethod]
        internal static void Initlize()
        {
            _instance = new RedisCache();
        }

        /// <summary>
        /// 单元测试时, Mock 缓存时,使用,手动初始化
        /// </summary>
        /// <param name="cache">Mock 的缓存</param>
        internal static void Initlize(ICache cache)
        {
            if (_instance == null && cache != null)
            {
                _instance = cache;
            }
        }
    }
}
