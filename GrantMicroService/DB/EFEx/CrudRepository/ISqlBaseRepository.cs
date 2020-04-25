/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： GrantMicroService.DB.EFEx.CrudRepository
 文件名：   ISqlBaseRepository
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/5/15 16:13:21

 功能描述：

----------------------------------------------------------------*/
using GrantMicroService.DB.EFEx.DynamicSearch;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace GrantMicroService.DB.EFEx.CrudRepository
{
    /// <summary>
    /// ISqlBaseRepository
    /// </summary>
    public interface ISqlBaseRepository
    {
        /// <summary>
        /// 根据Sql查询返回DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        DataSet QueryDataSetBySql(string sql);

        /// <summary>
        /// Query
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="searchParameters">查询条件</param>
        /// <returns>list</returns>
        List<dynamic> QueryByPageSql(string sql, SearchParameters searchParameters);
        /// <summary>
        /// Query
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="sql">sql</param>
        /// <param name="searchParameters">查询条件</param>
        /// <returns>list</returns>
        List<T> QueryByPageSql<T>(string sql, SearchParameters searchParameters);

        /// <summary>
        /// Parametric Query ,执行参数化SQL分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="pageInfo"></param>
        /// <param name="paramsDic"></param>
        /// <returns></returns>
        List<T> QueryPageByParametricSql<T>(string sql, PageInfo pageInfo, Dictionary<string, object> paramsDic);

        /// <summary>
        /// Query
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="sql">sql</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType">commandType</param>
        /// <returns>list</returns>
        List<T> QuerySql<T>(string sql, string[] paramsList, object[] valuesList, CommandType commandType);
        /// <summary>
        /// Query
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="trans">trans</param>
        /// <param name="sql">sql</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType">commandType</param>
        /// <returns>list</returns>
        List<T> QuerySql<T>(IDbTransaction trans, string sql, string[] paramsList, object[] valuesList, CommandType commandType);

        /// <summary>
        /// Query
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <returns>list</returns>
        List<dynamic> QuerySql(string sql, string[] paramsList, object[] valuesList);
        /// <summary>
        /// Query
        /// </summary>
        /// <param name="trans">trans</param>
        /// <param name="sql">sql</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <returns>list</returns>
        List<dynamic> QuerySql(IDbTransaction trans, string sql, string[] paramsList, object[] valuesList);

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="sql">sql</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <returns>TValue</returns>
        T ExecuteScalarSql<T>(string sql, string[] paramsList, object[] valuesList);

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="trans">trans</param>
        /// <param name="sql">sql</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <returns>Tvalue</returns>
        T ExecuteScalarSql<T>(IDbTransaction trans, string sql, string[] paramsList, object[] valuesList);

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <returns>dynamic</returns>
        dynamic ExecuteScalarSql(string sql, string[] paramsList, object[] valuesList);

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <param name="trans">trans</param>
        /// <param name="sql">sql</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <returns>dynamic</returns>
        dynamic ExecuteScalarSql(IDbTransaction trans, string sql, string[] paramsList, object[] valuesList);

        /// <summary>
        /// ExecuteNoQuery
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType">commandType</param>
        /// <returns>int</returns>
        int ExecuteNoQuerySql(string sql, string[] paramsList, object[] valuesList, CommandType commandType);
        /// <summary>
        /// ExecuteNoQuery
        /// </summary>
        /// <param name="trans">trans</param>
        /// <param name="sql">sql</param>
        /// <param name="paramsList">paramsList</param>
        /// <param name="valuesList">valuesList</param>
        /// <param name="commandType">commandType</param>
        /// <returns>int</returns>
        int ExecuteNoQuerySql(IDbTransaction trans, string sql, string[] paramsList, object[] valuesList, CommandType commandType);
        /// <summary>
        /// dapper底层函数
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="param">params</param>
        /// <param name="commandType">commandType</param>
        /// <returns>int</returns>
        int ExecuteNoQuerySql(string sql, object param, CommandType commandType);
        /// <summary>
        /// dapper底层函数
        /// </summary>
        /// <param name="trans">trans</param>
        /// <param name="sql">sql</param>
        /// <param name="param">params</param>
        /// <param name="commandType">commandType</param>
        /// <returns>int</returns>
        int ExecuteNoQuerySql(IDbTransaction trans, string sql, object param, CommandType commandType);
        /// <summary>
        /// Get Transaction
        /// </summary>
        /// <returns></returns>
        IDbTransaction GetTransaction();
        /// <summary>
        /// Disponse Transaction
        /// </summary>
        /// <param name="trans"></param>
        void DisposeTransaction(System.Data.IDbTransaction trans);
    }
}