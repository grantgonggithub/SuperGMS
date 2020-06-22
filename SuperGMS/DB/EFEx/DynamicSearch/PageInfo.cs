//
// 文件：PageInfo.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29

#region

using System;
using System.Runtime.Serialization;

#endregion

namespace SuperGMS.DB.EFEx.DynamicSearch
{
    using Microsoft.AspNetCore.Http;
    using System.Web;

    /// <summary>
    /// 分页实体
    /// </summary>
    [Serializable]
    [DataContract]
    public class PageInfo
    {
        public PageInfo()
        {
            IsGetTotalCount = false;
        }
        /// <summary>
        ///     当前页 , 和SkipCount不能同时赋值, 否则以SkipCount为准
        /// </summary>
        [DataMember]
        public int CurrentPage { get; set; }

        /// <summary>
        ///     跳过条数,和CurrentPage不能同时赋值, 否则以SkipCount为准
        /// </summary>
        [DataMember]
        public int SkipCount { get; set; }

        /// <summary>
        ///     页大小
        /// </summary>
        [DataMember]
        public int PageSize { get; set; }

        /// <summary>
        ///     排序字段
        /// </summary>
        [DataMember]
        public string SortField { get; set; }

        /// <summary>
        ///     升序 降序
        /// </summary>
        [DataMember]
        public string SortDirection { get; set; }

        /// <summary>
        ///     是否分页, 因为性能原因,此处设置false无效,为了提高性能必须分页,如果为了获取更多条数据,可以增大PageSize 到一个已知的最大值
        /// </summary>
        [DataMember]
        public bool IsPaging { get; set; }

        /// <summary>
        ///     是否获取总条数，默认是False，如果为了提高性能，而不关注总条数，可以将此值设置为false。因为获取总条数非常!非常！非常！消耗时间 add by Grant 2016-11-9
        /// </summary>
        [DataMember]
        public bool IsGetTotalCount { get; set; }

        /// <summary>
        ///     是否分页
        /// </summary>
        [DataMember]
        public int TotalCount { get; set; }

      
    }
}