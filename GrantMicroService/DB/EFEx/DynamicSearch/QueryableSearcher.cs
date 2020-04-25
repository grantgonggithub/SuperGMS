//
// 文件：QueryableSearcher.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GrantMicroService.DB.EFEx.DynamicSearch.Model;
using GrantMicroService.ExceptionEx;
using GrantMicroService.Tools;

#endregion

namespace GrantMicroService.DB.EFEx.DynamicSearch
{
    /// <summary>
    ///     查询方法，提供给 <see cref="GrantMicroService.EFEx.DynamicSearch.QueryableExtensions" />调用
    ///     add by Grant 2014-3-27
    /// </summary>
    /// <typeparam name="T">对象</typeparam>
    internal class QueryableSearcher<T>
    {
        /// <summary>
        ///     组织最小的表达式用的连接符字典,left 是属性表达式，Right 是值
        /// </summary>
        private static readonly Dictionary<QueryMethod, Func<Expression, Expression, Expression>> ExpressionDict =
            new Dictionary<QueryMethod, Func<Expression, Expression, Expression>>
            {
                {
                    QueryMethod.Equal,
                    (left, right) => { return Expression.Equal(left, right); }
                },
                {
                    QueryMethod.GreaterThan,
                    (left, right) => { return Expression.GreaterThan(left, right); }
                },
                {
                    QueryMethod.GreaterThanOrEqual,
                    (left, right) => { return Expression.GreaterThanOrEqual(left, right); }
                },
                {
                    QueryMethod.LessThan,
                    (left, right) => { return Expression.LessThan(left, right); }
                },
                {
                    QueryMethod.LessThanOrEqual,
                    (left, right) => { return Expression.LessThanOrEqual(left, right); }
                },
                {
                    QueryMethod.Contains,
                    (left, right) =>
                    {
                        if (left.Type != typeof (string)) return null;
                        return Expression.Call(left, typeof (String).GetMethod("Contains", new Type[]{ typeof(string)}), right);
                    }
                },
                {
                    QueryMethod.StdIn,
                    (left, right) =>
                    {
                        if (!right.Type.IsArray) return null;
                        //调用Enumerable.Contains扩展方法
                        MethodCallExpression resultExp =
                            Expression.Call(
                                typeof (Enumerable),
                                "Contains",
                                new[] {left.Type},
                                right,
                                left);

                        return resultExp;
                    }
                },
                {
                    QueryMethod.StdNotIn,
                    (left, right) =>
                    {
                        if (!right.Type.IsArray) return null;

                        //调用Enumerable.Contains扩展方法
                        //MethodCallExpression resultExp =
                        //    Expression.Call(
                        //        typeof (Enumerable),
                        //        "Contains",
                        //        new[] {left.Type},
                        //        right,
                        //        left);

                        return Expression.Not(Expression.Call(typeof(Enumerable),"Contains",new[] { left.Type },right,left));
                        //return resultExp;
                    }
                },
                {
                    QueryMethod.NotEqual,
                    (left, right) => { return Expression.NotEqual(left, right); }
                },
                {
                    QueryMethod.NotLike,
                    (left, right) => { return Expression.Not( Expression.Call(left, typeof (String).GetMethod("Contains", new Type[]{ typeof(string)}), right)); }
                },
                {
                    QueryMethod.StartsWith,
                    (left, right) =>
                    {
                        if (left.Type != typeof (string)) return null;
                        return Expression.Call(left, typeof (string).GetMethod("StartsWith", new[] {typeof (string)}),
                            right);
                    }
                },
                {
                    QueryMethod.EndsWith,
                    (left, right) =>
                    {
                        if (left.Type != typeof (string)) return null;
                        return Expression.Call(left, typeof (string).GetMethod("EndsWith", new[] {typeof (string)}),
                            right);
                    }
                }
            };

        /// <summary>
        ///     空构造方法
        /// </summary>
        public QueryableSearcher()
        {
        }

        /// <summary>
        ///     带查询内容和查询条件的构造方法
        /// </summary>
        /// <param name="table">待查询内容集合</param>
        /// <param name="items">查询条件</param>
        public QueryableSearcher(IQueryable<T> table, IEnumerable<ConditionItem> items)
            : this()
        {
            Table = table;
            Items = items;
        }

        private IEnumerable<ConditionItem> _items;

        /// <summary>
        ///     查询条件
        /// </summary>
        protected IEnumerable<ConditionItem> Items
        {
            set
            {
                Type t = typeof(T);
                List<ConditionItem> newItems = new List<ConditionItem>();
                foreach (var it in value)
                {
                    if (it.Value == null || it.Value.ToString() == "")
                    {
                        PropertyInfo Pinfo = t.GetProperty(it.Field);
                        if (Pinfo == null) throw new BusinessException(it.Field + "不存在目标对象,不能构建查询条件");
                        if (Pinfo.PropertyType.IsPrimitive) throw new BusinessException(it.Field + "属于简单类型,不存在空值和NULL值,不能构建查询条件");//简单类型为空直接跳过
                        newItems.Add(it);
                    }
                    else
                        newItems.Add(it);
                }
                _items = newItems;
            }
            get
            {
                return _items;
            }
        }

        /// <summary>
        ///     查询源
        /// </summary>
        protected IQueryable<T> Table { get; set; }

        /// <summary>
        ///     查询调用方法，构造完参数后调用此方法可以获取查询结果
        /// </summary>
        /// <returns>待查询集合</returns>
        public IQueryable<T> Search()
        {
            //构建 c=>Body中的c
            ParameterExpression param = Expression.Parameter(typeof(T), "c");
            //构建c=>Body中的Body
            Expression body = GetExpressoinBody(param, Items);
            //将二者拼为c=>Body
            Expression<Func<T, bool>> expression = Expression.Lambda<Func<T, bool>>(body, param);
            //传到Where中当做参数，类型为Expression<Func<T,bool>>
            return Table.Where(expression);
        }

        /// <summary>
        ///     构建表达式主体
        /// </summary>
        /// <param name="param">表达式命名</param>
        /// <param name="items">查询条件</param>
        /// <returns>表达式主体</returns>
        private Expression GetExpressoinBody(ParameterExpression param, IEnumerable<ConditionItem> items)
        {
            var list = new List<Expression>();
            //OrGroup为空的情况下，即为And组合
            IEnumerable<ConditionItem> andList = items.Where(c => string.IsNullOrEmpty(c.OrGroup));
            //将And的子Expression以AndAlso拼接
            if (andList.Count() != 0)
            {
                list.Add(GetGroupExpression(param, andList, Expression.AndAlso));
            }

            //OrGroup 中包含逗号的,即为 Or And 组合 ((A='1' and A='2') or (A='2' and  A='3') or (A='3')) and  ((B='1' and B='2') or (B='2' and  B='3'))
            // x,1 和 x,1 和 x,2 和 x,2 和 x 和  y,1 和 y,1 和y,2 和 y,2
            var orAndGrups = items.Where(c => !string.IsNullOrEmpty(c.OrGroup) && c.OrGroup.Contains(',')).GroupBy(c => c.OrGroup);

            //将And的子Expression以AndAlso拼接
            var childAndExp = new Dictionary<string, Expression>();
            var childAndPrefix = new List<string>();
            foreach (var orAnd in orAndGrups)
            {
                //先组织orAnd 再在Or里面拼接
                if (orAnd.Count() != 0)
                {
                    childAndExp.Add(orAnd.Key, GetGroupExpression(param, orAnd, Expression.AndAlso));
                    var prefix = orAnd.Key.Split(',')[0];
                    if (!childAndPrefix.Contains(prefix))
                    {
                        childAndPrefix.Add(prefix);
                    }
                }
            }

            //把OrGroup没逗号的 使用前缀匹配逻辑 用 or 拼接起来
            IEnumerable<IGrouping<string, ConditionItem>> orGroupByList =
                items.Where(c => !string.IsNullOrEmpty(c.OrGroup) && !c.OrGroup.Contains(',')).GroupBy(c => c.OrGroup);
            //拼接子Expression的Or关系
            foreach (var group in orGroupByList)
            {
                if (group.Count() != 0)
                {
                    Expression exp = GetGroupExpression(param, group, Expression.OrElse);
                    foreach (var expression in childAndExp)
                    {
                        if (expression.Key.StartsWith(group.Key + ","))
                        {
                            if (exp != null)
                            {
                                exp = new[] { exp, expression.Value }
                               .Aggregate(Expression.OrElse);
                            }
                        }
                    }
                    childAndPrefix.Remove(group.Key);
                    list.Add(exp);
                }
            }
            ////将剩余的 前缀相同的 and组合 用 or拼接
            foreach (var prefix in childAndPrefix)
            {
                var exps = childAndExp.Where(a => a.Key.StartsWith(prefix + ",")).Select(a => a.Value).ToList();
                var exp = exps[0];
                for (int i = 1; i < exps.Count; i++)
                {
                    exp = new[] { exp, exps[i] }
                               .Aggregate(Expression.OrElse);
                }
                list.Add(exp);
            }
            //将这些Expression再以And相连
            return list.Aggregate(Expression.AndAlso);
        }

        /// <summary>
        ///     获取分组表达式
        /// </summary>
        /// <param name="param">表达式命名</param>
        /// <param name="items">查询条件集合</param>
        /// <param name="func">操作符</param>
        /// <returns>分组表达式</returns>
        private Expression GetGroupExpression(ParameterExpression param, IEnumerable<ConditionItem> items,
            Func<Expression, Expression, Expression> func)
        {
            //获取最小的判断表达式
            IEnumerable<Expression> list = items.Select(item => GetExpression(param, item)).Where(x => x != null);
            //再以逻辑运算符相连
            return list.Aggregate(func);
        }

        /// <summary>
        ///     获取最小的表达式组合
        /// </summary>
        /// <param name="param">表达式命名</param>
        /// <param name="item">查询条件</param>
        /// <returns>最小的表达式组合</returns>
        private Expression GetExpression(ParameterExpression param, ConditionItem item)
        {
            //属性表达式
            LambdaExpression exp = GetPropertyLambdaExpression(item, param);
            if (exp == null) return null;
            //常量表达式
            Expression constant = ChangeTypeToExpression(item, exp.Body.Type);
            //以判断符或方法连接
            return ExpressionDict[item.Method](exp.Body, constant);
        }

        /// <summary>
        ///     构建属性表达式,支持多级拆分 如：c.Users.Proiles.UserId
        /// </summary>
        /// <param name="item">查询条件</param>
        /// <param name="param">属性表达式</param>
        /// <returns>Lambda表达式</returns>
        private LambdaExpression GetPropertyLambdaExpression(ConditionItem item, ParameterExpression param)
        {
            string[] props = item.Field.Split('.');
            Expression propertyAccess = param;
            Type typeOfProp = typeof(T);
            int i = 0;
            do
            {
                PropertyInfo property = typeOfProp.GetProperty(props[i]);
                if (property == null) return null;
                typeOfProp = property.PropertyType;
                propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                i++;
            } while (i < props.Length);

            return Expression.Lambda(propertyAccess, param);
        }

        #region ChangeType

        /// <summary>
        ///     类型转换，支持非空类型与可空类型之间的转换
        /// </summary>
        /// <param name="value">原始对象</param>
        /// <param name="conversionType">目标对象类型</param>
        /// <returns>转换后的对象</returns>
        public static object ChangeType(object value, Type conversionType)
        {
            if (value == null) return null;
            return Convert.ChangeType(value, TypeUtil.GetUnNullableType(conversionType));
        }

        /// <summary>
        ///     转换SearchItem中的Value的类型，为表达式树
        /// </summary>
        /// <param name="item">查询条件</param>
        /// <param name="conversionType">目标类型</param>
        public static Expression ChangeTypeToExpression(ConditionItem item, Type conversionType)
        {
            //if (item.Value == null||item.Value.ToString()=="")
            //{
            //    if (conversionType.IsPrimitive) return null;
            //    return Expression.Constant(item.Value, conversionType);
            //}
            if (item.Value == null) return Expression.Constant(item.Value, conversionType);

            #region 数组

            if (item.Method == QueryMethod.StdIn || item.Method == QueryMethod.StdNotIn)
            {
                Array arr = null;
                if (item.Value.GetType().IsGenericType)
                {
                    ////兼容泛型类型
                    if (item.Value is List<int>)
                    {
                        arr = ((List<int>)item.Value).ToArray();
                    }
                    else if (item.Value is List<string>)
                    {
                        arr = ((List<string>)item.Value).ToArray();
                    }
                }
                else
                {
                    arr = (item.Value as Array);
                }

                var expList = new List<Expression>();
                if (arr != null)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        //构造数组的单元Constant
                        object newValue = ChangeType(arr.GetValue(i), conversionType);
                        expList.Add(Expression.Constant(newValue, conversionType));
                    }
                }
                //构造inType类型的数组表达式树，并为数组赋初值
                return Expression.NewArrayInit(conversionType, expList);
            }

            #endregion 数组

            Type elementType = TypeUtil.GetUnNullableType(conversionType);
            object value = Convert.ChangeType(item.Value, elementType);
            return Expression.Constant(value, conversionType);
        }

        #endregion ChangeType
    }
}