/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.DB.EFEx
 文件名：  GrantDBContext
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/11/25 12:42:41

 功能描述：一个DBContext的动态管理类

----------------------------------------------------------------*/

using Microsoft.EntityFrameworkCore;
using GrantMicroService.ExceptionEx;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GrantMicroService.Config;
using GrantMicroService.DB.AttributeEx;
using GrantMicroService.DB.EFEx.DynamicSearch;
using GrantMicroService.DB.EFEx.GrantDbContext;
using GrantMicroService.Tools;
using Z.EntityFramework.Plus;

namespace GrantMicroService.DB.EFEx
{
    /// <summary>
    /// GrantDBContext
    /// </summary>
    public partial class GrantEFDbContext : IGrantEFDbContext
    {
        //public event DbEFCommit OnDbCommit;
        private DbContext _dbContext;
        private DbInfo _dbInfo;

        private readonly object _rootObj = new object();
        private Dictionary<string, object> _repositories = new Dictionary<string, object>();

        public GrantEFDbContext(DbContext context, DbInfo dbInfo)
        {
            _dbContext = context;
            _dbInfo = dbInfo;
        }

        /// <summary>
        /// 获取EF的 Repository
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public ICrudRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            // 先检查，检查没有，锁住再检查，保证原子性，又保证性能
            var type = typeof(TEntity).FullName.ToLower();
            if (_repositories.ContainsKey(type))
            {
                return (ICrudRepository<TEntity>)_repositories[type];
            }

            lock (_rootObj)
            {
                if (_repositories.ContainsKey(type))
                {
                    return (ICrudRepository<TEntity>)_repositories[type];
                }

                ICrudRepository<TEntity> crud = new EFCrudRepository<TEntity>(_dbContext, _dbInfo);
                _repositories.Add(type, crud);
                return crud;
            }
        }

        /// <inheritdoc />
        public IQueryable<T> GetByPage<T>(Expression<Func<T, bool>> exp, PageInfo pageInfo, out int count, bool isGetTotalCount = true) where T : class
        {
            return GetByPage(
                exp,
                pageInfo.SkipCount > 0 ? pageInfo.SkipCount : ((pageInfo.CurrentPage - 1) * pageInfo.PageSize),
                pageInfo.PageSize,
                pageInfo.SortField,
                pageInfo.SortDirection,
                out count,
                isGetTotalCount);
        }

        /// <inheritdoc />
        public IQueryable<T> GetByPage<T>(Expression<Func<T, bool>> exp, int skipCount, int pageSize, string sort, string dir, out int count,
            bool isGetTotalCount = true) where T : class
        {
            return Pagination<T>.PageList(_dbContext.Set<T>().Where(exp), skipCount, pageSize, sort, dir, out count, isGetTotalCount);
        }

        /// <inheritdoc />
        public IQueryable<T> GetByPage<T>(IQueryable<T> query, PageInfo pageInfo, out int count) where T : class
        {
            return Pagination<T>.PageList(
                query,
                pageInfo.SkipCount > 0 ? pageInfo.SkipCount : ((pageInfo.CurrentPage - 1) * pageInfo.PageSize),
                pageInfo.PageSize,
                pageInfo.SortField,
                pageInfo.SortDirection,
                out count,
                pageInfo.IsGetTotalCount);
        }

        /// <inheritdoc />
        public IQueryable<T> GetByPage<T>(SearchParameters searchParameters, string sqlwhere = "") where T : class
        {
            //处理EF查询时，查询null值问题
            searchParameters.BuildEmptySearch();
            if (searchParameters.PageInfo.IsGetTotalCount && searchParameters.PageInfo.PageSize <= 1)
            {
                var attr = ReflectionTool.GetCustomAttributeEx<ViewOraModelAttribute>(typeof(T));
                if (attr != null)
                {
                    var oraModelPropInfos = ReflectionTool.GetPropertyInfosFromCache(attr.OraModel);

                    // 判断是否所有的查询条件都在单表内
                    var sqlWhereCondition = sqlwhere.Split(
                        new[] { " And ", " Or ", " AND ", " OR ", " and ", " or " },
                        StringSplitOptions.RemoveEmptyEntries);
                    sqlWhereCondition = GetPropNameBySqlWhere(sqlWhereCondition);
                    if (searchParameters.QueryModel?.Items != null
                        && searchParameters.QueryModel.Items.All(a => oraModelPropInfos.Any(b => b.Name == a.Field))
                        && sqlWhereCondition.All(a => oraModelPropInfos.Any(b => a.ToLower().Equals(b.Name.ToLower()))))
                    {
                        GetSingleTableQuickCount(searchParameters, sqlwhere, attr);
                        return new List<T>().AsQueryable();
                    }
                }
            }

            IQueryable<T> list = _dbContext.Set<T>().Where(searchParameters.QueryModel);
            if (!string.IsNullOrEmpty(sqlwhere))
            {
                list = list.Where(sqlwhere);
            }

            var totalCount = 0;
            var result = GetByPage(list, searchParameters.PageInfo, out totalCount);
            searchParameters.PageInfo.TotalCount = totalCount;
            return result;
        }

        /// <inheritdoc />
        public T GetByID<T>(params object[] keyValue) where T : class
        {
            return _dbContext.Set<T>().Find(keyValue);
        }

        /// <inheritdoc />
        public T Get<T>(Expression<Func<T, bool>> exp, bool isTrackEntity = true) where T : class
        {
            if (isTrackEntity)
            {
                return _dbContext.Set<T>().FirstOrDefault(exp);
            }
            else
            {
                return _dbContext.Set<T>().AsNoTracking().FirstOrDefault(exp);
            }            
        }
        /// <summary>
        /// 获取所有数据
        /// 警告：此方法慎用
        /// </summary>
        /// <param name="isTrackEntity">是否跟踪实体(默认不跟踪)</param>
        /// <inheritdoc />
        public List<T> GetAll<T>(bool isTrackEntity = false) where T : class
        {
            if(isTrackEntity)
            {
                return _dbContext.Set<T>().ToList();
            }
            else
            {
                return _dbContext.Set<T>().AsNoTracking().ToList();
            }
        }

        /// <inheritdoc />
        public List<T> GetByQuery<T>(Expression<Func<T, bool>> exp, bool isTrackEntity = true) where T : class
        {
            if (isTrackEntity)
            {
                return _dbContext.Set<T>().Where(exp).ToList();
            }
            else
            {
                return _dbContext.Set<T>().AsNoTracking().Where(exp).ToList();
            }   
        }

        /// <inheritdoc />
        public IQueryable<T> GetQueryableByQuery<T>(Expression<Func<T, bool>> exp, bool isTrackEntity = true) where T : class
        {
            if (isTrackEntity)
            {
                return _dbContext.Set<T>().Where(exp);
            }
            else
            {
                return _dbContext.Set<T>().AsNoTracking().Where(exp);
            }       
        }

        /// <inheritdoc />
        public void Insert<T>(T entity) where T : class
        {
            _dbContext.Set<T>().Add(entity);
        }

        /// <inheritdoc />
        public void BatchInsert<T>(T[] entites) where T : class
        {
            _dbContext.Set<T>().AddRange(entites);
        }

        /// <inheritdoc />
        public void Update<T>(T entity, Func<T, bool> predicate = null) where T : class
        {
            if (predicate != null)
            {
                var attachedEntity = _dbContext.Set<T>().Local.FirstOrDefault(predicate);
                if (attachedEntity != null)
                {
                    var attachedEntry = _dbContext.Entry(attachedEntity);
                    attachedEntry.CurrentValues.SetValues(entity);
                    return;
                }
                else
                {
                    _dbContext.Set<T>().Update(entity);
                }
            }
            else
            {
                _dbContext.Set<T>().Update(entity);
            }
        }

        /// <inheritdoc />
        public void BatchUpdate<T>(T[] entites) where T : class
        {
            _dbContext.Set<T>().UpdateRange(entites);
        }

        /// <inheritdoc />
        public void BatchDelete<T>(Expression<Func<T, bool>> exp) where T : class
        {
            IEnumerable<T> objects = _dbContext.Set<T>().Where(exp).ToList();
            _dbContext.Set<T>().RemoveRange(objects);
        }

        /// <inheritdoc />
        public void Delete<T>(params object[] keyValue) where T : class
        {
            T t = _dbContext.Set<T>().Find(keyValue);
            if (t != null)
            {
                _dbContext.Set<T>().Remove(t);
            }
        }

        /// <inheritdoc />
        public void Delete<T>(T entity) where T : class
        {
            _dbContext.Set<T>().Remove(entity);
        }

        /// <inheritdoc />
        public List<T> SqlQuery<T>(string sqlKey, params object[] parameters) where T : class
        {
            var sql = ServerSetting.GetSql(_dbInfo.DbContextName, sqlKey);
            return _dbContext.Set<T>().FromSqlRaw(sql, parameters).AsNoTracking().ToList();
        }

        /// <inheritdoc />
        public int ExecuteSqlCommand(string sqlKey, params object[] parameters)
        {
            var sql = ServerSetting.GetSql(_dbInfo.DbContextName, sqlKey);
            return _dbContext.Database.ExecuteSqlRaw(sql, parameters);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // clear repositories
            if (_repositories != null)
            {
                _repositories.Clear();
                _repositories = null;
            }

            // dispose the db context.
            _dbContext?.Dispose(); // 这里的_dbContext释放掉就等于把Repository里面的_dbContext去释放吧
        }

        /// <summary>
        /// 从数据库中刷新所有已经查询出来的实体对象.
        /// </summary>
        public virtual void ReloadAll()
        {
            foreach (var entity in _dbContext.ChangeTracker.Entries())
            {
                entity.Reload();
            }
        }

        /// <summary>
        /// 刷新单个对象
        /// </summary>
        /// <param name="t"></param>
        public virtual void Reload<T>(T t) where T : class
        {
            _dbContext.Entry(t).Reload();
        }

        /// <summary>
        ///     是当前DBContext 提交到数据库中
        /// </summary>
        /// <exception cref="BusinessException">数据已经被更改，请重新加载操作</exception>
        /// <exception cref="BusinessException">DbUpdateException 提取 InnerException 抛出，开发更容易看懂</exception>
        /// <exception cref="BusinessException">DbEntityValidationException 提取EntityValidationErrors 抛出，开发更容易看懂</exception>
        public void Commit()
        {
            try
            {
                var isDbTran = (_dbContext.Database.CurrentTransaction != null)
                               || _dbContext.Database.IsInMemory(); // 内存数据库不支持Transaction 只能直接提交
                if (isDbTran)
                {
                    //var audit = new Z.EntityFramework.Plus.Audit();
                    _dbContext.SaveChanges();
                    //OnDbCommit?.Invoke(_dbContext, audit);
                }
                else
                {
                    //手工增加事物,是为了不使用默认的事物隔离级别 RepeateableRead , 强制使用 ReadCommitted .
                    using (var tran = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            //var audit = new Z.EntityFramework.Plus.Audit();
                            _dbContext.SaveChanges();
                            //OnDbCommit?.Invoke(_dbContext, audit);
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                tran.Rollback();
                            }
                            catch
                            {
                                // 防止回滚的异常将提交异常冲掉
                            }
                            throw ex;
                        }
                    }
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("系统并发日志：");
                if (ex.Entries != null)
                {
                    foreach (var entrie in ex.Entries)
                    {
                        builder.Append("{");
                        builder.Append("Type:" + entrie.Entity.GetType().Name + ",");
                        builder.Append("EntityState:" + entrie.State + ",");
                        builder.Append("Propertys:{");
                        var currentValues = entrie.CurrentValues;
                        foreach (var name in currentValues.Properties)
                        {
                            if (currentValues[name] != null)
                            {
                                string value = currentValues[name].ToString();
                                builder.AppendFormat("{0}:{1},", name.Name, value);
                            }
                        }
                        builder.Append("}}");
                    }
                }

                var newEx = new Exception(builder.ToString().Replace("}{", "},{").Replace(",}", "}"), ex);

                //并发处理控制，捕获并发异常信息，以业务异常抛出
                throw new BusinessException("数据已经被更改，请重新加载操作", newEx);
            }
            catch (Exception e)
            {
                var errorMsg = "数据提交出错。";
                if (e is ValidationException || e is DbUpdateException)
                {
                    //获取根部异常信息
                    //抛出内部异常，开发更容易看懂
                    var newEx = new Exception();
                    if (e.InnerException != null)
                    {
                        newEx = e.InnerException;
                    }
                    while (newEx.InnerException != null)
                    {
                        newEx = newEx.InnerException;
                    }

                    if (newEx != null)
                    {
                        errorMsg = ExceptionTool.GetEasyErrorMessage(newEx.Message);
                    }
                }
                throw new BusinessException(errorMsg, e);
            }
        }

        /// <summary>
        /// 用正则过滤出来真正的列名, 正则可能会不完整
        /// </summary>
        /// <param name="sqlWhereCondition">sqlWhereCondition</param>
        /// <returns>string[]</returns>
        private string[] GetPropNameBySqlWhere(string[] sqlWhereCondition)
        {
            for (int i = 0; i < sqlWhereCondition.Length; i++)
            {
                ////找第一个单词开头
                var matched = Regex.Match(sqlWhereCondition[i], "[A-Za-z|_]*?[/.|!|=|<|>]");
                if (matched.Success)
                {
                    sqlWhereCondition[i] = matched.Value.Remove(matched.Value.Length - 1);
                }
            }

            return sqlWhereCondition;
        }

        /// <summary>
        /// 单表统计总数，解决Count 性能问题，如果所查询的内容都在单表字段里，则不需要走原视图查询
        /// 根据之前的日订单量自动决定是否使用多线程查询合并
        /// </summary>
        /// <param name="searchParameters">searchParameters</param>
        /// <param name="sqlwhere">sqlwhere</param>
        /// <param name="attr">attr</param>
        private void GetSingleTableQuickCount(SearchParameters searchParameters, string sqlwhere, ViewOraModelAttribute attr)
        {
            Type t = _dbContext.GetType();
            var mi = t.GetMethod("Set", new Type[] { }).MakeGenericMethod(attr.OraModel);
            var tempQueryable = mi.Invoke(_dbContext, null);

            Type queryableT = typeof(QueryableExtensions);
            var queryableWhereMethod = queryableT.GetMethod("Where").MakeGenericMethod(attr.OraModel);
            tempQueryable =
                queryableWhereMethod.Invoke(null, new object[] { tempQueryable, searchParameters.QueryModel, string.Empty });

            if (!string.IsNullOrEmpty(sqlwhere))
            {
                //// 获取Where<T>(string) 方法 并调用
                var queryableWhereStringMethod = typeof(System.Linq.Dynamic.Core.DynamicQueryableExtensions)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)

                    // narrow the search before doing 'Single()'
                    .Single(m => m.Name == "Where"

                                 // this check technically not required, but more future proof
                                 && m.IsGenericMethodDefinition
                                 && m.GetParameters().Any(a => a.ParameterType.Name == "IQueryable`1"))
                    .MakeGenericMethod(attr.OraModel);
                tempQueryable = queryableWhereStringMethod.Invoke(null, new object[] { tempQueryable, sqlwhere, null });
            }
            //// 获取Count<T>() 方法 并调用
            var queryableCountMethod = typeof(Queryable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(m => m.Name == "Count"
                             && m.IsGenericMethodDefinition
                             && m.GetParameters().Length == 1)
                .MakeGenericMethod(attr.OraModel);
            searchParameters.PageInfo.TotalCount =
                int.Parse(queryableCountMethod.Invoke(null, new object[] { tempQueryable }).ToString());
        }
    }
}