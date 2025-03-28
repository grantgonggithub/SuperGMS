/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.MQ.RabbitMQ
 文件名：   ConnectionManager
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/5/14 15:33:07

 功能描述：

----------------------------------------------------------------*/
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

using SuperGMS.Log;
using SuperGMS.Tools;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SuperGMS.MQ.RabbitMQ
{
    /// <summary>
    /// ConnectionManager
    /// </summary>
    public class MQConnectionManager
    {
        private static Dictionary<string,List<ComboxClass<DateTime,MQConnection>>> dicConnections = new Dictionary<string, List<ComboxClass<DateTime, MQConnection>>>();
        private static object root = new object();
        private static bool isCheckRunning = false;
        private readonly static ILogger logger = LogFactory.CreateLogger<MQConnectionManager>();

        public static MQConnection GetMqConnection(string Ip, int port, string userName, string passWord)
        {
            string key = getKey(Ip, port, userName, passWord);
            lock (root)
            {
                if (!isCheckRunning)
                {
                    check();
                    isCheckRunning = true;
                }
                if (dicConnections.ContainsKey(key))
                {
                    var pools = dicConnections[key];
                    ComboxClass<DateTime, MQConnection> con =null;
                    if (pools!=null && pools.Count>1)
                    {
                        con = pools[0];
                        dicConnections[key].Remove(con); //如果有空闲连接，取出来一个，分配给外部，并从连接池移除掉
                        logger.LogInformation($"获取一个MQ已有连接,Ip:{Ip},Port:{port},poolsSize:{pools.Count}");
                        return con.V2;
                    }
                }
            }
            return getNewConnection(Ip, port, userName, passWord); // 没有就建个新的
        }

        private static MQConnection getNewConnection(string Ip,int port,string userName,string passWord)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = Ip;
            connectionFactory.Port = port;
            connectionFactory.UserName =userName;
            connectionFactory.Password = passWord;
            logger.LogInformation($"新建一个MQ连接,Ip:{Ip},Port:{port}");
            return new MQConnection(connectionFactory.CreateConnection(),getKey(Ip,port,userName,passWord));
        }

        private static string getKey(string Ip, int port, string userName, string passWord)
        {
            return $"{Ip}_{port}_{userName}_{passWord}";
        }

        /// <summary>
        /// 把用完的连接返回连接池
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="key"></param>
        public static void RelaceConnection(MQConnection connection, string key)
        {
            if (connection != null&& connection.Connection.IsOpen)
            {
                lock (root)
                {
                    if (dicConnections.ContainsKey(key))
                    {
                        var pools = dicConnections[key];
                        pools.Add(new ComboxClass<DateTime, MQConnection>(){ V1 = DateTime.Now, V2 = connection});
                    }
                    else
                    {
                        List<ComboxClass<DateTime,MQConnection>> li =new List<ComboxClass<DateTime, MQConnection>>();
                        dicConnections[key] = li;
                        li.Add(new ComboxClass<DateTime, MQConnection>() { V1 = DateTime.Now, V2 = connection });
                    }
                }
            }
            else
            {
                connection?.Close(); //不是open的连接就释放
            }
        }

        private static void check()
        {
            Thread thread = new Thread(new ThreadStart(checkTimeOut))
            {
                IsBackground = true,
                Name = "MQConnectionManager.check.thread"
            };
            thread.Start();
        }


        private static void checkTimeOut()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(60 * 1000); // 每1分钟检查一次
                    if (dicConnections.Count < 1)
                    {
                        continue;
                    }
                    string[] keys = dicConnections.Keys.ToArray();
                    Random r=new Random(DateTime.Now.Millisecond);
                    var k = keys[r.Next(0, keys.Length)];
                    var indxValue = dicConnections[k]; // 随机检查
                    var keyValuePairs =
                        indxValue.Where(x => DateTime.Now.Subtract(x.V1).TotalMilliseconds > 2 * 60 * 1000)?.ToArray(); // 找到2分钟没有被用的连接，释放掉
                    if (keyValuePairs == null || keyValuePairs.Length < 1)
                    {
                        continue;
                    }
                    lock (root)
                    {
                        for (int i = 0; i < keyValuePairs.Length; i++)
                        {
                            keyValuePairs[i].V2?.Close();
                            indxValue.Remove(keyValuePairs[i]);
                            keyValuePairs[i].V2 = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "checkTimeOut.Error");
                }

            }

        }
    }
}