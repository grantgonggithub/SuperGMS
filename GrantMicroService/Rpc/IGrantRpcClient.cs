/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.GrantRpc
 文件名：IGrantClient
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/5/8 15:11:38

 功能描述：

----------------------------------------------------------------*/
using System;

namespace GrantMicroService.Rpc
{
    public interface IGrantRpcClient:IDisposable
    {
        /// <summary>
        /// 连接信息
        /// </summary>
        ClientItem Item { get; }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        bool Send(string args,string m,out string result);
        /// <summary>
        /// 关闭掉,物理释放，这里把Dispose特殊用途了，
        /// 所以另外起了个关闭的名字，不清楚不要随便用
        /// </summary>
        void Close();
    }
}
