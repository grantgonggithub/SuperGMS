/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx
 文件名：  GrantDBContext
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/11/25 12:42:41

 功能描述：一个DBContext的动态管理类

----------------------------------------------------------------*/

using Microsoft.EntityFrameworkCore;
using SuperGMS.ExceptionEx;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SuperGMS.Config;
using SuperGMS.DB.AttributeEx;
using SuperGMS.DB.EFEx.DynamicSearch;
using SuperGMS.DB.EFEx.GrantDbContext;
using SuperGMS.Tools;
using Z.EntityFramework.Plus;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// GrantDBContext
    /// </summary>
    public partial class EFDbContext : IEFDbContext
    {
        //public event DbEFCommit OnDbCommit;
        private DbContext _dbContext;
        private DbInfo _dbInfo;

        private readonly object _rootObj = new object();
        private Dictionary<string, object> _repositories = new Dictionary<string, object>();

        public EFDbContext(DbContext context, DbInfo dbInfo)
        {
            _dbContext = context;
            _dbInfo = dbInfo;
        }

        internal ChangeTracker GetChangeTracker()
        {
            return _dbContext.ChangeTracker;
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
    }
}