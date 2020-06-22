/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.MQ.RabbitMQ
 文件名：   MQConnection
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/5/14 15:37:23

 功能描述：

----------------------------------------------------------------*/
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.MQ.RabbitMQ
{
    /// <summary>
    /// MQConnection
    /// </summary>
    public class MQConnection : IDisposable
    {
        public MQConnection(IConnection connection,string key)
        {
            _connection = connection;
            _key = key;
        }

        private string _key;

        public string Key {
            get { return _key; }
        }

        private IConnection _connection;

        public IConnection Connection {
            get { return _connection; }
        }

        public void Close()
        {
            try
            {
                _connection?.Close();
                _connection?.Dispose();
            }
            catch
            {
                //关闭、释放MQ连接异常，暂不做处理
            }
        }

        public void Dispose()
        {
            MQConnectionManager.RelaceConnection(this,_key);
        }
    }
}