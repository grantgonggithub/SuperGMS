/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Protocol.RpcProtocol
 文件名：  ArgsEx
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/3/5 13:08:19

 功能描述：

----------------------------------------------------------------*/
namespace SuperGMS.Protocol.RpcProtocol
{
    /// <summary>
    /// ArgsEx
    /// </summary>
    public static class ArgsEx
    {
        /// <summary>
        /// 拷贝一个当前上下文的args，作为下一个内部调用的args使用，
        /// 这里除了m和v没有拷贝，需要调用时赋值，其他都会原样拷贝结构和值
        /// </summary>
        /// <param name="args">当前上下文的args</param>
        /// <returns>返回除了m和v之外全新的Args</returns>
        public static Args<object> Copy(this Args<object> args)
        {
            return new Args<object>()
            {
                cs = args.cs,
                ct = args.ct,
                cv = args.cv,
                Headers = args.Headers,
                icp = args.icp,
                lg = args.lg,
                rid = args.rid,
                mv = args.mv,
                tk = args.tk,
                uri = args.uri,
            };
        }
    }
}
