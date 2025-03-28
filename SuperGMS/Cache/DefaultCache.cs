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
