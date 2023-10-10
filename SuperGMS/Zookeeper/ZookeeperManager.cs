/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Zookeeper
 文件名：  GrantZookeeper
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/2/23 14:35:41

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using org.apache.zookeeper.data;
using SuperGMS.Config;
using SuperGMS.Log;

namespace SuperGMS.Zookeeper
{
    /// <summary>
    /// GrantZookeeper
    /// </summary>
    public class ZookeeperManager
    {
        private static ZooKeeper currentZookeeper;
        private static string ConnectionString;
        private static int SessionTimeout;
        private static Watcher Watcher;
        private const string ZKROOT = "/ServerConfig";
        private const string QTCONFIG = "/config";
        private const string QTROUTER = "/router";
        private const string RELATION = "/relation";
        private readonly static ILogger logger = LogFactory.CreateLogger<ZookeeperManager>();
        private static object rootLock = new object();


        /// <summary>
        /// 初始化的一个zk，一个微服务只会连一个zk
        /// </summary>
        /// <param name="connectionString">connectionString</param>
        /// <param name="sessionTimeout">sessionTimeout</param>
        /// <param name="watcher">监控连接的watcher</param>
        public static void Initlize(string connectionString, int sessionTimeout, Watcher watcher)
        {
           // GrantLogTextWriter.Write($"开始初始化ZK连接:{connectionString}");
            try
            {
                //_connectionString = connectionString;
                //currentZookeeper = new ZooKeeper(connectionString, sessionTimeout, watcher);

                //var stats = currentZookeeper.getState();

                connection(connectionString, sessionTimeout, watcher);

                var existRoot = currentZookeeper.existsAsync(ZKROOT, false);
                existRoot.Wait();
                if (existRoot.Result == null)
                {
                    CreatePersistent(ZKROOT, ZKROOT); // 创建根节点
                }

                var exitConfig = currentZookeeper.existsAsync(ZKROOT + QTCONFIG, false);
                exitConfig.Wait();
                if (exitConfig.Result == null)
                {
                    CreatePersistent(ZKROOT + QTCONFIG, QTCONFIG); // 创建config节点
                }

                var exitRouter = currentZookeeper.existsAsync(ZKROOT + QTROUTER, false);
                exitRouter.Wait();
                if (exitRouter.Result == null)
                {
                    CreatePersistent(ZKROOT + QTROUTER, QTROUTER); // 创建router节点
                }

                var exitRelation = currentZookeeper.existsAsync(ZKROOT + RELATION, false); // 创建调用关系节点
                exitRelation.Wait();
                if (exitRelation.Result == null)
                {
                    CreatePersistent(ZKROOT+RELATION,RELATION);
                }

                logger.LogInformation($"ZK连接初始化完毕:{connectionString}");

                // 連接成功后，把連接參數記錄下來，便於後面重連
                ConnectionString = connectionString;
                SessionTimeout = sessionTimeout;
                Watcher = watcher;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, $"ZK连接:{connectionString}初始化异常");
                throw;
            }
        }

        public static void CheckConfig(string appName, Dictionary<string,string> configNames)
        {
            string path = getConfigPath(appName);
            int try_num = 0;
            gotoHere:
            try
            {
                var exitConfig = currentZookeeper.existsAsync(path, false);
                exitConfig.Wait();
                if (exitConfig.Result == null)
                {
                    CreatePersistent(path, string.Empty); // 创建config节点
                }
            }
            catch (Exception e)
            {
                try_num += 1;
                if (try_num < 3)
                {
                    reConnection(e);
                    Thread.Sleep(1000);
                    goto gotoHere;
                }

                logger.LogError(e, $"GrantZookeeperManager.CheckConfig.{path}初始化失败");
            }

            foreach (var name in configNames)
            {
                int tryNum = 0;
                string p = path + "/" + name.Key;
                gotoLable:
                try
                {
                    var exitConfig = currentZookeeper.existsAsync(p, false);
                    exitConfig.Wait();
                    if (exitConfig.Result == null)
                    {
                        CreatePersistent(p,name.Value); // 创建配置节点
                    }
                }
                catch (Exception e)
                {
                    tryNum += 1;
                    if (tryNum < 3)
                    {
                       reConnection(e);
                        Thread.Sleep(1000);
                        goto gotoLable;
                    }

                    logger.LogError(e, $"GrantZookeeperManager.CheckConfig.{p}初始化失败");
                }

            }
        }

        private static bool isConnectioning = false;

        private static void connection(string connectionString, int sessionTimeout, Watcher watcher,Action reConnectionAction = null)
        {
            if (currentZookeeper?.getState() != ZooKeeper.States.CONNECTED && !isConnectioning)
            {
                lock (rootLock)
                {
                    if (currentZookeeper?.getState() != ZooKeeper.States.CONNECTED && !isConnectioning)
                    {
                        isConnectioning = true;
                        gotoLable:
                        logger.LogInformation($"开始初始化ZK连接:{connectionString}");
                        try
                        {
                            ConnectionString = connectionString;
                            currentZookeeper = new ZooKeeper(connectionString, sessionTimeout, watcher);

                            var state = currentZookeeper.getState();
                            while (true)
                            {
                                if (state == ZooKeeper.States.CONNECTED)
                                {
                                    isConnectioning = false;
                                    reConnectionAction?.Invoke();
                                    logger.LogInformation($"ZK连接成功:{connectionString}");
                                    break;
                                }

                                Thread.Sleep(3000);
                                state = currentZookeeper.getState();
                            }
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(1000);
                            logger.LogError(ex, $"zk连接异常");
                            goto gotoLable;   // 一直尝试连接
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取子节点列表
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="watcher">监听的watcher</param>
        /// <returns>返回节点列表</returns>
        public static List<string> GetChildrenNode(string path, Watcher watcher = null)
        {
            logger.LogInformation($"GetChildrenNode 开始获取数据：{path}");
            int tryNum = 0;
            gotoLable:
            try
            {
                var result = watcher == null
                    ? currentZookeeper.getChildrenAsync(path, false)
                    : currentZookeeper.getChildrenAsync(path, watcher);
                result.Wait();
                logger.LogInformation($"GetChildrenNode 获取到的数据是：{path}----{string.Join(",", result?.Result?.Children?.ToArray())}");
                return result?.Result?.Children;
            }
            catch (KeeperException.NoNodeException)
            {
                logger.LogError("GrantZookeeperManager.GetChildrenNode.Error,NoNodeException");
            }
            catch (Exception e)
            {
                tryNum += 1;
                if (tryNum < 3)
                {
                    reConnection(e);
                    Thread.Sleep(1000);
                    goto gotoLable;
                }
                logger.LogError(e, $"GetChildrenNode 获取数据异常：{path}");
            }

            return null;
        }

        public static bool needReConnection(Exception e)
        {
            return
                //(e is KeeperException.SessionExpiredException)
                //|| (e is KeeperException.AuthFailedException) || (e is KeeperException.ConnectionLossException) || 

                currentZookeeper.getState() != ZooKeeper.States.CONNECTED && currentZookeeper.getState() != ZooKeeper.States.CONNECTEDREADONLY;
        }

        public static void reConnection(Exception e,Action reConnectionAction = null)
        {
            if (needReConnection(e))
            {
                Task.Run(() =>
                {
                    gotoLable:
                    try
                    {
                        Random r = new Random();
                        Thread.Sleep(r.Next(1000,6000)); // 重连一定要随机等待一下，错开时间，这样如果zk端开瞬间，不至于同时形成重连风暴
                        connection(ConnectionString, SessionTimeout, Watcher,reConnectionAction);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, $"zk重连异常");
                        goto  gotoLable;  // 一直重连
                    }
                });
            }
        }

        /// <summary>
        /// 删除节点，只有永久节点需要删除
        /// </summary>
        /// <param name="path">要删除的节点</param>
        public static void Delete(string path)
        {
            var rst = currentZookeeper.deleteAsync(path);
            rst.Wait();
            logger.LogWarning($"删除zk节点{path}");
        }

        /// <summary>
        /// 设置或者修改配置信息，一般指静态配置
        /// </summary>
        /// <param name="serverName">服务名</param>
        /// <param name="xmlConfig">注意数据是xml格式</param>
        public static void SetConfig(string serverName, string xmlConfig)
        {
            string path = getConfigPath(serverName);
            CreateOrUpdateNode(xmlConfig, path);
        }

        private static void CreateOrUpdateNode(string xmlConfig, string path)
        {
            var existPath = currentZookeeper.existsAsync(path, false);
            existPath.Wait();
            if (existPath.Result == null)
            {
                CreatePersistent(path, xmlConfig);
            }
            else
            {
                SetNodeData(path, xmlConfig);
            }
        }

        /// <summary>
        /// 获取微服务配置，根据微服务获取，这里可能会重建路径，注意区分GetWatcherBackConfig
        /// </summary>
        /// <param name="serverName">微服务serverName</param>
        /// <param name="watcher">回调方法watcher</param>
        /// <returns>配置内容</returns>
        public static string GetConfig(string serverName, Watcher watcher)
        {
            string p = getConfigPath(serverName);
            return GetNodeData(p, watcher);
        }

        public static List<string> GetConfigChildren(string serverName, Watcher watcher = null)
        {
            string p = getConfigPath(serverName);
            return GetChildrenNode(p, watcher);
        }

        public static List<string> GetRouterChildren(string serverName, Watcher watcher = null)
        {
            string p = getRouterPath(serverName);
            return GetChildrenNode(p, watcher);
        }

       // private const string routerData = "{ \"Pool\": 0,\"Ip\": \"{0}\",\"Port\": {1},\"ServerType\": 2,\"Enable\": {2}}";

        private static string getRouterData(string ip, int port, bool enable,int timeout)
        {
            // json内容中包含"{"和"}" string.format 中的占位符 也包含"{"和"}"所以异常了，只能用+拼字符串
            return "{ \"Pool\": 0,\"Ip\": \"" + ip + "\",\"Port\": " + port + ",\"ServerType\": 2,\"Enable\":\""+ enable + "\",\"TimeOut\":" + timeout + "}";
        }

        // "<Item Pool=\"0\" Ip=\"{0}\" Port=\"{1}\" ServerType=\"thrift\" Enable=\"{2}\"/>";

        /// <summary>
        /// 设置路由，一般都是服务启动之后会自动注册
        /// </summary>
        /// <param name="serverName">服务名称</param>
        /// <param name="data">数据</param>
        public static string SetRouter(string serverName, string ip, int port, bool enable,int timeout=0)
        {
            string path = getRouterPath(serverName);

            var existPath = currentZookeeper.existsAsync(path, false);
            existPath.Wait();
            if (existPath.Result == null)
            {
                CreatePersistent(path, serverName);
            }
            path = path + $"/[{ip}:{port}]";
            existPath = currentZookeeper.existsAsync(path);
            existPath.Wait();
            if (existPath.Result == null)
            {
                CreateEphemeral(path, getRouterData(ip, port, enable,timeout));
            }
            return path;
        }

        /// <summary>
        /// 设置调用关系
        /// </summary>
        /// <param name="relationServerName"></param>
        /// <param name="myServerName"></param>
        /// <returns></returns>
        public static string SetRelation(string relationServerName, string myServerName)
        {
            string path = getRelationPath(relationServerName);
            var existPath = currentZookeeper.existsAsync(path, false);
            existPath.Wait();
            if (existPath.Result == null)
            {
                CreatePersistent(path, relationServerName);
            }
            path = path + $"/{myServerName}";
            existPath = currentZookeeper.existsAsync(path);
            existPath.Wait();
            if (existPath.Result == null)
            {
                CreatePersistent(path,myServerName);
            }
            return path;
        }

        /// <summary>
        /// 清除路由，清理连接
        /// </summary>
        /// <param name="serverName">serverName</param>
        /// <param name="router">router</param>
        public static void ClearRouter(string serverName, string router)
        {
            string path = getRouterPath(serverName) + $"/[{router}]";
            logger.LogInformation($"服务退出，清理路由:{path}");
            try
            {
                var existPath = currentZookeeper.existsAsync(path, false);
                existPath.Wait();
                if (existPath.Result != null)
                {
                    Delete(path);
                }

                if (currentZookeeper != null)
                {
                    var rst = currentZookeeper.closeAsync();
                    rst.Wait();
                    currentZookeeper = null;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "ClearRouter.Error");
            }

        }

        /// <summary>
        /// 获取path的数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="watcher">监听path节点的watcher</param>
        /// <returns>返回数据</returns>
        public static string GetNodeData(string path, Watcher watcher = null)
        {
            int tryNum = 0;
            gotoLable:
            logger.LogInformation($"GetNodeData获取数据:path={path},watcher={watcher?.GetType()?.FullName}");
            try
            {
                var result = watcher == null
                    ? currentZookeeper.getDataAsync(path, true)
                    : currentZookeeper.getDataAsync(path, watcher);
                result.Wait();
                byte[] rst = result.Result.Data;
                if (rst == null || rst.Length < 1)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(rst);
            }
            catch (KeeperException.NoNodeException nodeEx) // 这样的异常不用重连
            {
                logger.LogError(nodeEx, $"GetNodeData 获取数据节点不存在 NoNodeException:{path}");
                return null;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"GetNodeData获取数据异常:path={path},watcher={watcher?.GetType()?.FullName}");
                tryNum += 1;
                if (tryNum < 3)
                {
                    reConnection(e);
                    Thread.Sleep(1000);
                    goto gotoLable;
                }
            }

            return null;
        }

        /// <summary>
        /// 设置节点数据
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="data">data</param>
        /// <returns>返回是否成功</returns>
        private static bool SetNodeData(string path, string data)
        {
            int tryNum = 0;
            gotoLable:
            logger.LogInformation($"SetNodeData开始设置:{path}_{data}");
            try
            {
                var rst = currentZookeeper.setDataAsync(path, Encoding.UTF8.GetBytes(data));
                rst.Wait();
                return true;
            }
            catch (KeeperException.NoNodeException nodeEx) // 这样的异常不用重连
            {
                logger.LogError(nodeEx, $"SetNodeData 设置数据异常节点不存在 NoNodeException:{path}{data}");
                return false;
            }
            catch (KeeperException.BadVersionException badVersion)
            {
                logger.LogError(badVersion, $"SetNodeData 设置数据异常节点不存在 BadVersionException:{path}{data}");
                return false;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"SetNodeData 设置数据异常:{path}{data}");
                tryNum += 1;
                if (tryNum < 3)
                {
                    reConnection(e);
                    Thread.Sleep(1000);
                    goto gotoLable;
                }
                else
                {
                    throw; // 设置数据的地方一定要抛异常 
                }
            }
        }

        /// <summary>
        /// 创建永久性节点
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <param name="data">节点数据</param>
        private static void CreatePersistent(string path, string data)
        {
            int tryNum = 0;
            gotoLable:
            try
            {
                var rst = currentZookeeper.createAsync(path, Encoding.UTF8.GetBytes(data), ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                rst.Wait();
                logger.LogInformation($"增加zk永久节点{path},数据是{data}");
            }
            catch (KeeperException.NoNodeException noNode)
            {
                logger.LogError(noNode, $"增加zk永久节点{path}异常,节点不存在,数据是{data}");
                return;
            }
            catch (KeeperException.NoChildrenForEphemeralsException noChildren)
            {
                logger.LogError(noChildren, $"增加zk永久节点{path}异常,子节点不存在,数据是{data}");
                return;
            }
            catch (KeeperException.NodeExistsException nodeExists)
            {
                logger.LogError(nodeExists, $"增加zk永久节点{path}异常,节点已经存在,数据是{data}");
                return;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error,CreatePersistent增加zk永久节点{path},数据是{data}失败");
                tryNum += 1;
                if (tryNum < 3)
                {
                    reConnection(e);
                    Thread.Sleep(1000);
                    goto gotoLable;
                }
                else
                {
                    throw; // 设置数据的地方一定要抛异常
                }
            }
        }


        public static string getConfigPath(string serverName)
        {
            return string.Format("{0}{1}/{2}", ZKROOT, QTCONFIG, serverName);
        }

        public static string getRouterPath(string serverName)
        {
            return string.Format("{0}{1}/{2}", ZKROOT, QTROUTER, serverName);
        }

        public static string getRelationPath(string relationRerverName)
        {
            return string.Format("{0}{1}/{2}", ZKROOT, RELATION, relationRerverName);
        }

        /// <summary>
        /// 创建一个临时节点，客户端掉线，节点将被删除
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="data">data</param>
        private static void CreateEphemeral(string path, string data)
        {
            int tryNum = 0;
            gotoLable:
            try
            {
                var rst = currentZookeeper.createAsync(path, Encoding.UTF8.GetBytes(data), ZooDefs.Ids.OPEN_ACL_UNSAFE,
                    CreateMode.EPHEMERAL);
                rst.Wait();
                logger.LogInformation($"增加zk临时节点{path},数据是{data}");
            }
            catch (KeeperException.NoNodeException noNode)
            {
                logger.LogError(noNode, $"增加zk临时节点{path}异常,节点不存在,数据是{data}");
                return;
            }
            catch (KeeperException.NoChildrenForEphemeralsException noChildren)
            {
                logger.LogError(noChildren, $"增加zk临时节点{path}异常,子节点不存在,数据是{data}");
                return;
            }
            catch (KeeperException.NodeExistsException nodeExists)
            {
                logger.LogError(nodeExists, $"增加zk临时节点{path}异常,节点已经存在,数据是{data}");
                return;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error,CreateEphemeral增加zk临时节点{path},数据是{data} 异常");
                tryNum += 1;
                if (tryNum < 3)
                {
                    reConnection(e);
                    Thread.Sleep(1000);
                    goto gotoLable;
                }
                else
                {
                    throw; // 设置数据的地方一定要抛异常 
                }
            }
        }
    }
}