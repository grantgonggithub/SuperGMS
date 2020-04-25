//
// 文件：SearchParameters.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29

#region

using System;
using System.Runtime.Serialization;

#endregion

namespace GrantMicroService.DB.EFEx.DynamicSearch
{
    using GrantMicroService.DB.EFEx.DynamicSearch.Model;
    using GrantMicroService.ExceptionEx;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    ///     查询参数，用来动态构建查询表达式树的实体
    ///     add by Grant 2014-3-27
    /// </summary>
    [Serializable]
    [DataContract]
    [KnownType(typeof(string[]))]
    [KnownType(typeof(int[]))]
    public class SearchParameters
    {
        /// <summary>
        ///     构造一个空的查询条件
        /// </summary>
        public SearchParameters()
        {
            QueryModel = new QueryModel();
            PageInfo = new PageInfo
            {
                CurrentPage = 1,
                IsPaging = true,
                PageSize = 20,
                SortDirection = "",
                SortField = "",
            };
        }

        /// <summary>
        ///     通过使用JqGrid传递过来的参数，构建一个查询条件
        /// </summary>
        /// <param name="toolBarSearch">JqGrid 查询时提交的参数实体<see cref="QtGrid.JqGridToolBarSearch" /></param>
        //public SearchParameters(Object toolBarSearch)
        //{
        //    QueryModel = new QueryModel();
        //    if (toolBarSearch != null)
        //    {
        //        //组织多选条件
        //        if (toolBarSearch.groups != null && toolBarSearch.groups.Any())
        //        {
        //            foreach (var gp in toolBarSearch.groups)
        //            {
        //                foreach (QtGrid.JqGridToolBarSearch.rule item in gp.rules)
        //                {
        //                    item.data = item.data.Trim();
        //                    if (item.data.Contains("-Default-")) continue;
        //                    if (item.data.Contains("-Blank-"))
        //                    {
        //                        QueryModel.Items.Add(new ConditionItem
        //                        {
        //                            Field = item.field,
        //                            Method = QueryMethod.Equal,
        //                            Value = null,
        //                            OrGroup = item.field
        //                        });
        //                        QueryModel.Items.Add(new ConditionItem
        //                        {
        //                            Field = item.field,
        //                            Method = QueryMethod.Equal,
        //                            Value = "",
        //                            OrGroup = item.field
        //                        });
        //                    }
        //                    else
        //                    {
        //                        QueryModel.Items.Add(new ConditionItem
        //                        {
        //                            Field = item.field,
        //                            Method = QueryMethod.Equal,
        //                            Value = item.data,
        //                            OrGroup = item.field
        //                        });
        //                    }
        //                }
        //            }

        //        }

        //        foreach (QtGrid.JqGridToolBarSearch.rule item in toolBarSearch.rules)
        //        {
        //            item.data = item.data.Trim();
        //            if (item.data.Contains("-Default-")) continue;
        //            if (item.data.Contains("/") && item.data.Contains("~"))
        //            {
        //                //日期类型需要通过* / ~ 拆分成> < 的两个查询条件，例如2014/03/27~*
        //                string[] datas = item.data.Split('~');
        //                if (datas[0] != "*")
        //                {
        //                    var startTime = new ConditionItem
        //                    {
        //                        Field = item.field,
        //                        Method = QueryMethod.GreaterThanOrEqual,
        //                        Value = "20" + datas[0] + ":00"
        //                    };
        //                    QueryModel.Items.Add(startTime);
        //                }
        //                if (datas[1] != "*")
        //                {
        //                    var endTime = new ConditionItem
        //                    {
        //                        Field = item.field,
        //                        Method = QueryMethod.LessThan,
        //                        Value = "20" + datas[1] + ":00"
        //                    };
        //                    QueryModel.Items.Add(endTime);
        //                }
        //            }
        //            else if (item.data.Contains("-Blank-"))
        //            {
        //                var item1 = new ConditionItem
        //                {
        //                    Field = item.field,
        //                    Method = QueryMethod.Equal,
        //                    Value = null,
        //                    OrGroup = item.field
        //                };
        //                var item2 = new ConditionItem
        //                {
        //                    Field = item.field,
        //                    Method = QueryMethod.Equal,
        //                    Value = "",
        //                    OrGroup = item.field
        //                };
        //                QueryModel.Items.Add(item1);
        //                QueryModel.Items.Add(item2);
        //            }
        //            else if (item.op == "mcn")
        //            {
        //                //多包含处理逻辑,将一个查询条件改为多个 contains 查询
        //                var values = item.data.Split(',');
        //                var guid = Guid.NewGuid().ToString("N");
        //                foreach (var value in values)
        //                {
        //                    var condition = new ConditionItem
        //                    {
        //                        Field = item.field,
        //                        Method = QueryMethod.Contains,
        //                        Value = value,
        //                        OrGroup = guid
        //                    };
        //                    QueryModel.Items.Add(condition);
        //                }
        //            }
        //            else
        //            {
        //                var condition = new ConditionItem
        //                {
        //                    Field = item.field,
        //                    Method = string.IsNullOrEmpty(item.op) ? QueryMethod.Contains : GetMethodByJqGridOp(item.op),
        //                    Value = item.data
        //                };

        //                //如果查询条件为其中之一时，需要将value值改为array类型,其中之一查询条件时，值用逗号隔开
        //                if (condition.Method == QueryMethod.StdIn || condition.Method == QueryMethod.StdNotIn)
        //                {
        //                    condition.Value = item.data.Split(',');
        //                }
        //                //如果查询条件为不包含时，需要将null值加到查询条件中
        //                if (item.op.Equals("nc"))
        //                {
        //                    condition.OrGroup = item.field;
        //                    var item1 = new ConditionItem
        //                    {
        //                        Field = item.field,
        //                        Method = QueryMethod.Equal,
        //                        Value = null,
        //                        OrGroup = item.field
        //                    };
        //                    QueryModel.Items.Add(item1);
        //                }
        //                QueryModel.Items.Add(condition);
        //            }
        //        }

        //        if (QueryModel.Items.Count > 500) throw new BusinessException("查询条件集合中的元素个数超过500，查询终止");
        //        foreach (ConditionItem it in QueryModel.Items)
        //        {
        //            if (it.Value?.GetType() == typeof(string[]))//是string[] 表示是QueryMethod.StdIn
        //            {
        //                string[] li = it.Value as string[];
        //                if (li.Length > 500) throw new BusinessException("查询条件集合中的元素个数超过500，查询终止");
        //            }
        //        }
        //    }
        //}

        /// <summary>
        ///     具体的查询条件
        /// </summary>
        [DataMember]
        public QueryModel QueryModel { get; set; }

        /// <summary>
        ///     分页参数
        /// </summary>
        [DataMember]
        public PageInfo PageInfo { get; set; }

        /// <summary>
        ///     根据JqGrid 的过滤参数转换成查询条件枚举
        ///     [{ oper:'eq', text:'equal'},{ oper:'ne', text:'not equal'},{ oper:'lt', text:'less'},
        ///     { oper:'le', text:'less or equal'},{ oper:'gt', text:'greater'},{ oper:'ge', text:'greater or equal'},
        ///     { oper:'bw', text:'begins with'},{ oper:'bn', text:'does not begin with'},{ oper:'in', text:'is in'},
        ///     { oper:'ni', text:'is not in'},{ oper:'ew', text:'ends with'},{ oper:'en', text:'does not end with'},
        ///     { oper:'cn', text:'contains'},{ oper:'nc', text:'does not contain'}]
        /// 补充
        ///      { oper:'mcn', text:'contains'},{ oper:'mcn', text:'more like'}
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        private QueryMethod GetMethodByJqGridOp(string op)
        {
            switch (op)
            {
                case "eq":
                    return QueryMethod.Equal;

                case "ne":
                    return QueryMethod.NotEqual;

                case "lt":
                    return QueryMethod.LessThan;

                case "le":
                    return QueryMethod.LessThanOrEqual;

                case "gt":
                    return QueryMethod.GreaterThan;

                case "ge":
                    return QueryMethod.GreaterThanOrEqual;

                case "bw":
                    return QueryMethod.StartsWith;

                case "in":
                    return QueryMethod.StdIn;

                case "ni":
                    return QueryMethod.StdNotIn;

                case "ew":
                    return QueryMethod.EndsWith;

                case "cn":
                    return QueryMethod.Contains;

                case "nc":
                    return QueryMethod.NotLike;

                default:
                    throw new BusinessException("不支持此查询条件");
            }
        }

        /// <summary>
        /// 根据QueryModel组织sqlWhere语句,如果有字段前缀的话,需要提前增加进来
        /// </summary>
        /// <returns></returns>
        public string GetSqlWhere()
        {
            var sb = new StringBuilder();
            List<string> groups = new List<string>();
            foreach (var conditionItem in QueryModel.Items)
            {
                string sqlWhere = string.Empty;
                if (!string.IsNullOrEmpty(conditionItem.OrGroup))
                {
                    if (!groups.Contains(conditionItem.OrGroup))
                    {
                        var sbChild = new StringBuilder();
                        foreach (var senItem in QueryModel.Items)
                        {
                            if (senItem.OrGroup == conditionItem.OrGroup)
                            {
                                if (sbChild.Length > 0)
                                {
                                    sbChild.Append(" or ");
                                }
                                sbChild.Append((string.IsNullOrEmpty(senItem.Prefix) ? "" : (senItem.Prefix + ".")) + senItem.Field + " " + ConvertMethodToSql(senItem.Method, senItem.Value));
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
        /// 获取QueryModel的参数列表
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetParameters()
        {
            var paraDic = new Dictionary<string, object>();
            for (int i = 0; i < QueryModel.Items.Count; i++)
            {
                var item = QueryModel.Items[i];
                switch (item.Method)
                {
                    /*
                      *   sqlText += " and Name like @Name";
                            p.Add("Name","%"+ model.Name+"%");
                      */
                    case QueryMethod.Contains:
                    case QueryMethod.NotLike:
                        paraDic.Add(item.Field + i, "%" + item.Value + "%");
                        break;

                    case QueryMethod.StartsWith:
                        paraDic.Add(item.Field + i, item.Value + "%");
                        break;

                    case QueryMethod.EndsWith:
                        paraDic.Add(item.Field + i, "%" + item.Value);
                        break;
                    /*
                     * string sql = "SELECT * FROM SomeTable WHERE id IN @ids"
                        var results = conn.Query(sql, new { ids = new[] { 1, 2, 3, 4, 5 }});
                     */
                    case QueryMethod.StdIn:
                    case QueryMethod.StdNotIn:
                    case QueryMethod.GreaterThan:
                    case QueryMethod.GreaterThanOrEqual:
                    case QueryMethod.Equal:
                    case QueryMethod.LessThan:
                    case QueryMethod.LessThanOrEqual:
                    case QueryMethod.NotEqual:
                        paraDic.Add(item.Field + i, item.Value);
                        break;

                    default:
                        paraDic.Add(item.Field + i, item.Value);
                        break;
                }
            }
            return paraDic;
        }

        /// <summary>
        /// 将Method转换成sql语法的查询语句
        /// </summary>
        /// <param name="method"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ConvertMethodToSql(QueryMethod method, object value)
        {
            switch (method)
            {
                ////字符串类型处理
                case QueryMethod.Contains:
                    return "like '%" + value + "%'";

                case QueryMethod.StdIn:
                    return "in ('" + string.Join("','", value as string[]) + "')";

                case QueryMethod.NotLike:
                    return "not like '%" + value + "%'";
                ////数字类型处理
                case QueryMethod.GreaterThan:
                    return "> '" + value + "'";

                case QueryMethod.GreaterThanOrEqual:
                    return ">= '" + value + "'";

                case QueryMethod.Equal:
                    return "= '" + value + "'";

                case QueryMethod.LessThan:
                    return "< '" + value + "'";

                case QueryMethod.LessThanOrEqual:
                    return "<= '" + value + "'";

                case QueryMethod.StdNotIn:
                    return "not in  ('" + string.Join("','", value as string[]) + "')";

                case QueryMethod.NotEqual:
                    return "<> '" + value + "'";

                case QueryMethod.StartsWith:
                    return "like '" + value + "%'";

                case QueryMethod.EndsWith:
                    return "like '%" + value + "'";

                default:
                    return "";
            }
        }
        /// <summary>
        /// 处理空值查询，null值查询
        /// </summary>
        public void BuildEmptySearch()
        {
            if (this.QueryModel != null && this.QueryModel.Items != null && this.QueryModel.Items.Count > 0)
            {
                foreach (var item in this.QueryModel.Items.ToArray())
                {
                    //处理空值查询, 不需要处理已分组的查询条件
                    if (item.Value != null && item.Value.ToString() == "" && string.IsNullOrEmpty(item.OrGroup) && item.Method == QueryMethod.Equal)
                    {
                        this.QueryModel.Items.Remove(item);
                        var newGroupId = Guid.NewGuid().ToString("N");
                        this.QueryModel.Items.Add(new ConditionItem
                        {
                            Field = item.Field,
                            Method = QueryMethod.Equal,
                            Value = null,
                            Prefix = item.Prefix,
                            OrGroup = newGroupId
                        });
                        this.QueryModel.Items.Add(new ConditionItem
                        {
                            Field = item.Field,
                            Method = QueryMethod.Equal,
                            Value = "",
                            Prefix = item.Prefix,
                            OrGroup = newGroupId
                        });
                    }
                    //处理不等于，不包含 算上null值 , 不需要处理已分组的查询条件
                    if ((item.Method == QueryMethod.NotEqual || item.Method == QueryMethod.NotLike) && string.IsNullOrEmpty(item.OrGroup))
                    {
                        this.QueryModel.Items.Remove(item);
                        item.OrGroup = Guid.NewGuid().ToString("N");
                        this.QueryModel.Items.Add(item);

                        if (item.Value != null && !IsSimple(item.Value.GetType()))
                        {
                            //不是简单类型，并且不为空值，才加上null判断
                            this.QueryModel.Items.Add(new ConditionItem
                            {
                                Field = item.Field,
                                Method = QueryMethod.Equal,
                                Value = null,
                                Prefix = item.Prefix,
                                OrGroup = item.OrGroup
                            });
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 是否是简单类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Nullable 不是简单类型
                return false;
            }
            return type.IsPrimitive
                   || type.IsEnum
                   || type.Equals(typeof(string))
                   || type.Equals(typeof(decimal));
        }
    }
}