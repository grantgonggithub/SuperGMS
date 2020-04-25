using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GrantMicroService.FileEx
{
    /// <summary>
    /// 自定义导入设置
    /// </summary>
    [Serializable]
    [DataContract]
    public class GrantImportSetting
    {
        /// <summary>
        /// 模板代码
        /// </summary>
        [DataMember]
        public string ImportTemplateID { get; set; }
        /// <summary>
        /// 模板名称
        /// </summary>
        [DataMember]
        public string ImportTemplateName { get; set; }
        /// <summary>
        /// 导入\出类型
        /// </summary>
        [DataMember]
        public string ImportOperation { get; set; }
        /// <summary>
        /// 导入DTO名
        /// </summary>
        [DataMember]
        public string ImportType { set; get; }
        /// <summary>
        /// 导入DTO名
        /// </summary>
        [DataMember]
        public string ImportSubType { set; get; }
        /// <summary>
        /// 下载文件的sheet名
        /// </summary>
        [DataMember]
        public string ImportSheetName { set; get; }
        /// <summary>
        /// 下载的文件名
        /// </summary>
        [DataMember]
        public string ImportFileName { set; get; }
        /// <summary>
        /// 最大导入行数
        /// </summary>
        [DataMember]
        public int ImportMaxCount { set; get; }
        /// <summary>
        /// 字段ID
        /// </summary>
        [DataMember]
        public string ImportFieldID { set; get; }
        /// <summary>
        /// 字段名称
        /// </summary>
        [DataMember]
        public string ImportFieldName { set; get; }
        /// <summary>
        /// 列顺序
        /// </summary>
        [DataMember]
        public int ImportSeqNo { set; get; }
        /// <summary>
        /// 类型顺序
        /// </summary>
        [DataMember]
        public int ImportTypeSeqNo { set; get; }
        /// <summary>
        /// 是否导入
        /// </summary>
        [DataMember]
        public string IsImport { set; get; }
        /// <summary>
        /// 是否必须导入
        /// </summary>
        [DataMember]
        public string IsImportMust { set; get; }
        /// <summary>
        /// 是否多页签
        /// </summary>
        [DataMember]
        public string IsMultSheet { set; get; }
        /// <summary>
        /// 是否关键字段（作为主键使用）
        /// </summary>
        [DataMember]
        public string IsImportKeyField { set; get; }
        /// <summary>
        /// 是否关键字段（作为主键使用）
        /// </summary>
        [DataMember]
        public string IsImportPrimaryField { set; get; }
        /// <summary>
        /// 默认值
        /// </summary>
        [DataMember]
        public string ImportDefaultValue { set; get; }
    }
}
