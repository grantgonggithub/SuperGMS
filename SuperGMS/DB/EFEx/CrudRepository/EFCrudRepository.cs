#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using SuperGMS.DB.EFEx.DynamicSearch;
using Microsoft.EntityFrameworkCore;
using SuperGMS.Tools;
using SuperGMS.DB.AttributeEx;
using System.Linq.Dynamic.Core;
using SuperGMS.Config;
using SuperGMS.DB.EFEx.DynamicSearch.Model;

#endregion

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    ///     EF数据访问公共类,提供了 CRUD 和分页查询公共方法， 可以被继承增加更多的数据访问方法
    ///     Add By Grant 2014-3-26
    /// </summary>
    /// <typeparam name="T">可以实例化的类</typeparam>
    public class EFCrudRepository<T> : ICrudRepository<T>
        where T : class
    {
        /// <summary>
        /// Context
        /// </summary>
        protected readonly DbContext Context;

        private DbInfo _dbInfo;

        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="EFCrudRepository{T}"/> class.
        /// 默认构造方法需要传递一个 DbContext
        /// </summary>
        /// <param name="dbContext">dbContext</param>
        /// <param name="dbInfo">dbInfo</param>
        /// <param name="entityName"></param>
        [Obsolete("已过时, 请直接使用GrantEFDBContext")]
        public EFCrudRepository(DbContext dbContext, DbInfo dbInfo, string entityName = null)
        {
            Context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            if (!string.IsNullOrWhiteSpace(entityName))
            {
                _dbSet = Context.Set<T>(entityName);
            }
            else
            {
                _dbSet = Context.Set<T>();
            }
            _dbInfo = dbInfo;
        }

        /// <summary>
        /// 根据条件查询 并且分页，返回总记录数
        /// </summary>
        /// <param name="exp">查询表达式</param>
        /// <param name="pageInfo">分页信息<see cref="PageInfo" /></param>
        /// <param name="count">总记录数</param>
        /// <param name="isGetTotalCount">是否获取总页数，默认是true</param>
        /// <returns>一个待查询的结果集</returns>
        public IQueryable<T> GetByPage(Expression<Func<T, bool>> exp, PageInfo pageInfo, out int count, bool isGetTotalCount = true)
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

        /// <summary>
        ///     分页查询
        /// </summary>
        /// <param name="exp">查询表达式</param>
        /// <param name="skipCount">跳过的记录数</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="dir">排序方式</param>
        /// <param name="count">总记录数</param>
        /// <param name="isGetTotalCount">是否获取总页数，默认是true</param>
        /// <returns>一个待查询的结果集</returns>
        public IQueryable<T> GetByPage(Expression<Func<T, bool>> exp, int skipCount, int pageSize, string sort, string dir, out int count, bool isGetTotalCount = true)
        {
            return Pagination<T>.PageList(_dbSet.Where(exp), skipCount, pageSize, sort, dir, out count, isGetTotalCount);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="query">待查询的结果集</param>
        /// <param name="pageInfo">分页信息<see cref="PageInfo" /></param>
        /// <param name="count">总记录数</param>
        /// <returns>一个待查询的结果集</returns>
        public IQueryable<T> GetByPage(IQueryable<T> query, PageInfo pageInfo, out int count)
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

        /// <summary>
        /// 分页查询
        /// 如果T中有OraModel，并且查询总记录数，并且查询条件都隶属于OraModel，则使用单表查询
        /// </summary>
        /// <param name="searchParameters">查询参数 <see cref="DynamicSearch.SearchParameters" /></param>
        /// <returns>一个待查询的结果集</returns>
        public IQueryable<T> GetByPage(SearchParameters searchParameters)
        {
            //处理EF查询时，查询null值问题
            searchParameters.BuildEmptySearch();
            IQueryable<T> list = _dbSet.Where(searchParameters.QueryModel);
            var totalCount = 0;
            var result = GetByPage(list, searchParameters.PageInfo, out totalCount);
            searchParameters.PageInfo.TotalCount = totalCount;
            return result;
        }

        /// <summary>
        /// 通过主键获取一个对象，如果是组合主键，必须按照主键顺序填写
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <returns>如果找到则返回该表对象(并被EF跟踪),否则返回null</returns>
        public virtual T GetByID(params object[] keyValue)
        {
            return _dbSet.Find(keyValue);
        }

        /// <summary>
        /// 根据一个表达式获取一个对象
        /// </summary>
        /// <param name="exp">查询表达式</param>
        /// <param name="isTrackEntity">是否跟踪实体(默认跟踪)</param>
        /// <returns>该表对象</returns>
        public virtual T Get(Expression<Func<T, bool>> exp, bool isTrackEntity = true)
        {
            if (isTrackEntity)
            {
                return _dbSet.FirstOrDefault(exp);
            }
            else
            {
                return _dbSet.AsNoTracking().FirstOrDefault(exp);
            }        
        }

        /// <summary>
        /// 获取所有数据
        /// 警告：此方法慎用
        /// </summary>
        /// <param name="isTrackEntity">是否跟踪实体(默认不跟踪)</param>
        public virtual List<T> GetAll(bool isTrackEntity = false)
        {       
            if (isTrackEntity)
            {
                return _dbSet.ToList();
            }
            else
            {
                return _dbSet.AsNoTracking().ToList();
            }
        }

        /// <summary>
        /// 根据表达式获取一个泛型对象集合，已连接数据库
        /// </summary>
        /// <param name="exp">查询表达式</param>
        /// <param name="isTrackEntity">是否跟踪实体(默认跟踪)</param>
        /// <returns>结果集</returns>
        public virtual List<T> GetByQuery(Expression<Func<T, bool>> exp, bool isTrackEntity = true)
        {        
            if (isTrackEntity)
            {
                return _dbSet.Where(exp).ToList();
            }
            else
            {
                return _dbSet.AsNoTracking().Where(exp).ToList();
            }
        }

        /// <summary>
        /// 根据表达式获取一个泛型对象集合，未连接数据库，需要使用ToList方法查询数据库转换成实体对象集合
        /// </summary>
        /// <param name="exp">查询表达式</param>
        /// <param name="isTrackEntity">是否跟踪实体(默认跟踪)</param>
        /// <returns>一个待查询的结果集</returns>
        public virtual IQueryable<T> GetQueryableByQuery(Expression<Func<T, bool>> exp, bool isTrackEntity = true)
        {         
            if (isTrackEntity)
            {
                return _dbSet.Where(exp);
            }
            else
            {
                return _dbSet.AsNoTracking().Where(exp); ;
            }
        }

        /// <summary>
        /// 根据QueryModel获取一个泛型对象集合，未连接数据库，需要使用ToList方法查询数据库转换成实体对象集合
        /// </summary>
        /// <param name="queryModel">SearchParameters.QueryModel查询表达式</param>
        /// <param name="isTrackEntity">是否跟踪实体(默认跟踪)</param>
        /// <returns>一个待查询的结果集</returns>
        public virtual IQueryable<T> GetQueryableByQuery(QueryModel queryModel, bool isTrackEntity = true)
        {
            if (isTrackEntity)
            {
                return _dbSet.Where(queryModel);
            }
            else
            {
                return _dbSet.AsNoTracking().Where(queryModel) ;
            }
        }

        /// <summary>
        ///     插入一个对象
        /// </summary>
        /// <param name="entity">要插入的对象</param>
        public virtual void Insert(T entity)
        {
            _dbSet.Add(entity);
        }

        /// <inheritdoc />
        public void BatchInsert(T[] entites)
        {
            _dbSet.AddRange(entites);
        }

        /// <summary>
        /// 更新一个对象
        /// </summary>
        /// <param name="entity">此对象必须在当前DbContext中获取出来的对象</param>
        /// <param name="predicate">更新时在本地内存中查找,解决在同一个dbContext中多次查询相同的实体对象进行操作的问题</param>
        public virtual void Update(T entity, Func<T, bool> predicate = null)
        {
            if (predicate != null)
            {
                var attachedEntity = _dbSet.Local.FirstOrDefault(predicate);
                if (attachedEntity != null)
                {
                    var attachedEntry = Context.Entry(attachedEntity);
                    attachedEntry.CurrentValues.SetValues(entity);
                    return;
                }
                else if (!Context.ChangeTracker.AutoDetectChangesEnabled)
                {
                    //有实体跟踪,这里就不需要再Update了, 否则就是全字段更新, 并且会更新TS,引起并发问题混乱
                    _dbSet.Update(entity);
                }
            }
            else
            {
                if (!Context.ChangeTracker.AutoDetectChangesEnabled)
                {
                    //有实体跟踪,这里就不需要再Update了, 否则就是全字段更新, 并且会更新TS,引起并发问题混乱
                    _dbSet.Update(entity);
                }
            }
        }

        /// <inheritdoc />
        public void BatchUpdate(T[] entites)
        {
            _dbSet.UpdateRange(entites);
        }

        /// <summary>
        ///     删除一个对象
        /// </summary>
        /// <param name="entity">此对象必须在当前DbContext中获取出来的对象</param>
        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        ///     删除一组满足查询表达式的对象
        /// </summary>
        /// <param name="exp">删除表达式</param>
        public virtual void BatchDelete(Expression<Func<T, bool>> exp)
        {
            IEnumerable<T> objects = _dbSet.Where(exp).ToList();
            _dbSet.RemoveRange(objects);
        }

        /// <summary>
        ///     根据主键删除一个对象
        /// </summary>
        /// <param name="keyValue">根据主键删除</param>
        public virtual void Delete(params object[] keyValue)
        {
            T t = _dbSet.Find(keyValue);
            if (t != null)
            {
                _dbSet.Remove(t);
            }
        }

        /// <summary>
        ///     传递SQL来执行获取实体
        ///     例：
        ///     <![CDATA[
        /// SqlQuery<Post>("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// SqlQuery<Post>("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// ]]>
        /// </summary>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="parameters">SQL中所要替换的参数值</param>
        /// <returns>List</returns>
        public virtual List<T> SqlQuery(string sqlKey, params object[] parameters)
        {
            var sql = ServerSetting.GetSql(_dbInfo.DbContextName, sqlKey);
            return _dbSet.FromSqlRaw(sql, parameters).AsNoTracking().ToList();
        }

        /// <summary>
        /// ExecuteSqlCommand
        /// </summary>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="parameters">parameters</param>
        /// <returns>int</returns>
        public int ExecuteSqlCommand(string sqlKey, params object[] parameters)
        {
            var sql = ServerSetting.GetSql(_dbInfo.DbContextName, sqlKey);
            return Context.Database.ExecuteSqlRaw(sql, parameters);
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
            Type t = Context.GetType();
            var mi = t.GetMethod("Set", new Type[] { }).MakeGenericMethod(attr.OraModel);
            var tempQueryable = mi.Invoke(Context, null);

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