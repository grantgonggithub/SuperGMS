//
// 文件：QueryModel.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29

#region

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#endregion

namespace SuperGMS.DB.EFEx.DynamicSearch.Model
{
    /// <summary>
    ///     用户自动收集搜索条件的Model
    ///     Add by Grant 2014-3-27
    /// </summary>
    [DataContract]
    [Serializable]
    public class QueryModel
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public QueryModel()
        {
            Items = new List<ConditionItem>();
        }

        /// <summary>
        ///     查询条件
        /// </summary>
        [DataMember]
        public List<ConditionItem> Items { get; set; }
    }
}