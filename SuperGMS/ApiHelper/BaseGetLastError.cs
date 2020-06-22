using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using SuperGMS.ApiHelper;
using SuperGMS.Config;
using SuperGMS.ExceptionEx;
using SuperGMS.Log;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc;
using SuperGMS.Rpc.Server;
using SuperGMS.Tools;

namespace SuperGMS.ApiHelper
{
    /// <summary>
    /// 得到微服务接口列表帮助信息
    /// </summary>
    public abstract class BaseGetLastError : GrantRpcBaseServer<Nullables, List<string>>
    {
        /// <summary>
        /// GetAddress接口实现
        /// </summary>
        /// <param name="valueArgs">传入参数</param>
        /// <param name="code">传出状态码</param>
        /// <returns>接口返回值</returns>
        protected override List<string> Process(Nullables valueArgs, out StatusCode code)
        {
            code = StatusCode.OK;
            try
            {
                var cs = Cache.ResourceCache.Instance;
                if (!(cs?.IsExistCfg() ?? false))
                {
                    throw new BusinessException("本服务没有配置Reource缓存服务，因此无法获取错误!");
                }
                var ss = cs.PopAllQueue(ServerSetting.AppName);
                var logs = ss.Select(x =>
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<LogInfo>(x)?.ToString();
                    }
                    catch
                    {
                        return x;
                    }
                }).ToList();
                return logs;
            }
            catch (Exception ex)
            {
                throw new BusinessException(ex.Message);
            }
        }

        /// <summary>
        /// Check
        /// </summary>
        /// <param name="args">args</param>
        /// <param name="code">code</param>
        /// <returns>bool</returns>
        protected override bool Check(Nullables args, out StatusCode code)
        {
            code = StatusCode.OK;
            return true;
        }
    }

    public class GetLastError : BaseGetLastError
    {

    }

}
