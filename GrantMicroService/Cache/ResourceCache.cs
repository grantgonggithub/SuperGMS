/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Cache
 文件名：  DefaultCache
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/23 17:32:53

 功能描述：

----------------------------------------------------------------*/


namespace GrantMicroService.Cache
{
    /// <summary>
    /// 资源文件
    /// </summary>
    //[InitlizeMethod]
    public class ResourceCache
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
            _instance = new RedisCache("resource");
        }

        /// <summary>
        /// 单元测试时, Mock 缓存时,使用,手动初始化
        /// </summary>
        /// <param name="cache">Mock 的缓存</param>
        public static void Initlize(ICache cache)
        {
            if (_instance == null && cache != null)
            {
                _instance = cache;
            }
        }
    }
}