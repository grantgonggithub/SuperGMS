/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.AttributeEx
 文件名：  EditFormApiArgs
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/24 12:03:31

 功能描述：

----------------------------------------------------------------*/

namespace SuperGMS.Extend.EditFormFieldHelper
{
    using SuperGMS.Extend.BaseAppExtend.EditFormFieldHelper;

    /// <summary>
    /// EditFormApiArgs
    /// </summary>
    public class EditFormApiArgs
    {
        /// <summary>
        /// 编辑表单的接口名称
        /// </summary>
        public string ApiName { get; set; }
    }

    /// <summary>
    /// 编辑表单各个字段的属性
    /// </summary>
    public class EditFormApiResult
    {
        /// <summary>
        /// Gets or sets 字段名称
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets 是否可以编辑，0，可以；1、不可用；
        /// </summary>
        public int IsEdit { get; set; }

        /// <summary>
        /// Gets or sets 是否必输，0、非必输，1、必输
        /// </summary>
        public int IsRequired { get; set; }

        /// <summary>
        /// Gets or sets 分组名称，当前字段对应得控件所在的分组，分组名称相同的控件会显示在一起
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets 当前字段所对应的控件显示的Tab页，Tab页相同的将被显示在一起
        /// </summary>
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets 当前字段的长度，这个长度是数据库中的字段长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets 当前字段类型，int\decimal\string\datetime
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets 显示的控件类型
        /// </summary>
        public ControlType CnType { get; set; }
    }
}
