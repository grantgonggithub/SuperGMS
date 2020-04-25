//
// 文件：QueryableExtensions.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GrantMicroService.DB.EFEx.DynamicSearch.Model;

#endregion

namespace GrantMicroService.DB.EFEx.DynamicSearch
{
    /// <summary>
    ///     对IQueryable的扩展方法
    ///     add by Grant 2014-3-27
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        ///     重载IQueryable 的Where方法，以便支持QueryModel 参数
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="table">IQueryable的查询对象</param>
        /// <param name="model">QueryModel对象</param>
        /// <param name="prefix">使用前缀区分查询条件</param>
        /// <returns></returns>
        public static IQueryable<TEntity> Where<TEntity>(this IQueryable<TEntity> table, QueryModel model,
            string prefix = "") where TEntity : class
        {
            Contract.Requires(table != null);
            return Where(table, model.Items, prefix);
        }
        /// <summary>
        ///     重载IQueryable 的Where方法，以便支持QueryModel 参数
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="table">IQueryable的查询对象</param>
        /// <returns></returns>
        public static IQueryable<TEntity> DomainFilter<TEntity>(this IQueryable<TEntity> table)
        {
            Contract.Requires(table != null);
            var searchParameters = new SearchParameters();
            ForceFilterCondition.FilterByTenantDomainWearHouse<TEntity>(searchParameters);
            return Where(table, searchParameters.QueryModel.Items, "");
        }
        /// <summary>
        ///     排序
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="source">要排序的数据源</param>
        /// <param name="value">排序依据（加空格）排序方式</param>
        /// <returns>IOrderedQueryable</returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string value)
        {
            string[] arr = value.Split(' ');
            string Name = arr[1].ToUpper() == "DESC" ? "OrderByDescending" : "OrderBy";
            return ApplyOrder(source, arr[0], Name);
        }

        /// <summary>
        ///     Linq动态排序再排序
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="source">要排序的数据源</param>
        /// <param name="value">排序依据（加空格）排序方式</param>
        /// <returns>IOrderedQueryable</returns>
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string value)
        {
            string[] arr = value.Split(' ');
            string Name = arr[1].ToUpper() == "DESC" ? "ThenByDescending" : "ThenBy";
            return ApplyOrder(source, arr[0], Name);
        }

        /// <summary>
        ///     排序方法
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="source">集合</param>
        /// <param name="property">字段名</param>
        /// <param name="methodName">排序方法</param>
        /// <returns>排过序的对象集合</returns>
        private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            Type type = typeof (T);
            ParameterExpression arg = Expression.Parameter(type, "a");
            PropertyInfo pi = type.GetProperty(property);
            if (pi == null)
            {
                return (IOrderedQueryable<T>)source;
            }

            Expression expr = Expression.Property(arg, pi);
            type = pi.PropertyType;
            Type delegateType = typeof (Func<,>).MakeGenericType(typeof (T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);
            object result = typeof (Queryable).GetMethods().Single(
                a => a.Name == methodName
                     && a.IsGenericMethodDefinition
                     && a.GetGenericArguments().Length == 2
                     && a.GetParameters().Length == 2)
                .MakeGenericMethod(typeof (T), type)
                .Invoke(null, new object[] {source, lambda});
            return (IOrderedQueryable<T>) result;
        }

        /// <summary>
        ///     内部方法提供给Where方法调用，使用了 prefix参数过滤查询条件
        /// </summary>
        /// <typeparam name="T">实体对象</typeparam>
        /// <param name="table">集合</param>
        /// <param name="items">查询条件集合</param>
        /// <param name="prefix">属性前缀，如果使用了属性前缀则只会过滤满足属性前缀的条件集合</param>
        /// <returns>对象集合</returns>
        private static IQueryable<T> Where<T>(IQueryable<T> table, IEnumerable<ConditionItem> items, string prefix = "")
        {
            Contract.Requires(table != null);
            IEnumerable<ConditionItem> filterItems =
                string.IsNullOrWhiteSpace(prefix)
                    ? items.Where(c => string.IsNullOrEmpty(c.Prefix))
                    : items.Where(c => c.Prefix == prefix);
            if (filterItems.Count() == 0) return table;
            return new QueryableSearcher<T>(table, filterItems).Search();
        }
    }
}