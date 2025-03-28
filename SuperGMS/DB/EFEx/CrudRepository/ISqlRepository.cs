/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx.CrudRepository
 文件名：  ISqlRepository
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/31 16:10:50

 功能描述：

----------------------------------------------------------------*/

using Dapper;

using SuperGMS.DB.EFEx.DynamicSearch;

using System.Collections.Generic;
using System.Data;

namespace SuperGMS.DB.EFEx.CrudRepository
{
    /// <summary>
    /// ISqlRepository
    /// </summary>
    public interface ISqlRepository:ISqlBaseRepository
    {
        /// <summary>
        /// 根据Sql查询返回DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        DataSet QueryDataSetBySqlKey(string sql, CommandType commandType = CommandType.Text);
        /// <summary>
        /// Query
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="searchParameters">查询条件</param>
        /// <returns>list</returns>
        List<T> QueryByPageSqlKey<T>(string sqlKey, SearchParameters searchParameters);

        /// <summary>
        /// Query
        /// </summary>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="searchParameters">查询条件</param>
        /// <returns>list</returns>
        List<dynamic> QueryByPageSqlKey(string sqlKey, SearchParameters searchParameters);

        /// <summary>
        /// Query
        /// </summary>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType"></param>
        /// <returns>list</returns>
        List<dynamic> QuerySqlKey(string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text);
        /// <summary>
        /// Query
        /// </summary>
        /// <param name="trans">trans</param>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType"></param>
        /// <returns>list</returns>
        List<dynamic> QuerySqlKey(IDbTransaction trans, string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text);
        /// <summary>
        /// Query
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType"></param>
        /// <returns>list</returns>
        List<T> QuerySqlKey<T>(string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text);
        /// <summary>
        /// Query
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="trans">trans</param>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType"></param>
        /// <returns>list</returns>
        List<T> QuerySqlKey<T>(IDbTransaction trans, string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text);

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType"></param>
        /// <returns>TValue</returns>
        T ExecuteScalarSqlKey<T>(string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text);
        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="trans">trans</param>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType"></param>
        /// <returns>TValue</returns>
        T ExecuteScalarSqlKey<T>(IDbTransaction trans, string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text);

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType"></param>
        /// <returns>dynamic</returns>
        dynamic ExecuteScalarSqlKey(string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text);
        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <param name="trans">trans</param>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType"></param>
        /// <returns>dynamic</returns>
        dynamic ExecuteScalarSqlKey(IDbTransaction trans, string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text);

        /// <summary>
        /// ExecuteNoQuery
        /// </summary>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType">commandType</param>
        /// <returns>int</returns>
        int ExecuteNoQuerySqlKey(string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType);
        /// <summary>
        /// ExecuteNoQuery
        /// </summary>
        /// <param name="trans">trans</param>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType">commandType</param>
        /// <returns>int</returns>
        int ExecuteNoQuerySqlKey(IDbTransaction trans, string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType);
        /// <summary>
        /// dapper底层函数
        /// </summary>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="param">params</param>
        /// <param name="commandType">commandType</param>
        /// <returns>int</returns>
        int ExecuteNoQuerySqlKey(string sqlKey, DynamicParameters param, CommandType commandType);
        /// <summary>
        /// dapper底层函数
        /// </summary>
        /// <param name="trans">trans</param>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="param">params</param>
        /// <param name="commandType">commandType</param>
        /// <returns>int</returns>
        int ExecuteNoQuerySqlKey(IDbTransaction trans, string sqlKey, DynamicParameters param, CommandType commandType);
        /// <summary>
        /// 获取配置的sql
        /// </summary>
        /// <param name="sqlKey"></param>
        /// <returns></returns>
        string GetSqlByKey(string sqlKey);

        /// <summary>
        /// 根据QueryModel组织sqlWhere语句,如果有字段前缀的话,需要提前增加进来
        /// </summary>
        /// <param name="searchParameters"></param>
        /// <param name="dbType"></param>
        /// <returns>where语句</returns>
        string ConvertToSqlWhere(SearchParameters searchParameters, DbType dbType);

    }
}