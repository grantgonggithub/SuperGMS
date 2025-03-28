/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.AttributeEx
 文件名：  GetFormFieldAttribute
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/24 12:02:33

 功能描述：

----------------------------------------------------------------*/

namespace SuperGMS.Extend.EditFormFieldHelper
{
    using SuperGMS.Extend.BaseAppExtend.EditFormFieldHelper;
    using SuperGMS.Protocol.RpcProtocol;
    using SuperGMS.Rpc.Server;

    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// GetFormFieldAttribute
    /// </summary>
    public abstract class GetFormFieldAttribute : RpcBaseServer<EditFormApiArgs, EditFormApiResult[]>
    {
        private static Dictionary<string, List<EditFormApiResult>> apiArgs = null;

        private static object root = new object();

        /// <summary>
        /// 把当前程序集中的表单接口名和参数保存下来，供客户端拉取表单字段属性
        /// </summary>
        /// <param name="ass">ass</param>
        public static void Initlize()
        {
            if (apiArgs == null)
            {
                lock (root)
                {
                    if (apiArgs == null)
                    {
                        apiArgs = new Dictionary<string, List<EditFormApiResult>>();

                        // var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
                        // var assemblies = DependencyContext.Default.GetRuntimeAssemblyNames(runtimeId);
                        // var assArrary = assemblies.ToArray(); // 找到所有依赖的程序集
                        // var GrantAss = assArrary.Where(a => a.FullName.ToLower().StartsWith("grant")).ToArray();

                        // List<Type> allTypes = new List<Type>();
                        // foreach (var qt in GrantAss)
                        // {
                        //    Assembly asb = AssemblyLoadContext.Default.LoadFromAssemblyName(qt);
                        //    allTypes.AddRange(asb.GetTypes());
                        // }
                        ApiHelper.ApiHelper helper = new ApiHelper.ApiHelper(Assembly.GetEntryAssembly());
                        var list = helper.GetAllInterface(); // 找到所有的app
                        foreach (var t in list)
                        {
                            var param = helper.GetInterfaceParam(t); // 找每个app的args

                            // 看args是否标记为EditForm
                            var editAttr = EditFormHelper.GetEditForm(param.args);
                            if (editAttr != null)
                            {
                                // List<Type> dbList = new List<Type>();
                                // foreach (var edit in editAttr.DbContextFullName)
                                // {
                                //   var dbType = allTypes.Where(a => a.FullName.ToLower().EndsWith(edit.ToLower())).ToArray();
                                //   dbList.AddRange(dbType);
                                // }
                                List<EditFormApiResult> apiList = new List<EditFormApiResult>();
                                EditFormHelper.GetEditFormField(param.args, ref apiList);
                                var key = t.Name.ToLower();
                                if (apiArgs.ContainsKey(key))
                                {
                                    apiArgs[key] = apiList;
                                }
                                else
                                {
                                    apiArgs.Add(key, apiList);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="valueArgs">valueArgs</param>
        /// <param name="code">code</param>
        /// <returns>EditFormApiResult</returns>
        protected override EditFormApiResult[] Process(EditFormApiArgs valueArgs, out StatusCode code)
        {
            string key = valueArgs.ApiName;
            code = StatusCode.OK;

            if (apiArgs.ContainsKey(key))
            {
                return apiArgs[key].ToArray();
            }

            return null;
        }
    }
}
