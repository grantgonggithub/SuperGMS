using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.DB.EFEx.DynamicSearch.Model
{
    /// <summary>
    /// rpc返回接口的泛型对象, 继承此对象
    /// </summary>
    public class PageResult<T>
    {
        public PageInfo Page { set; get; }
        public List<T> ListValue { set; get; }
    }
}