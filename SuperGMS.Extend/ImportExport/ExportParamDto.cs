using System;
using System.Collections.Generic;
using System.Text;
using SuperGMS.DB.EFEx.DynamicSearch;

namespace SuperGMS.Extend.ImportExport
{
    /// <summary>
    /// 导出参数
    /// </summary>
    public class ExportParamDto
    {
        /// <summary>导出模板文件guid</summary>
        public string TemplateGuid { get; set; }

        /// <summary>
        /// 系统id
        /// </summary>
        public string SysId { get; set; }

        /// <summary>
        /// 模块名称，迷失时验证用，或显示用
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>搜索过滤参数</summary>
        public SearchParameters SearchParameters { get; set; }

        /// <summary>从第几页开始</summary>
        public int FromPage { get; set; }

        /// <summary>到第几页结束</summary>
        public int ToPage { get; set; }

        /// <summary>
        /// 保留字段
        /// </summary>
        public string Reserve { get; set; }
    }
}
