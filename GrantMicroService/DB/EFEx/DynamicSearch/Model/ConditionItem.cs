//
// 文件：ConditionItem.cs
// 作者：Grant
// 最后更新日期：2017-11-12

#region

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#endregion

namespace GrantMicroService.DB.EFEx.DynamicSearch.Model
{
    /// <summary>
    ///     用于存储查询条件的单元
    ///     Add by Grant 2014-3-27
    /// </summary>
    [Serializable]
    [DataContract]
    [KnownType(typeof(string[]))]
    public class ConditionItem
    {
        /// <summary>
        ///     默认构造函数
        /// </summary>
        public ConditionItem()
        {
        }

        /// <summary>
        ///     传参构造函数
        /// </summary>
        /// <param name="field">查询字段，等于表的列名</param>
        /// <param name="method">查询方法<see cref="GrantMicroService.EFEx.DynamicSearch.Model.QueryMethod" /></param>
        /// <param name="val">查询的值</param>
        public ConditionItem(string field, QueryMethod method, object val)
        {
            Field = field;
            Method = method;
            _value = val;
        }

        /// <summary>
        ///     字段
        /// </summary>
        [DataMember]
        public string Field { get; set; }

        /// <summary>
        ///     查询方式, 可传递 Equal(等于)  ,LessThan(小于), GreaterThan(大于),LessThanOrEqual(小于等于),GreaterThanOrEqual(大于等于),Contains(包含)
        /// </summary>
        [DataMember]
        public QueryMethod Method { get; set; }

        private object _value;
        /// <summary>
        ///     值
        /// </summary>
        [DataMember]
        public object Value
        {
            get
            {

                return _value;
            }
            set
            {
                if (value is Newtonsoft.Json.Linq.JArray)
                {
                    // String数组作为 Object 类型 序列化以后，反序列化回来以后是JArray这个看不懂的类型，所以要强制弄成 这样 这句话是Grant和Grant加的
                    _value = JsonConvert.DeserializeObject<string[]>(value.ToString());
                }
                else if (value is List<int>)
                {
                    var tmp = value as List<int>;
                    _value = tmp.ToArray();
                }
                else if (value is List<string>)
                {
                    var tmp = value as List<string>;
                    _value = tmp.ToArray();
                }
                else
                {
                    _value = value;
                }
            }
        }


        /// <summary>
        ///     前缀，用于标记作用域
        /// </summary>
        [DataMember]
        public string Prefix { get; set; }

        /// <summary>
        ///     此值相同的一组Condition 会使用or 拼接起来 并和其他condition 使用and 连接
        ///     orGroup是组外 or ，组内仍然是 and
        ///     当需要使用((A='1' and B='2') or (A='11' and B='22') or  (C='3'))操作,则此5个表达式值需要维护为 x,1和x,1和x,2和x,2和x
        /// 只支持一个逗号
        /// </summary>
        [DataMember]
        public string OrGroup { get; set; }
    }
}