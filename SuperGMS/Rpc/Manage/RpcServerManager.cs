// *----------------------------------------------------------------
// Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

// 项目名称：SuperGMS.GrantRpc.Server
// 文件名：GrantRpcServerManager
// 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
// CLR版本：4.0.30319.42000
// 时间：2017/4/19 17:02:59

// 功能描述：RpcServer的统一管理类
// Windows 下的微服务管理类
//----------------------------------------------------------------*/
#pragma warning disable
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Xml.Linq;
//using System.IO;
//using SuperGMS.Log;
//using SuperGMS.Tools;
//using SuperGMS.Protocol.RpcProtocol;
//using Newtonsoft.Json;

//namespace  SuperGMS.Rpc.Server
//{
//    /// <summary>
//    /// RpcServer的统一管理类
//    /// </summary>
//    public class GrantRpcServerManager
//    {
//        private static readonly Dictionary<string,ComboxClass<GrantServerConfig,AppDomain,ServerStatus,ServerProxy>> apps = new Dictionary<string,ComboxClass<GrantServerConfig,AppDomain,ServerStatus, ServerProxy>>();

//        private static object root = new object();
//        /// <summary>
//        /// 注册Rpc服务
//        /// </summary>
//        public static void RpcServerRegister()
//        {
//            XElement root = null; //GrantFrameWorkCoreConfig.GetConfigXElement();
//            if (root != null && root.Element("GrantRpcServices") != null)
//            {
//                IEnumerable<XElement> servers = root.Element("GrantRpcServices").Elements("GrantService");
//                if (servers != null)
//                {
//                    foreach (XElement em in servers)
//                    {
//                        string serverName = em.Attribute("ServerName") == null ? "unknown_server" : em.Attribute("ServerName").Value;
//                        IEnumerable<XElement> items = em.Elements("Item");
//                        foreach (XElement it in items)
//                        {
//                            XAttribute p = it.Attribute("Port");
//                            if (p == null)
//                            {
//                                //在这里咋处理还没想好
//                            }
//                            int port = 0;
//                            if (!int.TryParse(p.Value, out port))
//                            {
//                                //这里也没想好
//                            }
//                            XAttribute st = it.Attribute("ServerType");
//                            if (st == null)
//                            {
//                                //
//                            }
//                            XAttribute ass = it.Attribute("AssemblyPath");
//                            if (ass == null)
//                            {
//                                //
//                            }
//                            GrantServerConfig s = new GrantServerConfig()
//                            {
//                                ServerName = serverName,
//                                Port = port,
//                                ServerType = ServerTypeParse.Parse(st.Value),
//                                AssemblyPath = ass.Value
//                            };
//                            Register(s,true);
//                        }
//                    }

//                }
//            }
//        }

//        /// <summary>
//        /// 根据配置注册
//        /// </summary>
//        /// <param name="config"></param>
//        /// <param name="isAutoRegister">是否主动注册</param>
//        public static StatusCode Register(GrantServerConfig config,bool isAutoRegister=true)
//        {
//            //GrantBaseServer server = null;
//            //switch (config.ServerType)
//            //{
//            //    case ServerType.Thrift:
//            //        server = new GrantThriftServer();
//            //        break;
//            //    case ServerType.Grpc:
//            //        break;
//            //    case ServerType.WCF:
//            //        break;

//            //}
//            //if (server == null)
//            //{
//            //    //
//            //}
//            //else //开启线程去监听，要不当前主线程会被hang住
//            //    System.Threading.Tasks.Task.Run(() =>
//            //    {
//            //        server.RpcServerRegister(config);
//            //    });
//            string key = getKey(config);//同一个服务器上如果端口重复就任务已经注册过了，不能重复注册
//            bool canAdd = true;
//            lock (root)
//            {
//                if (!apps.ContainsKey(key))
//                {
//                    canAdd = true;
//                    //apps.Add(key, RegisterServer(config));
//                }
//                else canAdd = false;
//            }
//            if (canAdd)
//            {
//                try
//                {
//                    ServerProxy proxy=null;
//                    AppDomain app = RegisterServer(config,out proxy);
//                    apps.Add(key, new ComboxClass<GrantServerConfig, AppDomain, ServerStatus,ServerProxy> { v1=config, v2=app, v3=ServerStatus.Running,v4=proxy });
//                     GrantLogTextWriter.Write(string.Format("成功注册了服务:{0}",JsonConvert.SerializeObject(config)));
//                    //保存当前安装app的快照,便于重启的时候，自动加载进来
//                    if(!isAutoRegister)//非主动注册的才保存
//                      saveLocalConfig();
//                    //将自己注册到zookeeper
//                    return StatusCode.OK;
//                }
//                catch (Exception ex)
//                {
//                    string msg = string.Format("{0}_{1}", ex.Message, config.ToString());
//                     GrantLogTextWriter.Write(new Exception("GrantRpcServerManager.Register.Error"+msg, ex));
//                    return new StatusCode(600,msg);
//                }
//            }
//            else
//            {
//                string msg = "端口被占用：" + config.ToString();
//                 GrantLogTextWriter.Write(new Exception("GrantRpcServerManager.Register.Error" + msg));
//                return new StatusCode(700,msg);
//            }
//        }

//        private static string getKey(GrantServerConfig config)
//        {
//            return string.Format("ip_{0}_port_{1}",config.ServerName, config.Port);
//        }

//        /// <summary>
//        /// 卸载当前的AppDomain，相当于卸载DLL
//        /// </summary>
//        /// <param name="config"></param>
//        /// <param name="isAutoUnRegister">是否主动释放</param>
//        /// <returns></returns>
//        public static StatusCode UnRegister(GrantServerConfig config,bool isAutoUnRegister = true)
//        {
//            try
//            {
//                string key = getKey(config);
//                lock (root)
//                {
//                    if (apps.ContainsKey(key))
//                    {
//                        apps[key].v4.Dispose();//把服务释放了，在卸载，要不卸载不了
//                        AppDomain.Unload(apps[key].v2);
//                        apps.Remove(key);
//                        //保存当前安装app的快照,便于重启的时候，自动加载进来
//                        if(!isAutoUnRegister)//主动释放，在外面保存，只有手动注册才更新
//                          saveLocalConfig();
//                        //将这个变更通知zookeeper
//                    }
//                }
//                return StatusCode.OK;
//            }
//            catch (Exception ex)
//            {
//                GrantLogTextWriter.Write(new Exception("UnRegister.Error.config=" + config.ToString(), ex));
//                return new StatusCode(600, ex.Message);
//            }
//        }

//        /// <summary>
//        /// 获取所有的服务状态
//        /// 先这样，后面可以获取每个app占用的内存和cpu
//        /// </summary>
//        /// <returns></returns>
//        public static AppServerInfo[] GetStatus()
//        {
//            lock (root) {
//                return apps.Values.ToArray()
//                    .Select<ComboxClass<GrantServerConfig, AppDomain, ServerStatus,ServerProxy>, AppServerInfo>
//                    (a => new AppServerInfo {   Config= a.v1,  Status = a.v3, Cputime=a.v2.MonitoringTotalProcessorTime, Memory=a.v2.MonitoringSurvivedMemorySize }).ToArray();
//            }
//        }

////        private static AppDomain RegisterServer(GrantServerConfig config, out ServerProxy proxy)
////        {
////            string path = config.AssemblyPath;
////            if (path.IndexOf(":\\") > -1)//绝对路径
////            {
////                //isCurrent = false;
////            }
////            else//相对路径，只能是当前的相对路径
////            {
////                path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd("\\".ToCharArray()) + "\\" + config.AssemblyPath;
////                //isCurrent = true;
////            }
////             GrantLogTextWriter.Write("path=" + path);
////            config.AssemblyPath = path;
////            path = config.AssemblyPath.Substring(0, config.AssemblyPath.LastIndexOf("\\"));
////            AppDomainSetup ads = new AppDomainSetup();
////            ads.ApplicationName = path.Replace("\\", ".").Replace(":", "");
////            ads.ApplicationBase = path;
////            ads.PrivateBinPath = path;
////            ads.CachePath = path;
////            //ads.ShadowCopyFiles = "true";
////            ads.ShadowCopyDirectories = path;
////            //ads.DisallowBindingRedirects = false;
////            //ads.DisallowCodeDownload = true;
////            ads.LoaderOptimization = LoaderOptimization.SingleDomain;
////            ads.PrivateBinPathProbe = path;
////            ads.ConfigurationFile = config.AssemblyPath.TrimEnd("\\".ToArray()) + ".config";
////            if (!System.IO.File.Exists(ads.ConfigurationFile))
////                ads.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
////            ads.ApplicationTrust = AppDomain.CurrentDomain.ApplicationTrust;
////            Evidence evidence = AppDomain.CurrentDomain.Evidence;
////            AppDomain app = AppDomain.CreateDomain(path, evidence, ads);

////            //app.DomainUnload += App_DomainUnload;
////            //app.FirstChanceException += App_FirstChanceException;
////            //app.UnhandledException += App_UnhandledException;

////            proxy = (ServerProxy)app.CreateInstanceFromAndUnwrap(path.TrimEnd("\\".ToCharArray()) + "\\" + "SuperGMS.dll",
////typeof(ServerProxy).FullName);
////            proxy.Register(config.Port, config.ServerType.ToString(), config.AssemblyPath, config.ServerName);
////            return app;
////        }

//        //private static void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
//        //{
//        //    LogEx.LogError(JsonConvert.SerializeObject(e.ExceptionObject));
//        //}

//        //private static void App_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
//        //{
//        //    LogEx.LogError(new Exception("App_FirstChanceException.Error", e.Exception));
//        //}

//        //private static void App_DomainUnload(object sender, EventArgs e)
//        //{
//        //    //throw new NotImplementedException();
//        //}

//        /// <summary>
//        /// 自动注册本地服务
//        /// </summary>
//        public static void AutoSetup()
//        {
//            try
//            {
//                GrantServerConfig[] cfgs = null;
//                using (FileStream stream = new FileStream(configPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
//                {
//                    using (StreamReader reader = new StreamReader(stream))
//                    {
//                        string cfgStr = reader.ReadToEnd();
//                        cfgs = JsonEx.JsonConvert.DeserializeObject<GrantServerConfig[]>(cfgStr);
//                    }
//                }
//                if (cfgs == null || cfgs.Length < 1)
//                {
//                    Log.LogEx.LogError("GrantRpcServerManager.AutoSetup.Empty.localConfig");
//                }
//                foreach (GrantServerConfig c in cfgs)
//                {
//                    if (c.ServerName.ToLower() == SuperGMS.RpcProxy.GrantHAWorkerRpcProxy.serviceName.ToLower())
//                        continue;//HAWorker默认管理程序，不用再二次加载
//                    Register(c,true);
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.LogEx.LogError(new Exception("GrantRpcServerManager.AutoSetup.Error", ex));
//            }
//        }

//        /// <summary>
//        /// 停止服务
//        /// </summary>
//        public static void Stop()
//        {
//            //停止之前把本地正在运行的app配置保存下来
//            saveLocalConfig();
//            //依次注销
//            if (apps != null && apps.Count > 0)
//            {
//                GrantServerConfig[] cfgs = apps.Values.Select(a => a.v1).ToArray();
//                if (cfgs != null && cfgs.Length > 0)
//                {
//                    foreach (GrantServerConfig c in cfgs)
//                        UnRegister(c, true);
//                }
//            }
//        }

//        private static string configPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd("\\".ToCharArray()) + "\\Servers.txt";

//        private static void saveLocalConfig()
//        {
//            //只有一个appdomain并且是HAWorker不进行保存
//            if (apps.Values.Count == 1 &&
//                apps.Values.ToArray()[0].v1.ServerName.ToLower() == SuperGMS.RpcProxy.GrantHAWorkerRpcProxy.serviceName.ToLower())
//                return;
//            try
//            {
//                if (apps != null && apps.Count > 0)
//                {
//                    GrantServerConfig[] cfgs = apps.Values.Select(a => a.v1).ToArray();
//                    string cfgStr = JsonEx.JsonConvert.JsonSerializer(cfgs);
//                    using (FileStream stream = new FileStream(configPath, FileMode.Create, FileAccess.Write))
//                    {
//                        using (StreamWriter writer = new StreamWriter(stream))
//                        {
//                            writer.Write(cfgStr);
//                            writer.Flush();
//                            writer.Close();
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.LogEx.LogError(new Exception("GrantRpcServerManager.saveLocalConfig.Error", ex));
//            }
//        }

//    }
//}