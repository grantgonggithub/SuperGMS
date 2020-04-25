//
// 文件：UnitOfWork.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29

#region

using System;
using System.Text;

#endregion

namespace GrantMicroService.DB.EFEx
{
    using Microsoft.EntityFrameworkCore;
    using GrantMicroService.ExceptionEx;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    ///    事物单元工厂类
    ///     add by Grant 2014-3-27
    /// </summary>
    /// <typeparam name="TContext">库名</typeparam>
    public class UnitOfWork<TContext> where TContext : DbContext, IDisposable
    {
        /// <summary>
        /// Gets the db context.
        /// </summary>
        /// <returns>The instance of type <typeparamref name="TContext"/>.</returns>
        public DbContext DbContext { get; }

        private bool disposed = false;
        private Dictionary<Type, object> repositories;

        public UnitOfWork(TContext context)
        {
            DbContext = context as DbContext;
        }

        /// <summary>
        /// Gets the specified repository for the <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>An instance of type inherited from <see cref="ICrudRepository{TEntity}"/> interface.</returns>
        public EFCrudRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (repositories == null)
            {
                repositories = new Dictionary<Type, object>();
            }

            var type = typeof(TEntity);
            if (!repositories.ContainsKey(type))
            {
                // repositories[type] = new EFCrudRepository<TEntity>(DbContext);
            }

            return (EFCrudRepository<TEntity>)repositories[type];
        }

        /// <summary>
        /// Executes the specified raw SQL command.
        /// </summary>
        /// <param name="sql">The raw SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The number of state entities written to database.</returns>
        public int ExecuteSqlCommand(string sql, params object[] parameters) => DbContext.Database.ExecuteSqlRaw(sql, parameters);

        /// <summary>
        /// Uses raw SQL queries to fetch the specified <typeparamref name="TEntity" /> data.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="sql">The raw SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>An <see cref="IQueryable{T}" /> that contains elements that satisfy the condition specified by raw SQL.</returns>
        public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class => DbContext.Set<TEntity>().FromSqlRaw(sql, parameters);

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
                DbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("系统并发日志：");
                if (ex.InnerException != null && ex.Entries != null)
                {
                    foreach (var entrie in ex.Entries)
                    {
                        builder.Append("{");
                        builder.Append("Type:" + entrie.Entity.GetType().Name + ",");
                        builder.Append("EntityState:" + entrie.State + ",");
                        builder.Append("Propertys:{");
                        var currentValues = entrie.CurrentValues;
                        foreach (var prop in currentValues.Properties)
                        {
                            if (currentValues[prop.Name] != null)
                            {
                                string value = currentValues[prop.Name].ToString();
                                builder.AppendFormat("{0}:{1},", prop.Name, value);
                            }
                        }
                        builder.Append("}}");
                    }
                }
                throw new BusinessException("数据已经被更改，请重新加载操作", new BusinessException(builder.ToString(), ex));
            }
            catch (DbUpdateException exc)
            {
                //抛出内部异常，开发更容易看懂
                Exception temp = exc;
                while (temp.InnerException != null)
                {
                    temp = temp.InnerException;
                }
                if (temp != null)
                {
                    if (temp.Message.StartsWith("Duplicate entry"))
                    {
                        string msg = temp.Message.Substring(temp.Message.IndexOf("'") + 1);
                        throw new BusinessException(msg.Substring(0, msg.IndexOf("'")) + "数据重复", exc);
                    }
                    if (temp.Message.StartsWith("Unable to determine the principal end of the"))
                        throw new BusinessException("数据" + Regex.Match(temp.Message, "'.*?'") + "重复", exc);

                    var regex = new Regex(@"违反了 PRIMARY KEY 约束.*?重复键值为 \((?<key>.*?)\)");
                    var match = regex.Match(temp.Message);
                    if (match.Success)
                    {
                        throw new BusinessException(match.Groups[1].Value + "数据重复", exc);
                    }
                    if (temp.Message.Contains("PK_") && (temp.Message.Contains("PRIMARY KEY") || temp.Message.Contains("违反唯一约束条件")))
                        throw new BusinessException("数据重复", exc);
                    if (temp.Message.Contains("foreign key") || temp.Message.Contains("FOREIGN KEY") || temp.Message.Contains("REFERENCE"))
                        throw new BusinessException("有外部数据引用", exc);
                    // 如果不是前面已罗列的异常，则抛出万金油异常给用户看 Black 2017-1-3 12:00:22
                    throw new BusinessException("数据提交出错", exc);
                }
            }
            catch (Exception e)//Add By Black 2016年12月19日16:00:59
            {
                throw new BusinessException("数据提交出错。", e);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // clear repositories
            if (repositories != null)
            {
                repositories.Clear();
            }

            // dispose the db context.
            DbContext.Dispose();
        }
    }
}