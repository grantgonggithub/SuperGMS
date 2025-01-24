using System;

namespace SuperGMS.AttributeEx
{
    /// <summary>
    /// 导入模板中的说明内容,此内容会生成到导入模板的第一行中
    /// </summary>
    public class ImportCommentAttribute : Attribute
    {
        /// <summary>
        /// 默认构造函数,需要传递导入列的说明
        /// </summary>
        /// <param name="comment">导入列的说明, 会根据字符串获取多语言</param>
        public ImportCommentAttribute(string comment)
        {
            Commont = comment;
        }

        /// <summary>
        /// 备注内容
        /// </summary>
        public string Commont { get; set; }
    }
}