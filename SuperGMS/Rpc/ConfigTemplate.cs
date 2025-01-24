/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Rpc
 文件名：  ConfigTemplate
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/3/14 10:41:12

 功能描述：

----------------------------------------------------------------*/
namespace SuperGMS.Rpc
{
    /// <summary>
    /// ConfigTemplate
    /// </summary>
    public class ConfigTemplate
    {
        public const string RPC_CLIENT= "<RpcClients>< Client ServerName=\"{0}\" RouterType=\"random\"><Item pool = \"{1}\" Ip=\"{2}\" Port=\"{3}\" Url=\"\" ServerType=\"thrift\" /></Client></RpcClients>";
    }
}
