using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SuperGMS.ExceptionEx;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc;
using SuperGMS.Rpc.Server;

namespace SuperGMS.ApiHelper
{
    /// <summary>
    /// 得到微服务接口列表帮助信息
    /// </summary>
    public abstract class BaseGetApiHelp : GrantRpcBaseServer<Nullables, List<ClassInfo>>
    {
        /// <summary>
        /// Gets 需实现的属性
        /// </summary>
        protected virtual Assembly Assembly { get; }

        public BaseGetApiHelp()
        {
            Assembly = Assembly.GetEntryAssembly();
        }

        /// <summary>
        /// GetAddress接口实现
        /// </summary>
        /// <param name="valueArgs">传入参数</param>
        /// <param name="code">传出状态码</param>
        /// <returns>接口返回值</returns>
        protected override List<ClassInfo> Process(Nullables valueArgs, out StatusCode code)
        {
            code = StatusCode.OK;
            try
            {
                var helper = new ApiHelper(this.Assembly);
                if (this.args.ct == "webapi")
                {
                    var list = helper.GetAllInterfaceClass(this.args.lg);
                    return list;
                }
                else
                {
                    var list = helper.GetAllInterfaceClass(this.args.lg, true);
                    return list;
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

    public class GetApiHelp : BaseGetApiHelp { }
}
