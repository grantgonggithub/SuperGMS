using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc.Server;

namespace SuperGMS.Extend
{
    /// <summary>
    /// 微服务术语类，封装部分操作，缓存类型
    /// </summary>
    public class MicroServiceAssembly
    {
        /// <summary>
        /// RPC 基类
        /// </summary>
        private static readonly string RpcBaseName = typeof(RpcBaseServer<object, object>).GetGenericTypeDefinition().FullName;
        private static List<Type> ApiInterfaces;

        static MicroServiceAssembly()
        {
            var theAssembly = Assembly.GetEntryAssembly();
            ApiInterfaces = theAssembly.GetTypes().Where(IsGrantRpcBaseServerChildClass).ToList();
        }

        /// <summary>
        /// 根据指定的api类名，调用run方法执行api
        /// </summary>
        /// <param name="apiName">类名</param>
        /// <param name="args">参数</param>
        /// <param name="code">状态码</param>
        /// <param name="objValue">对象参数</param>
        /// <returns>结果值</returns>
        public static object Run(string apiName, Args<object> args, out StatusCode code, object objValue = null)
        {
            var t = ApiInterfaces.FirstOrDefault(x => x.Name.Equals(apiName));
            if (t == null)
            {
                throw new NotImplementedException($"{apiName} class dont found.");
            }

            dynamic apiInstance = Activator.CreateInstance(t);
            return apiInstance.Run(args, out code, objValue);
        }

        /// <summary>
        /// 判断是否Rpc接口类
        /// </summary>
        /// <param name="type">本微服务中的类型</param>
        /// <returns>是否Rpc接口类</returns>
        private static bool IsGrantRpcBaseServerChildClass(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().FullName == RpcBaseName)
            {
                return true;
            }

            return (type.BaseType != null) && IsGrantRpcBaseServerChildClass(type.BaseType);
        }
    }
}
