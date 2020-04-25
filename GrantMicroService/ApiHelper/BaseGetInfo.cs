using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Xml.Linq;
using GrantMicroService.Config;
using GrantMicroService.ExceptionEx;
using GrantMicroService.Protocol.RpcProtocol;
using GrantMicroService.Rpc;
using GrantMicroService.Rpc.Server;
using GrantMicroService.Tools;

namespace GrantMicroService.ApiHelper
{
    /// <summary>
    /// 得到微服务接口列表帮助信息
    /// </summary>
    public abstract class BaseGetInfo : GrantRpcBaseServer<Nullables, ServiceInfo>
    {
        private static ServiceInfo _info = null;
        /// <summary>
        /// GetAddress接口实现
        /// </summary>
        /// <param name="valueArgs">传入参数</param>
        /// <param name="code">传出状态码</param>
        /// <returns>接口返回值</returns>
        protected override ServiceInfo Process(Nullables valueArgs, out StatusCode code)
        {
            code = StatusCode.OK;
            try
            {
                if (_info != null)
                {
                    return _info;
                }
                else
                {
                   // int port = 0;
                    var root = ServerSetting.GetRpcServer();
                    //if (root?.Element("RpcService") != null)
                    //{
                    //    XElement em = root.Element("RpcService");
                    //    XAttribute p = em?.Attribute("Port");
                    //    int.TryParse(p?.Value, out port);
                    //}
                   // var coreAssemblyInfo = FileVersionInfo.GetVersionInfo(typeof(object).Assembly.Location);

                    Assembly assembly = Assembly.GetEntryAssembly();
                    _info = new ServiceInfo()
                    {
                        NetCoreVersion = Environment.Version.ToString(),
                        Name = ServerSetting.AppName,
                        Version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion.ToString(), //AssemblyFileVersionAttribute
                        ReleaseDate = System.IO.File.GetLastWriteTime(assembly.Location),
                        AssemblyName = assembly.FullName,
                        AssemblyPath = assembly.Location,
                        ListenPort = root.Port,
                        ComputerAddress = root.Ip,
                        ComputerName = ServiceEnvironment.ComputerName,
                        ProcessInfo = ServiceEnvironment.ProcessInfo,
                        Config = ServerSetting.Config.SimpleClone(),
                    };
                    return _info;
                }
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

    public class GetInfo : BaseGetInfo
    {

    }

}
