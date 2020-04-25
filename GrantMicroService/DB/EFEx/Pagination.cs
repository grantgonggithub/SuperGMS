#region

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

#endregion

namespace GrantMicroService.DB.EFEx
{
    using GrantMicroService.DB.EFEx.DynamicSearch;
    using GrantMicroService.DB.EFEx.DynamicSearch.Model;
    using System.Collections.Generic;

    /// <summary>
    ///     分页类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pagination<T> where T : class
    {
        /// <summary>
        ///     分页，就是将已经整理的查询条件 IQueryable 再加上 skip，take 方法
        /// </summary>
        /// <param name="query"></param>
        /// <param name="skipCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort">排序字段,如果是多字段排序,此字段记录为 "ID ASC,Name Desc" </param>
        /// <param name="dir">ASC/DESC,如果是多字段排序,此字段为空</param>
        /// <param name="count">总纪录数</param>
        /// <param name="isGetTotalCount">是否获取总页数，默认是true</param>
        /// <returns></returns>
        public static IQueryable<T> PageList(IQueryable<T> query, int skipCount, int pageSize, string sort, string dir,
            out int count, bool isGetTotalCount = true)
        {
            //如果是获取总记录数，则不需要排序
            if (isGetTotalCount)
            {
                try
                {
                    count = query.Count();
                }
                catch (Exception e)
                {
                    // LogEx.LogError(e);
                    count = 100000;
                }
            }
            else
            {
                count = 100000;
            }

            IOrderedQueryable<T> iOrderedQueryable = OrderedQueryable(query, sort, dir);
            // skipCount 必须大于等于0
            skipCount = skipCount > 0 ? skipCount : 0;
            if (pageSize > 0)
            {
                // 通过总条数检查当前页是有效(有数据), 如果没数据,则自动将页码设置为最后一页
                if (count > 0 && skipCount >= count)
                {
                    skipCount = skipCount > pageSize ? skipCount : pageSize;
                }

                query = iOrderedQueryable.Skip(skipCount).Take(pageSize);
            }
            else
            {
                query = query.Take(0);
            }

            return query.AsNoTracking();
        }

        /// <summary>
        /// 构建排序
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static IOrderedQueryable<T> OrderedQueryable(IQueryable<T> query, string sort, string dir)
        {
            IOrderedQueryable<T> iOrderedQueryable = null;
            if (string.IsNullOrEmpty(dir))
            {
                if (!string.IsNullOrEmpty(sort))
                {
                    //如果是多字段排序，则会把排序字段和排序方式记录到sort上，dir为空
                    char[] delimiters = { ',' };
                    string[] sorts = sort.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < sorts.Length; i++)
                    {
                        string[] values = sorts[i].Trim().Split(' ');
                        string sortName = values[0];
                        string sortDir = values.Length == 2 ? values[1] : "ASC";
                        iOrderedQueryable = i == 0
                            ? query.OrderBy(sortName + " " + sortDir)
                            : iOrderedQueryable.ThenBy(sortName + " " + sortDir);
                    }
                }
                else
                {
                    //如果没有传递任何排序字段,则按第一个字段排序
                    //ReflectionTool.GetPropertyInfosFromCache(typeof(T))[0].Name + " ASC"
                    iOrderedQueryable = query as IOrderedQueryable<T>;
                }
            }
            else
            {
                iOrderedQueryable = query.OrderBy(sort + " " + dir);
            }
            return iOrderedQueryable;
        }

        /// <summary>
        /// 对List进行分页, 此方法会强制大小写匹配
        /// </summary>
        /// <typeparam name="Entity"></typeparam>
        /// <param name="seachParameters">查询条件</param>
        /// <param name="listProvince">泛型集合</param>
        /// <returns></returns>
        public static List<Entity> PagingListData<Entity>(SearchParameters seachParameters, List<Entity> listProvince) where Entity : class
        {
            PageInfo page = seachParameters.PageInfo;
            seachParameters.QueryModel = seachParameters.QueryModel ?? new QueryModel();
           
            ////这里需要转换一下查询条件, 将不等于null的判断加入到QueryModel中,否则会抛出NullReferenceException异常
            var notNullConditions = new ConditionItem[seachParameters.QueryModel.Items.Count];
            seachParameters.QueryModel.Items.CopyTo(notNullConditions);
            foreach (ConditionItem conditionItem in notNullConditions)
            {
                var temp = new ConditionItem { Field = conditionItem.Field, Method = QueryMethod.NotEqual, Value = null };
                seachParameters.QueryModel.Items.Insert(0, temp);
            }
           
           
            int count;
            var srclist =
                Pagination<Entity>.PageList(
                    listProvince.AsQueryable().Where(seachParameters.QueryModel),
                    page.SkipCount == 0 ? (page.CurrentPage - 1) * page.PageSize : page.SkipCount,
                    page.PageSize,
                    page.SortField,
                    page.SortDirection,
                    out count).ToList();
            page.TotalCount = count;
            return srclist;
        }
    }
}