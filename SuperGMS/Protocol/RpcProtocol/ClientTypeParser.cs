using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.Protocol.RpcProtocol
{
    /// <summary>
    /// 客户端转换类
    /// </summary>
    public class ClientTypeParser
    {
        /// <summary>
        /// 转换客户端类型为枚举
        /// </summary>
        /// <param name="clientType"> 客户端类型字符串</param>
        /// <returns>枚举</returns>
        public static ClientType Parser(string clientType)
        {
            if (!Enum.TryParse<ClientType>(clientType,true, out var ct))
            {
                ct = ClientType.Unkunwn;
            }

            return ct;
        }
    }
}