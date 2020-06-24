/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.DB.EFEx.GrantDbFactory
 文件名：   GrantDBConnection
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/8/28 18:16:22

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace SuperGMS.DB.EFEx.GrantDbFactory
{
    /// <summary>
    /// GrantDBConnection
    /// </summary>
    public class DBConnection:IDisposable
    {
        private DbConnection _connection;
        public DBConnection(DbConnection connection)
        {
            this._connection = connection;
        }
       public DbConnection Connection { get { return _connection; } }

        /// <summary>
        /// 后面改成连接池的释放
        /// </summary>
        public void Dispose()
        {
            if (_connection != null)
            {
                if(_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
                _connection.Dispose();
            }
        }
    }
}