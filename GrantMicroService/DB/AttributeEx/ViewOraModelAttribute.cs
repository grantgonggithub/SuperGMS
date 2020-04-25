//
// 文件：OraModelAttribute.cs
// 作者：Grant
// 最后更新日期：2014-8-20
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrantMicroService.DB.AttributeEx
{
    /// <summary>
    /// 视图对应的原始模型,
    /// 如果在调用CurdRepository.GetByPage(SearchParemeters) 方法时设置了此属性，
    /// 并且SearchParemeters的 IsGetTotalCount 为 true，并且 PageSize 小于等于1，并且查询条件都在OraModel中
    /// 则会自动切换到单表进行查询统计，提高性能
    /// </summary>
    public class ViewOraModelAttribute : Attribute
    {
        /// <summary>
        /// 原始对象
        /// </summary>
        public Type OraModel { get; set; }


        /// <summary>
        /// 记录原始Model
        /// </summary>
        /// <param name="oraModel"></param>
        public ViewOraModelAttribute(Type oraModel)
        {
            OraModel = oraModel;
        }
    }
}
