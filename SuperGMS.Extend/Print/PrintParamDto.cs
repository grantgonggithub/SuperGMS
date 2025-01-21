namespace SuperGMS.Extend.Print
{
    /// <summary>
    /// 打印参数
    /// </summary>
    public class PrintParamDto
    {
        /// <summary>打印模板文件guid</summary>
        public string TemplateGuid { get; set; }

        /// <summary>
        /// 系统id
        /// </summary>
        public string SysId { get; set; }

        /// <summary>
        /// 模块名称，迷失时验证用，或显示用
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// 业务id集合
        /// </summary>
        public string[] Ids { get; set; }

        /// <summary>
        /// 保留字段
        /// </summary>
        public string Reserve { get; set; }
    }
}
