/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.GrantRpc.Server
 文件名：GrantRpcDistributer
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:46:51

 功能描述：

----------------------------------------------------------------*/

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SuperGMS.Config;
using SuperGMS.Log;
using SuperGMS.Protocol.RpcProtocol;
using SuperGMS.Rpc.AssemblyTools;
using SuperGMS.Tools;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace SuperGMS.Rpc.Server
{
    /// <summary>
    /// 一个Rpc分发器 ,
    /// </summary>
    internal class RpcDistributer
    {
        private Dictionary<string, ComboxClass<Type, MethodInfo>> servers = null;
        private List<ComboxClass<Type, MethodInfo>> disposeLink = null;
        private readonly static ILogger logger = LogFactory.CreateLogger<RpcDistributer>();
        private object root = new object();

        private static JsonSerializerSettings resultJsonSetting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Populate,
            FloatParseHandling = FloatParseHandling.Decimal
        };
        private string assShortName;
        /// <summary>
        /// Initializes a new instance of the <see cref="RpcDistributer"/> class.
        /// GrantRpcDistributer
        /// </summary>
        /// <param name="config">服务配置</param>
        public RpcDistributer(SuperGMSServerConfig config)
        {
            Initlize(config);
        }

        /// <summary>
        /// Gets 微服务注册名称
        /// </summary>
        public string ShortName
        {
            get { return assShortName; }
        }

        /// <summary>
        /// 底层传输上来的协议内容
        /// </summary>
        /// <param name="args">args</param>
        /// <param name="context">context</param>
        /// <returns>string</returns>
        public string Distributer(string args, object context)
        {
            var msg = $"收到请求：  args={args}  |  context={JsonConvert.SerializeObject(context, resultJsonSetting)}";
            logger.LogInformation(msg);
            // 构造一个请求日志
            var requestLog = new LogRequest()
            {
                Parameters = msg
            };
            StatusCode code = StatusCode.OK;
            string rid = string.Empty;
            Args<object> a = null;
            try
            {
                if (context is Args<object> b) a = b; // webApi上来的数据不做二次反序列化
                else
                 a = JsonConvert.DeserializeObject<Args<object>>(args, resultJsonSetting);
            }
            catch (Exception ex)
            {
                code = StatusCode.ArgesError;
                logger.LogError(ex, "反序列化参数[args]异常");
            }

            ComboxClass<Type, MethodInfo> tInfo = null;
            if (a == null)
            {
                code = StatusCode.ArgesError;
            }
            else
            {
                if (!servers.TryGetValue(a.m.ToLower(), out tInfo))
                {
                    code = StatusCode.MethodNotExist;
                }
                else
                {
                    try
                    {
                        object obj = Activator.CreateInstance(tInfo.V1);
                        object[] o = new object[] { a, code, context };
                        object r = tInfo.V2.Invoke(obj, o);
                        code = (StatusCode)o[1];
                        if (r != null)
                        {
                            string rr = JsonConvert.SerializeObject(r, resultJsonSetting);
                            requestLog.Result = rr;
                            requestLog.SetInfo(a, new Result<object>()
                            {
                                c = code.code,
                                msg = code.msg,
                            });                          
                            logger.LogInformation($"请求处理正常结束，返回值是：{rr}");
                            return rr;
                        }
                    }
                    catch (Exception ex)
                    {
                        code = StatusCode.ServerError;
                        code.msg = $"{ex.Message}:{ex.StackTrace},rid={a.rid}";
                        logger.LogError(ex, $"请求处理异常结束,{code.msg}");
                    }
                }

                rid = a.rid;
            }

            Result<object> rst = new Result<object>();
            rst.c = code.code;
            rst.msg = code.msg.ToLower();
            string rs = JsonConvert.SerializeObject(rst, resultJsonSetting);
            logger.LogInformation($"请求异常返回:{rs}");
            return rs;
        }

        /// <summary>
        /// 初始化当前路径下所有的rpc类库
        /// </summary>
        private void Initlize(SuperGMSServerConfig config)
        {
            try
            {
                if (servers == null)
                {
                    lock (root)
                    {
                        if (servers == null)
                        {
                            servers = new Dictionary<string, ComboxClass<Type, MethodInfo>>();
                            string longName = config.AssemblyPath;
                            if (string.IsNullOrEmpty(longName))
                            {
                                string msg = "Web Config is not Find the key 'Apps'  or the value is null";
                                throw new Exception(msg);
                            }

                            string[] appList = longName.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            if (appList.Length < 1)
                            {
                                string msg = "Web Config  key 'Apps' is error";
                                throw new Exception(msg);
                            }
                            bool isConfig = false;
                            foreach (string s in appList)
                            {
                                Assembly asLoad = null;
                                Assembly frameWorkAss = null;
                                string path = s;

                                // if (!File.Exists(longName)) //这个判断是个坑
                                // 这个需要判断是相对路径还是绝对路径
                                if (!Path.IsPathRooted(s))
                                {
                                    logger.LogDebug($"AppContext.BaseDirectory:{AppContext.BaseDirectory}");
                                    path = string.Format("{0}{1}", AppContext.BaseDirectory, s);
                                }

                                logger.LogDebug("Assembly 路径是:" + path);
                                try
                                {
                                    // Assembly ass = Assembly.LoadFrom(path);
                                    // asLoad =Assembly.Load(ass.FullName);
                                    // AssemblyName assName= AssemblyLoadContext.GetAssemblyName(path);
                                    // asLoad=AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                                    AssemblyLoadContext.Default.Resolving +=
                                        (AssemblyLoadContext arg1, AssemblyName args2) =>
                                        {
                                            // string p = path.Substring(0, path.LastIndexOf("\\"));
                                            // return arg1.LoadFromAssemblyPath(string.Format("{0}\\{1}.dll", p, args2.Name));
                                            try
                                            {
                                                // fix .resource.dll load error
                                                // to see https://github.com/dotnet/coreclr/issues/8416
                                                if (args2.Name.EndsWith(".resources"))
                                                {
                                                    return null;
                                                }
                                                logger.LogInformation($"尝试加载：{args2.FullName}");
                                                string p = path.Substring(0,
                                                    path.LastIndexOf(Path.DirectorySeparatorChar));
                                                return arg1.LoadFromAssemblyPath(string.Format("{0}{1}{2}.dll", p,
                                                    Path.DirectorySeparatorChar, args2.Name));
                                            }
                                            catch (ReflectionTypeLoadException RTLEx)
                                            {
                                                logger.LogCritical(RTLEx, $"无法加载程序集:{args2.Name}");
                                                if (RTLEx.LoaderExceptions != null && RTLEx.LoaderExceptions.Length > 0)
                                                {
                                                    foreach (var lex in RTLEx.LoaderExceptions)
                                                    {
                                                        logger.LogError(lex, "程序集加载异常");
                                                    }
                                                }

                                                throw;
                                            }
                                            catch (TypeLoadException TLEx)
                                            {
                                                logger.LogCritical(TLEx, $"无法加载程序集:{args2.Name}");
                                                throw;
                                            }
                                            catch (FileNotFoundException FNtEx)
                                            {
                                                logger.LogCritical(FNtEx, $"无法加载程序集:{args2.Name}");
                                                throw;
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.LogCritical(ex, $"加载程序集{args2.FullName}失败");
                                                throw;
                                            }
                                        };
                                    asLoad = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                                    if (!isConfig)
                                    {
                                        frameWorkAss =
                                            AssemblyLoadContext.Default.LoadFromAssemblyPath(
                                                $"{AppContext.BaseDirectory}SuperGMS.dll");
                                    }
                                    assShortName = AssemblyToolProxy.GetCurrentAppName(asLoad);

                                    // 在程序集初始化之前需要初始化加载App配置信息
                                    if (!isConfig)
                                    {
                                        isConfig = true;
                                        ServerSetting.Initlize(assShortName, config.Pool);
                                    }
                                }
                                catch (ReflectionTypeLoadException RTLEx)
                                {
                                    logger.LogCritical(RTLEx, "无法加载程序集");
                                    if (RTLEx.LoaderExceptions != null && RTLEx.LoaderExceptions.Length > 0)
                                    {
                                        foreach (var lex in RTLEx.LoaderExceptions)
                                        {
                                            logger.LogError(lex, "程序集加载异常");
                                        }
                                    }

                                }
                                catch (TypeLoadException TLEx)
                                {
                                    logger.LogCritical(TLEx, $"无法加载程序集:{TLEx.TypeName}");
                                }
                                catch (FileNotFoundException FNtEx)
                                {
                                    logger.LogCritical(FNtEx, $"无法加载程序集:{FNtEx.FileName}");
                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }

                                List<Type> types = new List<Type>();
                                if (asLoad != null)
                                {
                                    types.AddRange(asLoad.GetTypes());
                                }
                                if (frameWorkAss!=null)
                                {
                                    types.AddRange(frameWorkAss.GetTypes());
                                }
                                foreach (Type t in types)
                                {
                                    #region //加载所有继承了AppBase的类

                                    if (isChildAppBase(t) && !t.IsAbstract)
                                    {
                                        if (!servers.ContainsKey(t.Name.ToLower()))
                                        {
                                            servers.Add(t.Name.ToString().ToLower(),
                                                new ComboxClass<Type, MethodInfo>() {V1 = t, V2 = t.GetMethod("Run")});
                                            logger.LogDebug(string.Format("初始化了appPath={0},appName={1}", path, t.Name));
                                        }
                                    }

                                    #endregion //加载所有继承了AppBase的类

                                    #region 加载所有标记了需要初始化的方法

                                    object[] attrs = t.GetCustomAttributes(true);
                                    foreach (Attribute at in attrs)
                                    {
                                        if (at is InitlizeMethodAttribute)
                                        {
                                            MethodInfo[] methods = t.GetMethods(
                                                BindingFlags.InvokeMethod | BindingFlags.Public |
                                                BindingFlags.Instance | BindingFlags.Static);
                                            if (methods == null || methods.Length < 1) continue;
                                            foreach (MethodInfo info in methods)
                                            {
                                                Attribute atr =
                                                    info.GetCustomAttribute(typeof(InitlizeMethodAttribute));
                                                if (atr == null) continue;
                                                t.InvokeMember(info.Name,
                                                    BindingFlags.InvokeMethod | BindingFlags.Public |
                                                    BindingFlags.Instance | BindingFlags.Static, null, null, null);
                                                logger.LogDebug("初始化了方法" + t.FullName + "." + info.Name);
                                            }
                                        }
                                        if (at is UnRegisterMethodAttribute)
                                        {
                                            MethodInfo[] methods = t.GetMethods(
                                                BindingFlags.InvokeMethod | BindingFlags.Public |
                                                BindingFlags.Instance | BindingFlags.Static);
                                            if (methods == null || methods.Length < 1) continue;
                                            foreach (MethodInfo info in methods)
                                            {
                                                Attribute atr =
                                                    info.GetCustomAttribute(typeof(UnRegisterMethodAttribute));
                                                if (atr == null) continue;
                                                if (disposeLink == null)
                                                    disposeLink = new List<ComboxClass<Type, MethodInfo>>();
                                                disposeLink.Add(new ComboxClass<Type, MethodInfo> {V1 = t, V2 = info});
                                                logger.LogDebug("解析了Dispose方法" + t.FullName + "." + info.Name);
                                            }
                                        }
                                    }

                                    #endregion 加载所有标记了需要初始化的方法
                                }
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException RTLEx)
            {
                logger.LogCritical(RTLEx, "无法加载程序集");
                if (RTLEx.LoaderExceptions != null && RTLEx.LoaderExceptions.Length > 0)
                {
                    foreach (var lex in RTLEx.LoaderExceptions)
                    {
                        logger.LogError(lex, "无法加载程序集");
                    }
                }
            }
            catch (TypeLoadException TLEx)
            {
                logger.LogCritical(TLEx, $"无法加载程序集:{TLEx.TypeName}");
            }
            catch (FileNotFoundException FNtEx)
            {
                logger.LogCritical(FNtEx, $"无法加载程序集:{FNtEx.FileName}");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, string.Format("初始化assAssemblyPath={0}出现错误！", config.AssemblyPath));
                throw; // 初始化失败，严重错误，直接抛到顶层去，不让程序启动。
            }
        }

        /// <summary>
        /// isChildAppBase
        /// </summary>
        /// <param name="t">tt</param>
        /// <returns>bool</returns>
        public static bool isChildAppBase(Type t)
        {
            if (t == null)
            {
                return false;
            }

            if (t.Name.StartsWith("RpcBaseServer"))
            {
                return true;
            }

            return isChildAppBase(t.BaseType);
        }

        /// <summary>
        /// 通知业务服务请求停止，赶紧进行清理工作
        /// </summary>
        /// <returns>bool</returns>
        public bool Dispose()
        {
            try
            {
                if (disposeLink == null || disposeLink.Count < 1)
                {
                    return true;
                }

                int error = 0;
                for (int i = 0; i < disposeLink.Count; i++)
                {
                    try
                    {
                        object obj = Activator.CreateInstance(disposeLink[i].V1);
                        object r = disposeLink[i].V2.Invoke(obj, null);
                        logger.LogInformation(string.Format("执行回收方法{0}.{1}", disposeLink[i].V1.FullName, disposeLink[i].V2.Name));
                    }
                    catch
                    {
                        error += 1;
                    }
                }

                return error == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}