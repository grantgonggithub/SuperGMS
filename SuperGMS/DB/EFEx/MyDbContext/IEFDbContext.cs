/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx.GrantDbContext
 文件名：  IGrantEFDbContext
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/31 16:55:31

 功能描述：

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SuperGMS.DB.EFEx.DynamicSearch;

namespace SuperGMS.DB.EFEx.GrantDbContext
{
    /// <summary>
    /// IGrantEFDbContext
    /// </summary>
    public interface IEFDbContext : IDbContext
    {
        ICrudRepository<TEntity> GetRepository<TEntity>()
            where TEntity : class;

        /// <summary>
        ///     根据条件查询 并且分页，返回总记录数
        /// </summary>
        /// <param name="exp">查询表达式</param>
        /// <param name="pageInfo">分页信息<see cref="PageInfo" /></param>
        /// <param name="count">总记录数</param>
        /// <param name="isGetTotalCount">是否获取总页数，默认是true</param>
        /// <returns>一个待查询的结果集</returns>
        IQueryable<T> GetByPage<T>(Expression<Func<T, bool>> exp, PageInfo pageInfo, out int count,
           bool isGetTotalCount = true) where T : class;

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
        IQueryable<T> GetByPage<T>(Expression<Func<T, bool>> exp, int skipCount, int pageSize, string sort,
           string dir, out int count, bool isGetTotalCount = true) where T : class;

        /// <summary>
        ///     分页查询
        /// </summary>
        /// <param name="query">待查询的结果集</param>
        /// <param name="pageInfo">分页信息<see cref="PageInfo" /></param>
        /// <param name="count">总记录数</param>
        /// <returns>一个待查询的结果集</returns>
        IQueryable<T> GetByPage<T>(IQueryable<T> query, PageInfo pageInfo, out int count) where T : class;

        /// <summary>
        /// 分页查询
        /// 如果T中有OraModel，并且查询总记录数，并且查询条件都隶属于OraModel，则使用单表查询
        /// </summary>
        /// <param name="searchParameters">查询参数 <see cref="DynamicSearch.SearchParameters" /></param>
        /// <param name="sqlwhere">字符串的查询条件</param>
        /// <returns>一个待查询的结果集</returns>
        IQueryable<T> GetByPage<T>(SearchParameters searchParameters, string sqlwhere = "") where T : class;

        /// <summary>
        /// 通过主键获取一个对象，如果是组合主键，必须按照主键顺序填写
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <returns>如果找到则返回该表对象(并被EF跟踪),否则返回null</returns>
        T GetByID<T>(params object[] keyValue) where T : class;

        /// <summary>
        /// 根据一个表达式获取一个对象
        /// </summary>
        /// <param name="exp">查询表达式</param>
        /// <param name="isTrackEntity">是否跟踪实体(默认跟踪)</param>
        /// <returns>该表对象</returns>
        T Get<T>(Expression<Func<T, bool>> exp, bool isTrackEntity = true) where T : class;

        /// <summary>
        /// 获取所有数据
        /// 警告：此方法慎用
        /// </summary>
        /// <param name="isTrackEntity">是否跟踪实体(默认不跟踪)</param>
        /// <returns>表中所有结果集</returns>
        List<T> GetAll<T>(bool isTrackEntity = false) where T : class;

        /// <summary>
        ///     根据表达式获取一个泛型对象集合，已连接数据库
        /// </summary>
        /// <param name="exp">查询表达式</param>
        /// <param name="isTrackEntity">是否跟踪实体(默认跟踪)</param>
        /// <returns>结果集</returns>
        List<T> GetByQuery<T>(Expression<Func<T, bool>> exp, bool isTrackEntity = true) where T : class;

        /// <summary>
        /// 根据表达式获取一个泛型对象集合，未连接数据库，需要使用ToList方法查询数据库转换成实体对象集合
        /// </summary>
        /// <param name="exp">查询表达式</param>
        /// <param name="isTrackEntity">是否跟踪实体(默认跟踪)</param>
        /// <returns>一个待查询的结果集</returns>
        IQueryable<T> GetQueryableByQuery<T>(Expression<Func<T, bool>> exp, bool isTrackEntity = true) where T : class;

        /// <summary>
        ///     插入一个对象
        /// </summary>
        /// <param name="entity">要插入的对象</param>
        void Insert<T>(T entity) where T : class;

        /// <summary>
        ///     插入多个对象
        /// </summary>
        /// <param name="entites">要插入的对象</param>
        void BatchInsert<T>(T[] entites) where T : class;

        /// <summary>
        ///     更新一个对象
        /// </summary>
        /// <param name="entity">此对象必须在当前DbContext中获取出来的对象</param>
        /// <param name="predicate">更新时在本地内存中查找,解决在同一个dbContext中多次查询相同的实体对象进行操作的问题</param>
        void Update<T>(T entity, Func<T, bool> predicate = null) where T : class;

        /// <summary>
        ///     批量更新对象
        /// </summary>
        /// <param name="entites">此对象必须在当前DbContext中获取出来的对象</param>
        void BatchUpdate<T>(T[] entites) where T : class;

        /// <summary>
        ///     删除一组满足查询表达式的对象
        /// </summary>
        /// <param name="exp">删除表达式</param>
        void BatchDelete<T>(Expression<Func<T, bool>> exp) where T : class;

        /// <summary>
        ///     根据主键删除一个对象
        /// </summary>
        /// <param name="keyValue">根据主键删除</param>
        void Delete<T>(params object[] keyValue) where T : class;

        /// <summary>
        ///     删除一个对象
        /// </summary>
        /// <param name="entity">此对象必须在当前DbContext中获取出来的对象</param>
        void Delete<T>(T entity) where T : class;

        /// <summary>
        /// 传递SQL来执行获取实体(不做实体跟踪)
        /// </summary>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="parameters">parameters</param>
        /// <returns>list</returns>
        List<T> SqlQuery<T>(string sqlKey, params object[] parameters) where T : class;

        /// <summary>
        /// 执行参数化脚本
        /// </summary>
        /// <param name="sqlKey">sqlKey</param>
        /// <param name="parameters">parameters</param>
        /// <returns>int</returns>
        int ExecuteSqlCommand(string sqlKey, params object[] parameters);

        void Commit();
    }
}