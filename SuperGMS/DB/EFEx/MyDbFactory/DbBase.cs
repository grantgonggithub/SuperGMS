/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx.GrantDbFactory
 文件名：  DbBase
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/31 21:30:19

 功能描述：

----------------------------------------------------------------*/

using Dapper;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using SuperGMS.Config;
using SuperGMS.DB.EFEx.CrudRepository;
using SuperGMS.DB.EFEx.DynamicSearch;
using SuperGMS.DB.EFEx.DynamicSearch.Model;
using SuperGMS.ExceptionEx;
using SuperGMS.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SuperGMS.DB.EFEx.GrantDbFactory
{
    /// <summary>
    /// DbBase
    /// </summary>
    public abstract class DbBase : ISqlRepository
    {
        /// <summary>
        /// 在提交完成之后回调事件
        /// </summary>
        //public event DbDapperCommit OnDbCommit;
        /// <summary>
        /// 参数前缀 如：@，#
        /// </summary>
        protected abstract string Prefix { get; }
        protected readonly static ILogger logger = LogFactory.CreateLogger(typeof(DbBase).FullName);
        private DbInfo _dbInfo;
        
        protected DbInfo DbInfo
        {
            get { return _dbInfo; }
        }

        public DbBase(DbInfo dbInfo)
        {
            _dbInfo = dbInfo;
            if (_dbInfo.CommandTimeout < 1) _dbInfo.CommandTimeout = 10 * 60 * 1000; // 默认10分钟 ，这是OMS统计W库存时,时间过长，nick要求修改的  2019-2-19 by grant
        }

        public IDbTransaction GetTransaction()
        {
            return GetConnection().Connection.BeginTransaction();
        }

        /// <summary>
        /// 释放事务
        /// </summary>
        /// <param name="trans"></param>
        public void DisposeTransaction(IDbTransaction trans)
        {
            if(trans.Connection != null && trans.Connection.State == ConnectionState.Open)
            {
                trans.Connection.Close();
            }          
            trans.Dispose();
        }

        protected DBConnection GetConnection()
        {
            return new DBConnection(getDbConnection());
        }

        /// <summary>
        /// 根据不同的数据库生成对应得连接对象
        /// </summary>
        /// <returns></returns>
        protected DbConnection getDbConnection()
        {
            DbConnection conn = null;
            switch (_dbInfo.DbType)
            {
                case DbType.MySql:
                    conn = new MySqlConnection(MySqlDBContextOptionBuilder.GetDbConnectionString(DbInfo));
                    break;
                case DbType.Oracle:
                    conn = new OracleConnection(OracleDBContextOptionBuilder.GetDbConnectionString(DbInfo));
                    break;
                case DbType.SqlServer:
                    conn = new SqlConnection(SqlServerDBContextOptionBuilder.GetDbConnectionString(DbInfo));
                    break;
                case DbType.PostgreSQL:
                    conn = new NpgsqlConnection(PostgresqlDBContextOptionBuilder.GetDbConnectionString(DbInfo));
                    break;
            }
            if (conn == null)
                throw new Exception($"配置的数据库类型错误:DbType={_dbInfo.DbType}, Info={_dbInfo.ToString()}");
            conn.Open();
            return conn;
        }

        protected DynamicParameters PrepareCommand(string[] paramsList, object[] valuesList)
        {
            DynamicParameters parameters = new DynamicParameters();
            if (paramsList != null && paramsList.Length > 0 && valuesList != null && valuesList.Length > 0)
            {
                if (paramsList.Length < valuesList.Length)
                {
                    throw new Exception("paramsList.length<values.length error");
                }

                for (int i = 0; i < paramsList.Length; i++)
                {
                    // 把前缀补上
                    // string key = paramsList[i].Trim().StartsWith(Prefix) ? paramsList[i].Trim() : Prefix + paramsList[i].Trim();
                    string key = Prefix + paramsList[i].Trim(); // 不能有上面的判断，要不会埋坑，顶层的参数不能携带特定数据库的前缀
                    parameters.Add(key, valuesList[i]);
                }
            }

            return parameters;
        }

        /// <summary>
        /// 根据QueryModel组织sqlWhere语句,如果有字段前缀的话,需要提前增加进来
        /// 如果是Dapper 需要转换成
        /// </summary>
        /// <returns>Where 语句</returns>
        public string ConvertToSqlWhere(SearchParameters searchParameters, DbType dbType)
        {
            if (searchParameters == null) return "";

            //处理sql查询时，查询null值问题
            searchParameters.BuildEmptySearch();
            // 复制一个,避免修改的时候影响外部数据
            var copyCondition = JsonEx.JsonConvert.CopyObject(searchParameters.QueryModel.Items);
            copyCondition.Sort((a, b) =>
            {
                return a.Field.CompareTo(b.Field);
            });
            var sb = new StringBuilder();
            List<string> groups = new List<string>();

            // 如果是Dapper 则把Condition 的 Field 和 Value 替换掉

            var mapDic = DbColumnMaps.GetDbContextFiledMaps();
            for (int i = 0; i < copyCondition.Count; i++)
            {
                var item = copyCondition[i];
                if (dbType == DbType.Oracle)
                {
                    // Oracle 字段需要前后双引号
                    item.Value = "\"" + Prefix + item.Field + i + "\"";
                }
                else if (item.Value != null)
                {
                    item.Value = Prefix + item.Field + i;
                }

                if (mapDic.ContainsKey(item.Field))
                {
                    item.Field = mapDic[item.Field];
                }
            }

            foreach (var conditionItem in copyCondition)
            {
                if (!string.IsNullOrEmpty(conditionItem.OrGroup))
                {
                    if (!groups.Contains(conditionItem.OrGroup))
                    {
                        var sbChild = new StringBuilder();
                        foreach (var senItem in copyCondition)
                        {
                            if (senItem.OrGroup == conditionItem.OrGroup)
                            {
                                if (sbChild.Length > 0)
                                {
                                    sbChild.Append(" or ");
                                }
                                sbChild.Append(GetQueryCloumn(senItem) + " " + ConvertMethodToSql(senItem.Method, senItem.Value));
                            }
                        }
                        if (sb.Length > 0)
                            sb.Append(" and ");
                        sb.Append("(" + sbChild.ToString() + ")");
                        groups.Add(conditionItem.OrGroup);
                    }
                }
                else
                {
                    if (sb.Length > 0)
                        sb.Append(" and ");
                    sb.Append((string.IsNullOrEmpty(conditionItem.Prefix) ? "" : (conditionItem.Prefix + ".")) + conditionItem.Field + " " + ConvertMethodToSql(conditionItem.Method, conditionItem.Value));
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 根据SearchParamerters 的 FiledName 转换成数据库的 ColumnName
        /// </summary>
        /// <param name="searchParameters">查询参数</param>
        /// <returns>Dapper动态参数</returns>
        private DynamicParameters GetDynamicParametersBySearchParameter(SearchParameters searchParameters)
        {
            var paras = searchParameters.GetParameters();
            var keyList = new List<string>();
            var valueList = new List<object>();
            foreach(var p in paras)
            {
                if (p.Value is Array)
                {
                    var tmp = p.Value as Array;
                    if (tmp == null || tmp.Length < 1)
                        throw new BusinessException("ConditionItem的Value字段类型为数组时长度不能小于1");
                }
                keyList.Add(p.Key);
                valueList.Add(p.Value);
            }
            return PrepareCommand(keyList.ToArray(), valueList.ToArray());
        }
        /// <summary>
        /// 转换列
        /// </summary>
        /// <param name="senItem">查询条件</param>
        /// <returns>查询列名</returns>
        protected virtual string GetQueryCloumn(ConditionItem senItem)
        {
            return (string.IsNullOrEmpty(senItem.Prefix) ? "" : (senItem.Prefix + ".")) + senItem.Field;
        }
        /// <summary>
        /// 将Method转换成Dapper语法的查询语句
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="value">值</param>
        /// <returns>条件</returns>
        protected virtual string ConvertMethodToSql(QueryMethod method, object value)
        {
            switch (method)
            {
                /*
                  *   sqlText += " and Name like @Name";
                        p.Add("Name","%"+ model.Name+"%");
                  */
                case QueryMethod.Contains:
                case QueryMethod.StartsWith:
                case QueryMethod.EndsWith:
                    return "like " + value;
                /*
                 * string sql = "SELECT * FROM SomeTable WHERE id IN @ids"
                    var results = conn.Query(sql, new { ids = new[] { 1, 2, 3, 4, 5 }});
                 */
                case QueryMethod.StdIn:
                    return "in " + value;

                case QueryMethod.StdNotIn:
                    return "not in " + value;

                case QueryMethod.NotLike:
                    return "not like " + value;
                ////数字类型处理
                case QueryMethod.GreaterThan:
                    return "> " + value;

                case QueryMethod.GreaterThanOrEqual:
                    return ">= " + value;

                case QueryMethod.Equal:
                    if (value == null)
                    {
                        return " is null ";
                    }
                    return "= " + value;

                case QueryMethod.LessThan:
                    return "< " + value;

                case QueryMethod.LessThanOrEqual:
                    return "<= " + value;

                case QueryMethod.NotEqual:
                    if (value == null)
                    {
                        return " is not null ";
                    }
                    return "<> " + value;

                default:
                    return "";
            }
        }
        /// <summary>
        /// 获取一个分页Sql
        /// </summary>
        /// <param name="searchParameters">查询条件</param>
        /// <param name="sql">查询主sql</param>
        /// <returns>分页sql</returns>
        private string GetPageSql(SearchParameters searchParameters, string sql)
        {
            var pageInfo = searchParameters.PageInfo;
            pageInfo.CurrentPage = pageInfo.CurrentPage < 1 ? 1 : pageInfo.CurrentPage;
            var skipCount = pageInfo.SkipCount > 0 ? pageInfo.SkipCount : ((pageInfo.CurrentPage - 1) * pageInfo.PageSize);

            if (skipCount >= 0 && pageInfo.PageSize > 0)
            {
                //通过总条数检查当前页是有效(有数据), 如果没数据,则自动将页码设置为最后一页
                if (pageInfo.TotalCount > 0 && skipCount >= pageInfo.TotalCount)
                {
                    //获取总页数
                    var totalPage = pageInfo.TotalCount / pageInfo.PageSize + (pageInfo.TotalCount % pageInfo.PageSize > 0 ? 1 : 0);
                    //设置查询最后一页数据
                    skipCount = (totalPage - 1) * pageInfo.PageSize;
                }
                sql += $" limit {skipCount} , {pageInfo.PageSize}";
            }
            else
            {
                sql += $" limit 0 , 0";
            }

            return sql;
        }

        #region 分页查询命令执行(不提供带事务方法)
        /// <summary>
        /// 使用SearchParameter查询并分页
        /// 先使用SearchParamerters 创建带分页的查询语句,以及动态查询参数
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sqlKey">key</param>
        /// <param name="searchParameters">查询条件</param>
        /// <returns>列表</returns>
        public List<T> QueryByPageSqlKey<T>(string sqlKey, SearchParameters searchParameters)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return QueryByPageSql<T>(sql, searchParameters);
        }
        public List<dynamic> QueryByPageSqlKey(string sqlKey, SearchParameters searchParameters)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return QueryByPageSql<dynamic>(sql, searchParameters);
        }
        public List<T> QueryByPageSql<T>(string sql, SearchParameters searchParameters)
        {
            searchParameters.PageInfo.TotalCount = GetTotalCountBySearchParameter(sql, searchParameters);
            string whereSql = GetSqlBySearchParameter(sql, searchParameters);
            var pageSql = GetPageSql(searchParameters, whereSql);
            DynamicParameters parameters = GetDynamicParametersBySearchParameter(searchParameters);
            try
            {
                using (var connection = GetConnection())
                {
                    return connection.Connection.Query<T>(pageSql, parameters, null, false, DbInfo.CommandTimeout, CommandType.Text).AsList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Parametric Query ,执行参数化SQL分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="pageInfo"></param>
        /// <param name="paramsDic"></param>
        /// <returns></returns>
        public List<T> QueryPageByParametricSql<T>(string sql, PageInfo pageInfo, Dictionary<string,object> paramsDic)
        {
            SearchParameters searchParameters = new SearchParameters();
            searchParameters.PageInfo = pageInfo;

            searchParameters.PageInfo.TotalCount = GetTotalCountByParameters(sql, pageInfo, paramsDic);
            string whereSql = GetSqlBySearchParameter(sql, searchParameters);
            var pageSql = GetPageSql(searchParameters, whereSql);
            DynamicParameters parameters = GetDynamicParametersBySearchParameter(searchParameters);
            if (paramsDic != null)
            {
                foreach (var kv in paramsDic)
                    parameters.Add(kv.Key, kv.Value);
            }
            try
            {
                using (var connection = GetConnection())
                {
                    return connection.Connection.Query<T>(pageSql, parameters, null, false, DbInfo.CommandTimeout, CommandType.Text).AsList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<dynamic> QueryByPageSql(string sql, SearchParameters searchParameters)
        {
            int totalCount = GetTotalCountBySearchParameter(sql, searchParameters);
            string pageSql = GetSqlBySearchParameter(sql, searchParameters);
            DynamicParameters parameters = GetDynamicParametersBySearchParameter(searchParameters);
            try
            {
                using (var connection = GetConnection())
                {
                    return connection.Connection.Query(pageSql, parameters, null, false, DbInfo.CommandTimeout, CommandType.Text).AsList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private int GetTotalCountBySearchParameter(string sql, SearchParameters searchParameters)
        {
            if (!searchParameters.PageInfo.IsGetTotalCount)
            {
                searchParameters.PageInfo.TotalCount = 5000;
            }
            else
            {
                // 只替换第一次的select xxxx from. 子查询不替换
                var countSql = Regex.Replace(sql.Trim(), @"select[\s|\r|\n][\s\S]*?[\s|\r|\n]from", match =>
                {
                    if (match.Index == 0) return "select count(1) from";
                    return match.Value;
                }, RegexOptions.IgnoreCase);
                var countParaSql = GetSqlBySearchParameter(countSql, searchParameters);
                DynamicParameters parameters = GetDynamicParametersBySearchParameter(searchParameters);

                try
                {
                    using (var connection = GetConnection())
                    {
                        searchParameters.PageInfo.TotalCount = connection.Connection.Query<int>(countParaSql, parameters, null, false, DbInfo.CommandTimeout, CommandType.Text)?.FirstOrDefault()??0;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return searchParameters.PageInfo.TotalCount;
        }

        private int GetTotalCountByParameters(string sql, PageInfo pageInfo, Dictionary<string, object> paramsDic)
        {
            if (!pageInfo.IsGetTotalCount)
            {
                pageInfo.TotalCount = 5000;
            }
            else
            {
                // 只替换第一次的select xxxx from. 子查询不替换
                var countSql = Regex.Replace(sql.Trim(), @"select[\s|\r|\n][\s\S]*?[\s|\r|\n]from", match =>
                {
                    if (match.Index == 0) return "select count(1) from";
                    return match.Value;
                }, RegexOptions.IgnoreCase);

                DynamicParameters parameters = new DynamicParameters();
                if (paramsDic != null)
                {
                    foreach (var kv in paramsDic)
                        parameters.Add(kv.Key, kv.Value);
                }

                try
                {
                    using (var connection = GetConnection())
                    {
                        pageInfo.TotalCount = connection.Connection.Query<int>(countSql, parameters, null, false, DbInfo.CommandTimeout, CommandType.Text)?.FirstOrDefault()??0;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return pageInfo.TotalCount;
        }

        /// <summary>
        /// 使用SearchParamerters 自动追加 sql 的 where 条件, 并用占位符处理value   例: `ORDER_ID`=@OrderId
        /// 并根据PageInfo信息生成sql分页内容
        /// </summary>
        /// <param name="sql">sqlkey</param>
        /// <param name="searchParameters">查询条件</param>
        /// <returns>返回一个组织好的sql语句</returns>
        private string GetSqlBySearchParameter(string sql, SearchParameters searchParameters)
        {
            StringBuilder sb = new StringBuilder(sql);
            if (searchParameters.QueryModel.Items.Count > 0)
            {
                sb.Append(" where " + ConvertToSqlWhere(searchParameters, DbType.MySql));
            }
            if (string.IsNullOrEmpty(searchParameters.PageInfo.SortDirection))
            {
                if (!string.IsNullOrEmpty(searchParameters.PageInfo.SortField))
                {
                    sb.Append(" Order by ");
                    //如果是多字段排序，则会把排序字段和排序方式记录到sort上，dir为空
                    char[] delimiters = { ',' };
                    string[] sorts =
                        searchParameters.PageInfo.SortField.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    var mapDic = DbColumnMaps.GetDbContextFiledMaps();
                    for (int i = 0; i < sorts.Length; i++)
                    {
                        string[] values = sorts[i].Trim().Split(' ');
                        string sortName = values[0];
                        if (mapDic.ContainsKey(sortName))
                        {
                            sortName = mapDic[sortName];
                        }
                        string sortDir = values.Length == 2 ? values[1] : "ASC";
                        if (i == sorts.Length - 1)
                        {
                            sb.Append(" " + sortName + " " + sortDir + " ");
                        }
                        else
                        {
                            sb.Append(" " + sortName + " " + sortDir + ", ");
                        }
                    }
                }
            }
            else
            {
                var mapDic = DbColumnMaps.GetDbContextFiledMaps();
                var sortName = searchParameters.PageInfo.SortField;
                if (mapDic.ContainsKey(sortName))
                {
                    sortName = mapDic[sortName];
                }
                sb.Append(" Order by " + sortName + " " + searchParameters.PageInfo.SortDirection + " ");
            }
            return sb.ToString();
        }
        /// <summary>
        /// 根据sqlKey获取sql
        /// </summary>
        /// <param name="sqlKey"></param>
        /// <returns></returns>
        public string GetSqlByKey(string sqlKey)
        {
            return ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
        }
        #endregion

        #region 查询命令执行
        public DataSet QueryDataSetBySqlKey(string sqlKey, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return QueryDataSetBySql(sql, commandType);
        }
        public List<dynamic> QuerySqlKey(string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return QuerySql(sql, paramsList, valuesList, commandType);
        }
        public List<dynamic> QuerySqlKey(IDbTransaction trans, string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return QuerySql(trans, sql, paramsList, valuesList, commandType);
        }

        public List<T> QuerySqlKey<T>(string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return QuerySql<T>(sql, paramsList, valuesList,commandType);
        }
        public List<T> QuerySqlKey<T>(IDbTransaction trans, string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return QuerySql<T>(trans, sql, paramsList, valuesList, commandType);
        }

        public DataSet QueryDataSetBySql(string sql,CommandType commandType=CommandType.Text)
        {
            DataSet ds = new DataSet();
            try
            {
                var arrSql = sql.Split(";");
                using (var connection = GetConnection())
                {
                    foreach (var tmpSql in arrSql)
                    {
                        if (string.IsNullOrEmpty(tmpSql))
                        {
                            continue;
                        }
                        //using (DbCommand cmd = new MySqlCommand(tmpSql, connection.Connection as MySqlConnection))
                        using (DbCommand cmd = connection.Connection.CreateCommand())
                        {
                            cmd.CommandText = tmpSql;
                            cmd.CommandType = commandType;
                            cmd.CommandTimeout = DbInfo.CommandTimeout;
                            using (var dr = cmd.ExecuteReader())
                            {
                                var dt = new DataTable();
                                ds.Tables.Add(dt);
                                dt.Load(dr);
                            }
                        }
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<dynamic> QuerySql(string sql, string[] paramsList, object[] valuesList,CommandType commandType=CommandType.Text)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    DynamicParameters ps = PrepareCommand(paramsList, valuesList);
                    return connection.Connection.Query(sql, ps, null, false, DbInfo.CommandTimeout, commandType).AsList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<dynamic> QuerySql(IDbTransaction trans, string sql, string[] paramsList, object[] valuesList,CommandType commandType = CommandType.Text)
        {
            DynamicParameters ps = PrepareCommand(paramsList, valuesList);
            PrintSql(sql, ps);
            return trans.Connection.Query(sql, ps, trans, false, DbInfo.CommandTimeout, commandType).AsList();
        }
        public List<T> QuerySql<T>(string sql, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    DynamicParameters ps = PrepareCommand(paramsList, valuesList);
                    return connection.Connection.Query<T>(sql, ps, null, false, DbInfo.CommandTimeout, commandType).AsList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<T> QuerySql<T>(IDbTransaction trans, string sql, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            DynamicParameters ps = PrepareCommand(paramsList, valuesList);
            PrintSql(sql, ps);
            return trans.Connection.Query<T>(sql, ps, trans, false, DbInfo.CommandTimeout, commandType).AsList();
        }
        #endregion

        #region 标量命令执行
        public T ExecuteScalarSqlKey<T>(string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return ExecuteScalarSql<T>(sql, paramsList, valuesList,commandType);
        }
        public T ExecuteScalarSqlKey<T>(IDbTransaction trans, string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return ExecuteScalarSql<T>(trans, sql, paramsList, valuesList, commandType);
        }
        public dynamic ExecuteScalarSqlKey(string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return ExecuteScalarSql(sql, paramsList, valuesList, commandType);
        }
        public dynamic ExecuteScalarSqlKey(IDbTransaction trans, string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return ExecuteScalarSql(trans, sql, paramsList, valuesList, commandType);
        }
        public T ExecuteScalarSql<T>(string sql, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalarCommand<T>(sql, commandType, paramsList, valuesList);
        }
        public T ExecuteScalarSql<T>(IDbTransaction trans, string sql, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalarCommand<T>(trans, sql, commandType, paramsList, valuesList);
        }
        public dynamic ExecuteScalarSql(string sql, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalarCommand(sql, commandType, paramsList, valuesList);
        }
        public dynamic ExecuteScalarSql(IDbTransaction trans, string sql, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalarCommand(trans, sql, commandType, paramsList, valuesList);
        }

        protected dynamic ExecuteScalarCommand(string sql, CommandType commandType, string[] paramsList, object[] valuesList)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    using (var tran = connection.Connection.BeginTransaction())
                    {
                        try
                        {
                            DynamicParameters ps = PrepareCommand(paramsList, valuesList);
                            dynamic v = connection.Connection.ExecuteScalar(sql, ps, tran, DbInfo.CommandTimeout, commandType);
                            //OnDbCommit?.Invoke(connection, new SqlPara { sql = sql, parameters = ps, dbTransaction = tran });
                            tran.Commit();
                            return v;
                        }
                        catch (Exception e)
                        {
                            tran.Rollback();
                            throw e;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        protected dynamic ExecuteScalarCommand(IDbTransaction trans, string sql, CommandType commandType, string[] paramsList, object[] valuesList)
        {

            DynamicParameters ps = PrepareCommand(paramsList, valuesList);
            PrintSql(sql, ps);
            return trans.Connection.ExecuteScalar(sql, ps, trans, DbInfo.CommandTimeout, commandType);
        }
        protected T ExecuteScalarCommand<T>(string sql, CommandType commandType, string[] paramsList, object[] valuesList)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    using (var tran = connection.Connection.BeginTransaction())
                    {
                        try
                        {
                            DynamicParameters ps = PrepareCommand(paramsList, valuesList);
                            T v = connection.Connection.ExecuteScalar<T>(sql, ps, tran, DbInfo.CommandTimeout, commandType);
                            //OnDbCommit?.Invoke(connection, new SqlPara { sql = sql, parameters = ps, dbTransaction = tran });
                            tran.Commit();
                            return v;
                        }
                        catch (Exception e)
                        {
                            tran.Rollback();
                            throw e;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        protected T ExecuteScalarCommand<T>(IDbTransaction trans, string sql, CommandType commandType, string[] paramsList, object[] valuesList)
        {
            DynamicParameters ps = PrepareCommand(paramsList, valuesList);
            PrintSql(sql, ps);
            return trans.Connection.ExecuteScalar<T>(sql, ps, trans, DbInfo.CommandTimeout, commandType);
        }
        #endregion

        #region 非查询命令执行
        public int ExecuteNoQuerySqlKey(string sqlKey, object param, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return ExecuteNoQueryCommand(sql, param, commandType);
        }
        public int ExecuteNoQuerySqlKey(IDbTransaction trans, string sqlKey, object param, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return ExecuteNoQueryCommand(trans, sql, param, commandType);
        }
        public int ExecuteNoQuerySqlKey(string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return ExecuteNoQuerySql(sql, paramsList, valuesList, commandType);
        }
        public int ExecuteNoQuerySqlKey(IDbTransaction trans, string sqlKey, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            string sql = ServerSetting.GetSql(DbInfo.DbContextName, sqlKey);
            return ExecuteNoQuerySql(trans, sql, paramsList, valuesList, commandType);
        }
        public int ExecuteNoQuerySql(string sql, object param, CommandType commandType = CommandType.Text)
        {
            return ExecuteNoQueryCommand(sql, param, commandType);
        }
        public int ExecuteNoQuerySql(IDbTransaction trans, string sql, object param, CommandType commandType = CommandType.Text)
        {
            return ExecuteNoQueryCommand(trans, sql, param, commandType);
        }
        public int ExecuteNoQuerySql(string sql, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            return ExecuteNoQueryCommand(sql, commandType, paramsList, valuesList);
        }
        public int ExecuteNoQuerySql(IDbTransaction trans, string sql, string[] paramsList, object[] valuesList, CommandType commandType = CommandType.Text)
        {
            return ExecuteNoQueryCommand(trans, sql, commandType, paramsList, valuesList);
        }



        protected int ExecuteNoQueryCommand(string sql, CommandType commandType, string[] paramsList, object[] valuesList)
        {
            DynamicParameters ps = PrepareCommand(paramsList, valuesList);
            return ExecuteNoQueryCommand(sql, ps, commandType);
        }
        protected int ExecuteNoQueryCommand(IDbTransaction trans, string sql, CommandType commandType, string[] paramsList, object[] valuesList)
        {
            DynamicParameters ps = PrepareCommand(paramsList, valuesList);
            return ExecuteNoQueryCommand(trans, sql, ps, commandType);
        }
        protected int ExecuteNoQueryCommand(string sql, object param, CommandType commandType)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    using (var tran = connection.Connection.BeginTransaction())
                    {
                        try
                        {
                            int i = connection.Connection.Execute(sql, param, tran, DbInfo.CommandTimeout, commandType);
                            //OnDbCommit?.Invoke(connection, new SqlPara { sql = sql, param = param, dbTransaction = tran });
                            tran.Commit();
                            return i;
                        }
                        catch (Exception e)
                        {
                            tran.Rollback();
                            throw e;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        protected int ExecuteNoQueryCommand(IDbTransaction trans, string sql, object param, CommandType commandType)
        {
            PrintSql(sql, param);
            return trans.Connection.Execute(sql, param, trans, DbInfo.CommandTimeout, commandType);
        }
        #endregion

        private void PrintSql(string sql, object param)
        {
            try
            {
                if (ServerSetting.GetConstValue("TrackSql")?.Value.ToLower() == "true")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("TrackSql=" + sql);
                    sb.AppendLine(JsonEx.JsonConvert.JsonSerializer(param));
                    logger.LogInformation(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                //打印SQL异常时不影响业务
            }
        }

        private void PrintSql(string sql, DynamicParameters ps)
        {
            try
            {
                if (ServerSetting.GetConstValue("TrackSql")?.Value.ToLower() == "true")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("TrackSql=" + sql);
                    ps?.ParameterNames?.ToList()?.ForEach(x =>
                    {
                        sb.AppendLine($"{x}={ps.Get<object>(x)}");
                    });
                    logger.LogInformation(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                //打印SQL异常时不影响业务
            }
        }
    }
}