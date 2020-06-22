/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Redis
 文件名：RedisConfig
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/3/24 14:35:06

 功能描述：redis服务器的配置的初始化和管理

----------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading;
using SuperGMS.Log;
using Microsoft.Extensions.Logging;

namespace SuperGMS.Redis
{
    public class RedisConfig
    {
        // Dictionary<nodeName, RedisNodeConfig> 每个节点的配置
        private static readonly Dictionary<string, RedisNode> redisNodes = new Dictionary<string, RedisNode>(0);

        private static readonly ReaderWriterLock readerWriterLock = new ReaderWriterLock();

        private readonly static ILogger logger = LogFactory.CreateLogger<RedisConfig>();
        // private static readonly string localConfig = "<RedisConfig><Node NodeName=\"default\" IsMasterSlave=\"false\"><Item pool = \"0\" IsMaster=\"false\" server=\"{0}\" port=\"{1}\" allowadmin=\"true\" connectTimeout=\"0\" ssl=\"false\" pwd=\"\"  /></Node></RedisConfig>";

        /// <summary>
        /// 初始化服务器配置
        /// </summary>
        /// <returns>服务器配置列表</returns>
        // public static Dictionary<string,RedisNode> InitlizeServerList(XElement root)
        // {
        //    if (root != null && root.Element("RedisConfig") != null &&
        //        root.Element("RedisConfig").Elements("Node") != null)
        //    {
        //        IEnumerable nodes =
        //            from em in root.Element("RedisConfig").Elements("Node")
        //            select em;
        //        foreach (XElement it in nodes)
        //        {
        //            if (it.Elements("Item") == null)
        //            {
        //                continue;
        //            }
        //            RedisNode node = new RedisNode()
        //            {
        //                NodeName = it.Attribute("NodeName").Value.ToLower(),
        //                IsMasterSlave = it.Attribute("IsMasterSlave").Value == "true" ? true : false,
        //            };
        //            if (redisNodes.ContainsKey(node.NodeName))
        //            {
        //                Log.GrantLogTextWriter.Write(new Exception(string.Format("不能配置业务节点同名的redis服务器,{0}节点的配置将被忽略", node.NodeName)));
        //                continue;
        //            }
        //            redisNodes[node.NodeName] = node;
        //            IEnumerable srvs = from sr in it.Elements("Item")
        //                               select sr;
        //            var servers = new List<RedisServer>();
        //            foreach (XElement s in srvs)
        //            {
        //                var srv = new RedisServer
        //                {
        //                    Node = node,
        //                    IsMaster = s.Attribute("IsMaster").Value == "true" ? true : false,
        //                    Pool = int.Parse(s.Attribute("pool").Value),
        //                    ConnectTimeout = int.Parse(s.Attribute("connectTimeout").Value),
        //                    Server = s.Attribute("server").Value,
        //                    Port = int.Parse(s.Attribute("port").Value),
        //                    AllowAdmin = s.Attribute("allowadmin").Value == "true" ? true : false,
        //                    Ssl = s.Attribute("ssl").Value == "true" ? true : false,
        //                    Pwd = s.Attribute("pwd").Value,
        //                };
        //                if (node.IsMasterSlave && srv.IsMaster)
        //                {
        //                    RedisConnectionManager.GetConnection(srv); // 提前预初始化
        //                    node.MasterServer = srv; // 如果配置了多个主，这里只会把最后一个做为主
        //                }
        //                else
        //                {
        //                    servers.Add(srv); // 如果是非主从模式，将所有的都加到从上，没有主次
        //                    RedisConnectionManager.GetConnection(srv);
        //                }
        //            }
        //            node.SlaveServers = servers.ToArray();
        //        }
        //        return redisNodes;
        //    }
        //    throw new Exception("无法解析RedisConfig配置");
        //    // else
        //    //    Log.LogEx.LogError("redis clinet config is null");
        // }
        public static Dictionary<string, RedisNode> InitlizeServerList(Config.RedisConfig config)
        {
            if (config != null && config.Nodes != null && config.Nodes.Count > 0)
            {
                foreach (Config.RedisNode it in config.Nodes)
                {
                    RedisNode node = new RedisNode()
                    {
                        NodeName = it.NodeName.ToLower(),
                        IsMasterSlave = it.IsMasterSlave,
                    };

                    // 这里的初始化方法需要满足 重复调用. 所以注释掉这句, 覆盖原来的配置
                    //if (redisNodes.ContainsKey(node.NodeName))
                    //{
                    //    Log.GrantLogTextWriter.Write(new Exception(string.Format("不能配置业务节点同名的redis服务器,{0}节点的配置将被忽略", node.NodeName)));
                    //    continue;
                    //}
                    redisNodes[node.NodeName] = node;
                    var servers = new List<RedisServer>();
                    foreach (Config.RedisItem sItem in it.Items)
                    {
                        var srv = new RedisServer
                        {
                            Node = node,
                            IsMaster = sItem.IsMaster,
                            Pool = sItem.Pool,
                            ConnectTimeout = sItem.ConnectTimeout,
                            Server = sItem.Server,
                            Port = sItem.Port,
                            AllowAdmin = sItem.AllowAdmin,
                            Ssl = sItem.Ssl,
                            Ssl2=sItem.Ssl2,
                            Pwd = sItem.Pwd,
                            DbIndex=sItem.DbIndex,
                        };
                        if (node.IsMasterSlave && srv.IsMaster)
                        {
                            RedisConnectionManager.GetConnection(srv); // 提前预初始化
                            node.MasterServer = srv; // 如果配置了多个主，这里只会把最后一个做为主
                        }
                        else
                        {
                            servers.Add(srv); // 如果是非主从模式，将所有的都加到从上，没有主次
                            RedisConnectionManager.GetConnection(srv);
                        }
                    }

                    node.SlaveServers = servers.ToArray();
                }

                return redisNodes;
            }

            throw new Exception("无法解析RedisConfig配置");
        }

        /// <summary>
        /// 初始化redis集群的节点
        /// </summary>
        public static void Initlize(Config.RedisConfig config)
        {
            try
            {
                readerWriterLock.AcquireWriterLock(800);
                InitlizeServerList(config);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SuperGMS.Redis.redisConfig.Initlize error");
                throw ex;
            }
            finally
            {
                if (readerWriterLock.IsWriterLockHeld)
                {
                    readerWriterLock.ReleaseWriterLock();
                }
            }
        }

        /// <summary>
        /// 通过业务节点名称获取节点配置
        /// </summary>
        /// <param name="nodeName">nodeName</param>
        /// <returns>RedisNode</returns>
        public static RedisNode GetNode(string nodeName)
        {
            try
            {
                nodeName = nodeName.ToLower();
                readerWriterLock.AcquireReaderLock(80);
                if (redisNodes.ContainsKey(nodeName))
                {
                    return redisNodes[nodeName];
                }

                logger.LogCritical($"SuperGMS.Redis.redisConfig.GetNode nodeName={nodeName} is not exist");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"SuperGMS.Redis.redisConfig.GetNode error nodeName={nodeName}");
            }
            finally
            {
                if (readerWriterLock.IsReaderLockHeld)
                {
                    readerWriterLock.ReleaseReaderLock();
                }
            }

            return null;
        }
    }
}