/* 
 * Copyright 2019 Castle
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *  http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
 * either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using SuperGMS.Tools;

namespace SuperGMS.DB.EFEx.QueryFilterRule
{
    /// <summary>
    /// Generic IQueryable filter implementation.  Based upon configuration of FilterRules 
    /// mapping to the data source.  When applied, acts as an advanced filter mechanism.
    /// </summary>
    public static class QueryBuilder
    {
        /// <summary>
        /// Gets or sets a value indicating whether incoming dates in the filter should be parsed as UTC.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [parse dates as UTC]; otherwise, <c>false</c>.
        /// </value>
        public static bool ParseDatesAsUtc { get; set; }

        private static object root = new object();
        private static Dictionary<string, List<string>> _dicTypeOperator;
        private static Dictionary<string, List<string>> DicTypeOperator
        {
            get
            {
                if (_dicTypeOperator == null)
                {
                    lock (root)
                    {
                        if (_dicTypeOperator == null)
                        {
                            // 数据类型与其支持的操作符
                            _dicTypeOperator = new Dictionary<string, List<string>>();
                            _dicTypeOperator.Add(typeof(int).Name, new List<string>
                            {
                                "in",
                                "not_in",
                                "equal",
                                "not_equal",
                                "between",
                                "not_between",
                                "less",
                                "less_or_equal",
                                "greater",
                                "greater_or_equal",
                                //"begins_with",
                                //"not_begins_with",
                                //"contains",
                                //"not_contains",
                                //"ends_with",
                                //"not_ends_with",
                                //"is_empty",
                                //"is_not_empty",
                                "is_null",
                                "is_not_null",
                                //"mc",
                                //"sdiff",
                                //"ediff"
                            }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                            _dicTypeOperator.Add(typeof(long).Name, new List<string>
                            {
                                "in",
                                "not_in",
                                "equal",
                                "not_equal",
                                "between",
                                "not_between",
                                "less",
                                "less_or_equal",
                                "greater",
                                "greater_or_equal",
                                //"begins_with",
                                //"not_begins_with",
                                //"contains",
                                //"not_contains",
                                //"ends_with",
                                //"not_ends_with",
                                //"is_empty",
                                //"is_not_empty",
                                "is_null",
                                "is_not_null",
                                //"mc",
                                //"sdiff",
                                //"ediff"
                            }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                            _dicTypeOperator.Add(typeof(decimal).Name, new List<string>
                            {
                                "in",
                                "not_in",
                                "equal",
                                "not_equal",
                                "between",
                                "not_between",
                                "less",
                                "less_or_equal",
                                "greater",
                                "greater_or_equal",
                                //"begins_with",
                                //"not_begins_with",
                                //"contains",
                                //"not_contains",
                                //"ends_with",
                                //"not_ends_with",
                                //"is_empty",
                                //"is_not_empty",
                                "is_null",
                                "is_not_null",
                                //"mc",
                                //"sdiff",
                                //"ediff"
                            }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                            _dicTypeOperator.Add(typeof(string).Name, new List<string>
                            {
                                "in",
                                "not_in",
                                "equal",
                                "not_equal",
                                //"between",
                                //"not_between",
                                //"less",
                                //"less_or_equal",
                                //"greater",
                                //"greater_or_equal",
                                "begins_with",
                                "not_begins_with",
                                "contains",
                                "not_contains",
                                "ends_with",
                                "not_ends_with",
                                "is_empty",
                                "is_not_empty",
                                "is_null",
                                "is_not_null",
                                "mc",
                                //"sdiff",
                                //"ediff"
                            }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                            _dicTypeOperator.Add(typeof(DateTime).Name, new List<string>
                            {
                                "in",
                                "not_in",
                                "equal",
                                "not_equal",
                                "between",
                                "not_between",
                                "less",
                                "less_or_equal",
                                "greater",
                                "greater_or_equal",
                                //"begins_with",
                                //"not_begins_with",
                                //"contains",
                                //"not_contains",
                                //"ends_with",
                                //"not_ends_with",
                                //"is_empty",
                                //"is_not_empty",
                                "is_null",
                                "is_not_null",
                                //"mc",
                                "sdiff",
                                "ediff"
                            }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                            _dicTypeOperator.Add(typeof(bool).Name, new List<string>
                            {
                                "in",
                                "not_in",
                                "equal",
                                "not_equal",
                                //"between",
                                //"not_between",
                                //"less",
                                //"less_or_equal",
                                //"greater",
                                //"greater_or_equal",
                                //"begins_with",
                                //"not_begins_with",
                                //"contains",
                                //"not_contains",
                                //"ends_with",
                                //"not_ends_with",
                                //"is_empty",
                                //"is_not_empty",
                                "is_null",
                                "is_not_null",
                                //"mc",
                                //"sdiff",
                                //"ediff"
                            }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                        }
                    }
                }
                return _dicTypeOperator;
            }
        }

        /// <summary>
        /// Gets the filtered collection after applying the provided filter rules.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="useIndexedProperty">Whether or not to use indexed property</param>
        /// <param name="indexedPropertyName">The indexable property to use</param>
        /// <returns>Filtered IQueryable</returns>
        public static IQueryable<T> BuildQuery<T>(this IList<T> queryable, FilterRule filterRule, bool useIndexedProperty = false, string indexedPropertyName = null)
        {
            string parsedQuery;
            return BuildQuery(queryable.AsQueryable(), filterRule, out parsedQuery, useIndexedProperty, indexedPropertyName);
        }
        public static IQueryable<T> BuildQuery<T>(this IList<T> queryable, string jsonStringFilterRule, bool useIndexedProperty = false, string indexedPropertyName = null)
        {
            return BuildQuery(queryable, JsonEx.JsonConvert.DeserializeObject<FilterRule>(jsonStringFilterRule), useIndexedProperty, indexedPropertyName);
        }
        /// <summary>
        /// Gets the filtered collection after applying the provided filter rules.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="useIndexedProperty">Whether or not to use indexed property</param>
        /// <param name="indexedPropertyName">The indexable property to use</param>
        /// <returns>Filtered IQueryable</returns>
        public static IQueryable<T> BuildQuery<T>(this IQueryable<T> queryable, FilterRule filterRule, bool useIndexedProperty = false, string indexedPropertyName = null)
        {
            string parsedQuery;
            return BuildQuery(queryable, filterRule, out parsedQuery, useIndexedProperty, indexedPropertyName);
        }

        /// <summary>
        /// Gets the filtered collection after applying the provided filter rules. 
        /// Returns the string representation for diagnostic purposes.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="parsedQuery">The parsed query.</param>
        /// <param name="useIndexedProperty">Whether or not to use indexed property</param>
        /// <param name="indexedPropertyName">The indexable property to use</param>
        /// <returns>Filtered IQueryable.</returns>
        public static IQueryable<T> BuildQuery<T>(this IQueryable<T> queryable, FilterRule filterRule, out string parsedQuery, bool useIndexedProperty = false, string indexedPropertyName = null)
        {
            if (filterRule == null || filterRule.Rules == null || filterRule.Rules.Count == 0)
            {
                parsedQuery = "";
                return queryable;
            }

            var pe = Expression.Parameter(typeof(T), "item");

            var expressionTree = BuildExpressionTree(pe, filterRule, useIndexedProperty, indexedPropertyName);
            if (expressionTree == null)
            {
                parsedQuery = "";
                return queryable;
            }

            parsedQuery = expressionTree.ToString();

            var whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new[] { queryable.ElementType },
                queryable.Expression,
                Expression.Lambda<Func<T, bool>>(expressionTree, pe));

            var filteredResults = queryable.Provider.CreateQuery<T>(whereCallExpression);

            return filteredResults;

        }

        private static Expression BuildExpressionTree(ParameterExpression pe, FilterRule rule, bool useIndexedProperty = false, string indexedPropertyName = null)
        {

            if (rule.Rules != null && rule.Rules.Any())
            {
                var expressions =
                    rule.Rules.Select(childRule => BuildExpressionTree(pe, childRule, useIndexedProperty, indexedPropertyName))
                        .Where(exp => exp != null)
                        .ToList();

                var expressionTree = expressions.First();

                var counter = 1;
                while (counter < expressions.Count)
                {
                    expressionTree = rule.Condition.ToLower() == "or"
                        ? Expression.Or(expressionTree, expressions[counter])
                        : Expression.And(expressionTree, expressions[counter]);
                    counter++;
                }

                return expressionTree;
            }

            if (string.IsNullOrEmpty(rule.Field))
            {
                throw new Exception("属性未设置");
            }

            if (string.IsNullOrEmpty(rule.Operator))
            {
                throw new Exception($"属性【{rule.Field}】运算符未设置");
            }

            Expression propertyExp = null;
            if (useIndexedProperty)
            {
                propertyExp = Expression.Property(pe, indexedPropertyName, Expression.Constant(rule.Field));
            }
            else
            {
                // QT 扩展，支持复杂属性 obj.Foo.Boo
                if (string.IsNullOrEmpty(rule.Table))
                {
                    if (pe.Type.GetProperty(rule.Field) == null)
                    {
                        throw new Exception(string.Format("对象【{0}】属性【{1}】不存在", pe.Type.Name, rule.Field));
                    }
                    propertyExp = Expression.Property(pe, rule.Field);
                }
                else
                {
                    propertyExp = Expression.Property(pe, rule.Table);

                    if (propertyExp.Type.GetProperty(rule.Field) == null)
                    {
                        throw new Exception(string.Format("对象【{0}】属性【{1}】不存在", propertyExp.Type.Name, rule.Field));
                    }
                    propertyExp = Expression.Property(propertyExp, rule.Field);
                }
            }

            Type type = propertyExp.Type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }

            if (!DicTypeOperator.ContainsKey(type.Name) || !DicTypeOperator[type.Name].Contains(rule.Operator.ToLower()))
            {
                throw new Exception($"字段【{rule.Field}】不支持运算符【{rule.Operator}】");
            }

            Expression expression;

            // 检查输入数据的类型是否和字段类型兼容，例如字段类型是数字
            try
            {
                switch (rule.Operator.ToLower())
                {
                    case "in":
                        expression = In(type, rule.Value, propertyExp);
                        break;
                    case "not_in":
                        expression = NotIn(type, rule.Value, propertyExp);
                        break;
                    case "equal":
                        expression = Equals(type, rule.Value, propertyExp);
                        break;
                    case "not_equal":
                        expression = NotEquals(type, rule.Value, propertyExp);
                        break;
                    case "between":
                        expression = Between(type, rule.Value, propertyExp);
                        break;
                    case "not_between":
                        expression = NotBetween(type, rule.Value, propertyExp);
                        break;
                    case "less":
                        expression = LessThan(type, rule.Value, propertyExp);
                        break;
                    case "less_or_equal":
                        expression = LessThanOrEqual(type, rule.Value, propertyExp);
                        break;
                    case "greater":
                        expression = GreaterThan(type, rule.Value, propertyExp);
                        break;
                    case "greater_or_equal":
                        expression = GreaterThanOrEqual(type, rule.Value, propertyExp);
                        break;
                    case "begins_with":
                        expression = BeginsWith(type, rule.Value, propertyExp);
                        break;
                    case "not_begins_with":
                        expression = NotBeginsWith(type, rule.Value, propertyExp);
                        break;
                    case "contains":
                        expression = Contains(type, rule.Value, propertyExp);
                        break;
                    case "not_contains":
                        expression = NotContains(type, rule.Value, propertyExp);
                        break;
                    case "ends_with":
                        expression = EndsWith(type, rule.Value, propertyExp);
                        break;
                    case "not_ends_with":
                        expression = NotEndsWith(type, rule.Value, propertyExp);
                        break;
                    case "is_empty":
                        expression = IsEmpty(propertyExp);
                        break;
                    case "is_not_empty":
                        expression = IsNotEmpty(propertyExp);
                        break;
                    case "is_null":
                        expression = IsNull(propertyExp);
                        break;
                    case "is_not_null":
                        expression = IsNotNull(propertyExp);
                        break;
                    case "mc":      // QT扩展运算符：多包含
                        expression = ContainsAny(type, rule.Value, propertyExp);
                        break;
                    case "sdiff":   // QT扩展运算符：几天前，只应用于日期属性
                        expression = DaysBefore(type, rule.Value, propertyExp);
                        break;
                    case "ediff":   // QT扩展运算符：几天后，只应用于日期属性
                        expression = DaysAfter(type, rule.Value, propertyExp);
                        break;
                    default:
                        throw new Exception($"Unknown expression operator: {rule.Operator}");
                }
            }
            catch (NotSupportedException)
            {
                throw new Exception($"字段【{rule.Field}】输入值【{rule.Value}】不合法");
            }

            return expression;
        }

        private static List<ConstantExpression> GetConstants(Type type, string value, bool isCollection)
        {
            if (type == typeof(DateTime) && ParseDatesAsUtc)
            {
                DateTime tDate;
                if (isCollection)
                {
                    var vals =
                        value.Split(new[] { ",", "，", "[", "]", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .Select(p => DateTime.TryParse(p.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out tDate) ? (DateTime?)tDate : null)
                            .Select(p => Expression.Constant(p, type));
                    return vals.ToList();
                }
                else
                {
                    return new List<ConstantExpression>()
                    {
                        Expression.Constant(DateTime.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out tDate) ? (DateTime?)tDate : null)
                    };
                }
            }
            else
            {
                if (isCollection)
                {
                    var tc = TypeDescriptor.GetConverter(type);
                    var vals =
                        value.Split(new[] { ",", "，", "[", "]", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .Select(p => tc.ConvertFromString(p.Trim()))
                            .Select(p => Expression.Constant(p, type));
                    return vals.ToList();
                }
                else
                {
                    var tc = TypeDescriptor.GetConverter(type);
                    return new List<ConstantExpression>()
                    {
                        Expression.Constant(tc.ConvertFromString(value.Trim()))
                    };
                }
            }
        }

        #region Expression Types

        private static Expression GetNullCheckExpression(Expression propertyExp)
        {
            var isNullable = !propertyExp.Type.IsValueType || Nullable.GetUnderlyingType(propertyExp.Type) != null;

            if (isNullable)
            {
                return Expression.NotEqual(propertyExp, Expression.Constant(propertyExp.Type.GetDefaultValue(), propertyExp.Type));

            }
            return Expression.Equal(Expression.Constant(true, typeof(bool)), Expression.Constant(true, typeof(bool)));
        }



        private static Expression IsNull(Expression propertyExp)
        {
            var isNullable = !propertyExp.Type.IsValueType || Nullable.GetUnderlyingType(propertyExp.Type) != null;

            if (isNullable)
            {
                var someValue = Expression.Constant(null, propertyExp.Type);

                Expression exOut = Expression.Equal(propertyExp, someValue);

                return exOut;
            }
            return Expression.Equal(Expression.Constant(true, typeof(bool)), Expression.Constant(false, typeof(bool)));
        }

        private static Expression IsNotNull(Expression propertyExp)
        {
            return Expression.Not(IsNull(propertyExp));
        }

        private static Expression IsEmpty(Expression propertyExp)
        {
            var someValue = Expression.Constant(0, typeof(int));

            var nullCheck = GetNullCheckExpression(propertyExp);

            Expression exOut;

            if (IsGenericList(propertyExp.Type))
            {

                exOut = Expression.Property(propertyExp, propertyExp.Type.GetProperty("Count"));

                exOut = Expression.AndAlso(nullCheck, Expression.Equal(exOut, someValue));
            }
            else
            {
                exOut = Expression.Property(propertyExp, typeof(string).GetProperty("Length"));

                exOut = Expression.AndAlso(nullCheck, Expression.Equal(exOut, someValue));
            }

            return exOut;
        }

        private static Expression IsNotEmpty(Expression propertyExp)
        {
            return Expression.Not(IsEmpty(propertyExp));
        }

        private static Expression Contains(Type type, string value, Expression propertyExp)
        {
            var someValue = Expression.Constant(value.ToLower(), typeof(string));

            var nullCheck = GetNullCheckExpression(propertyExp);

            var method = propertyExp.Type.GetMethod("Contains", new[] { type });

            Expression exOut = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            exOut = Expression.AndAlso(nullCheck, Expression.Call(exOut, method, someValue));

            return exOut;
        }

        private static Expression NotContains(Type type, string value, Expression propertyExp)
        {
            return Expression.Not(Contains(type, value, propertyExp));
        }

        private static Expression EndsWith(Type type, string value, Expression propertyExp)
        {
            var someValue = Expression.Constant(value.ToLower(), typeof(string));

            var nullCheck = GetNullCheckExpression(propertyExp);

            var method = propertyExp.Type.GetMethod("EndsWith", new[] { type });

            Expression exOut = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            exOut = Expression.AndAlso(nullCheck, Expression.Call(exOut, method, someValue));

            return exOut;
        }

        private static Expression NotEndsWith(Type type, string value, Expression propertyExp)
        {
            return Expression.Not(EndsWith(type, value, propertyExp));
        }

        private static Expression BeginsWith(Type type, string value, Expression propertyExp)
        {
            var someValue = Expression.Constant(value.ToLower(), typeof(string));

            var nullCheck = GetNullCheckExpression(propertyExp);

            var method = propertyExp.Type.GetMethod("StartsWith", new[] { type });

            Expression exOut = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            exOut = Expression.AndAlso(nullCheck, Expression.Call(exOut, method, someValue));

            return exOut;
        }

        private static Expression NotBeginsWith(Type type, string value, Expression propertyExp)
        {
            return Expression.Not(BeginsWith(type, value, propertyExp));
        }



        private static Expression NotEquals(Type type, string value, Expression propertyExp)
        {
            return Expression.Not(Equals(type, value, propertyExp));
        }



        private static Expression Equals(Type type, string value, Expression propertyExp)
        {
            Expression someValue = GetConstants(type, value, false).First();

            Expression exOut;
            if (type == typeof(string))
            {
                if (string.IsNullOrEmpty(value))
                {
                    exOut = Expression.OrElse(IsNull(propertyExp), Expression.Equal(propertyExp, Expression.Constant("")));
                }
                else
                {
                    var nullCheck = GetNullCheckExpression(propertyExp);

                    exOut = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                    someValue = Expression.Call(someValue, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                    exOut = Expression.AndAlso(nullCheck, Expression.Equal(exOut, someValue));
                }
            }
            else
            {
                exOut = Expression.Equal(propertyExp, Expression.Convert(someValue, propertyExp.Type));
            }

            return exOut;
        }


        private static Expression LessThan(Type type, string value, Expression propertyExp)
        {
            var someValue = GetConstants(type, value, false).First();

            Expression exOut = Expression.LessThan(propertyExp, Expression.Convert(someValue, propertyExp.Type));


            return exOut;


        }

        private static Expression LessThanOrEqual(Type type, string value, Expression propertyExp)
        {
            var someValue = GetConstants(type, value, false).First();

            Expression exOut = Expression.LessThanOrEqual(propertyExp, Expression.Convert(someValue, propertyExp.Type));


            return exOut;


        }

        private static Expression GreaterThan(Type type, string value, Expression propertyExp)
        {

            var someValue = GetConstants(type, value, false).First();



            Expression exOut = Expression.GreaterThan(propertyExp, Expression.Convert(someValue, propertyExp.Type));


            return exOut;


        }

        private static Expression GreaterThanOrEqual(Type type, string value, Expression propertyExp)
        {
            var someValue = GetConstants(type, value, false).First();

            Expression exOut = Expression.GreaterThanOrEqual(propertyExp, Expression.Convert(someValue, propertyExp.Type));


            return exOut;


        }

        private static Expression Between(Type type, string value, Expression propertyExp)
        {
            var someValue = GetConstants(type, value, true);


            Expression exBelow = Expression.GreaterThanOrEqual(propertyExp, Expression.Convert(someValue[0], propertyExp.Type));
            Expression exAbove = Expression.LessThanOrEqual(propertyExp, Expression.Convert(someValue[1], propertyExp.Type));

            return Expression.And(exBelow, exAbove);


        }

        private static Expression NotBetween(Type type, string value, Expression propertyExp)
        {
            return Expression.Not(Between(type, value, propertyExp));
        }

        private static Expression In(Type type, string value, Expression propertyExp)
        {


            var someValues = GetConstants(type, value, true);

            var nullCheck = GetNullCheckExpression(propertyExp);

            if (IsGenericList(propertyExp.Type))
            {
                var genericType = propertyExp.Type.GetGenericArguments().First();
                var method = propertyExp.Type.GetMethod("Contains", new[] { genericType });
                Expression exOut;

                if (someValues.Count > 1)
                {
                    exOut = Expression.Call(propertyExp, method, Expression.Convert(someValues[0], genericType));
                    var counter = 1;
                    while (counter < someValues.Count)
                    {
                        exOut = Expression.Or(exOut,
                            Expression.Call(propertyExp, method, Expression.Convert(someValues[counter], genericType)));
                        counter++;
                    }
                }
                else
                {
                    exOut = Expression.Call(propertyExp, method, Expression.Convert(someValues.First(), genericType));
                }


                return Expression.AndAlso(nullCheck, exOut);
            }
            else
            {
                Expression exOut;

                if (someValues.Count > 1)
                {
                    if (type == typeof(string))
                    {

                        exOut = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                        exOut = Expression.Equal(exOut, Expression.Convert(someValues[0], propertyExp.Type));
                        var counter = 1;
                        while (counter < someValues.Count)
                        {
                            exOut = Expression.Or(exOut,
                                Expression.Equal(
                                    Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes)),
                                    Expression.Convert(someValues[counter], propertyExp.Type)));
                            counter++;
                        }
                    }
                    else
                    {
                        exOut = Expression.Equal(propertyExp, Expression.Convert(someValues[0], propertyExp.Type));
                        var counter = 1;
                        while (counter < someValues.Count)
                        {
                            exOut = Expression.Or(exOut,
                                Expression.Equal(propertyExp, Expression.Convert(someValues[counter], propertyExp.Type)));
                            counter++;
                        }
                    }



                }
                else
                {
                    if (type == typeof(string))
                    {

                        exOut = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                        exOut = Expression.Equal(exOut, someValues.First());
                    }
                    else
                    {
                        exOut = Expression.Equal(propertyExp, Expression.Convert(someValues.First(), propertyExp.Type));
                    }
                }


                return Expression.AndAlso(nullCheck, exOut);
            }


        }

        private static Expression NotIn(Type type, string value, Expression propertyExp)
        {
            return Expression.Not(In(type, value, propertyExp));
        }

        #endregion


        private static Expression ContainsAny(Type type, string value, Expression propertyExp)
        {
            var someValues = GetConstants(type, value, true);

            var containsMethod = propertyExp.Type.GetMethod("Contains", new[] { type });

            var listFilter = new List<Expression>();
            foreach (var v in someValues)
            {
                Expression toLower = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                var body = Expression.Call(toLower, containsMethod, v);
                listFilter.Add(body);
            }

            Expression bodyExp = listFilter.Aggregate(Expression.OrElse);

            var nullCheck = GetNullCheckExpression(propertyExp);
            Expression exOut = Expression.AndAlso(nullCheck, bodyExp);

            return exOut;
        }

        private static Expression DaysBefore(Type type, string value, Expression propertyExp)
        {
            if (type.Name != "DateTime")
            {
                return NeverTrue();
            }

            int numOfDays = 0;
            if (!int.TryParse(value, out numOfDays))
            {
                return NeverTrue();
            }

            var dateTimeValue = Expression.Constant(DateTime.Now.Date.AddDays(-numOfDays));

            var nullCheck = GetNullCheckExpression(propertyExp);
            Expression bodyExp = Expression.LessThan(propertyExp, Expression.Convert(dateTimeValue, propertyExp.Type));
            Expression exOut = Expression.AndAlso(nullCheck, bodyExp);
            return exOut;
        }

        private static Expression DaysAfter(Type type, string value, Expression propertyExp)
        {
            if (type.Name != "DateTime")
            {
                return NeverTrue();
            }

            int numOfDays = 0;
            if (!int.TryParse(value, out numOfDays))
            {
                return NeverTrue();
            }

            var dateTimeValue = Expression.Constant(DateTime.Now.Date.AddDays(numOfDays));

            var nullCheck = GetNullCheckExpression(propertyExp);
            Expression bodyExp = Expression.GreaterThan(propertyExp, Expression.Convert(dateTimeValue, propertyExp.Type));
            Expression exOut = Expression.AndAlso(nullCheck, bodyExp);
            return exOut;
        }

        private static Expression NeverTrue()
        {
            return Expression.Equal(Expression.Constant(true, typeof(bool)), Expression.Constant(false, typeof(bool)));
        }

        private static bool IsGenericList(this Type o)
        {
            var isGenericList = false;

            var oType = o;

            if (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)))
                isGenericList = true;

            return isGenericList;
        }

        private static object GetDefaultValue(this Type type)
        {
            return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
        }

    }

    /// <summary>
    /// This class is used to define a hierarchical filter for a given collection.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FilterRule
    {
        /// <summary>
        /// Condition - acceptable values are "and" and "or".
        /// </summary>
        /// <value>
        /// The condition.
        /// </value>
        [JsonProperty("condition")]
        public string Condition { get; set; }
        /// <summary>
        /// The name of the field that the filter applies to.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        [JsonProperty("FilterField")]
        public string Field { get; set; }
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty("id")]
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the input.
        /// </summary>
        /// <value>
        /// The input.
        /// </value>
        [JsonProperty("input")]
        public string Input { get; set; }
        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// <value>
        /// The operator.
        /// </value>
        [JsonProperty("Operator")]
        [JsonConverter(typeof(OperatorConverter))]
        public string Operator { get; set; }
        /// <summary>
        /// Gets or sets nested filter rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        [JsonProperty("rules")]
        public virtual List<FilterRule> Rules { get; set; }
        /// <summary>
        /// Gets or sets the type. Supported values are "integer", "double", "string", "date", "datetime", and "boolean".
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [JsonProperty("type")]
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the value of the filter.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [JsonProperty("FilterValue")]
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("FilterTable")]
        public string Table { get; set; }
    }

    /// <summary>
    /// 转换操作符
    /// </summary>
    public class OperatorConverter : JsonConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <summary>
        /// QT 操作符转换为当前组件操作符
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var operation = string.Format("{0}", reader.Value);
            switch (operation)
            {
                case "==":
                    return "equal";
                case "<>":
                    return "not_equal";
                case "⊂":
                    return "contains";
                case "⊄":
                    return "not_contains";
                case "ni":
                    return "not_in";
                case "≥":
                    return "greater_or_equal";
                case "≤":
                    return "less_or_equal";
                case ">":
                    return "greater";
                case "<":
                    return "less";
                default:
                    return operation;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var operation = string.Format("{0}", value);
            var newOp = "";
            switch (operation)
            {
                case "equal":
                    newOp = "==";
                    break;
                case "not_equal":
                    newOp = "<>";
                    break;
                case "contains":
                    newOp = "⊂";
                    break;
                case "not_contains":
                    newOp = "⊄";
                    break;
                case "not_in":
                    newOp = "ni";
                    break;
                case "greater_or_equal":
                    newOp = "≥";
                    break;
                case "less_or_equal":
                    newOp = "≤";
                    break;
                case "greater":
                    newOp = ">";
                    break;
                case "less":
                    newOp = "<";
                    break;
                default:
                    newOp = operation;
                    break;
            }
            writer.WriteValue(newOp);
        }
    }
}
