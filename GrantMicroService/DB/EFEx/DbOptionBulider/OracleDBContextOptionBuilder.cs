/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.DB.EFEx.DbOptionBulider
 文件名：  OracleDBContextOptionBuilder
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/2 21:03:01

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace GrantMicroService.DB.EFEx
{
    /// <summary>
    /// OracleDBContextOptionBuilder
    /// </summary>
    public class OracleDBContextOptionBuilder : IContextOptionBuilderFactory
    {
        public DbContextOptionsBuilder<T> CreateOptionsBuilder<T>(string connectionString) where T : DbContext
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetConnectionString
        /// </summary>
        /// <param name="dbInfo"></param>
        /// <returns></returns>
        public string GetConnectionString(DbInfo dbInfo)
        {
            return OracleDBContextOptionBuilder.GetDbConnectionString(dbInfo);
        }

        /// <summary>
        /// 根据数据类型获取连接字符串
        /// </summary>
        /// <param name="dbInfo">dbInfo</param>
        /// <returns>string</returns>
        public static string GetDbConnectionString(DbInfo dbInfo)
        {
            //return $"DATA SOURCE ={dbInfo.Ip}:{dbInfo.Port}/{dbInfo.DbName}; PASSWORD = {dbInfo.Pwd}; PERSIST SECURITY INFO = True; USER ID = {dbInfo.UserName}";
           return $"Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = {dbInfo.Ip})(PORT = {(dbInfo.Port>0?dbInfo.Port:1521)}))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = {dbInfo.DbName}))); User ID = {dbInfo.UserName}; Password = {dbInfo.Pwd}; Persist Security Info = True";
        }
    }
}
