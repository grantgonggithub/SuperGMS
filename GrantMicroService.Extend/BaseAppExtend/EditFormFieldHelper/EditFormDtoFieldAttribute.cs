/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.AttributeEx
 文件名：  EditFormDto
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/21 17:26:30

 功能描述：

----------------------------------------------------------------*/

namespace GrantMicroService.Extend.BaseAppExtend.EditFormFieldHelper
{
    using System;

    /// <summary>
    /// EditFormDto
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditFormDtoFieldAttribute : Attribute
    {
        /// <summary>
        /// 用于标记表单字段的属性
        /// </summary>
        /// <param name="isEdit">isEdit默认可以编辑</param>
        /// <param name="isRequired">isRequired默认非必填</param>
        /// <param name="controlType">controlType</param>
        /// <param name="length">长度</param>
        /// <param name="groupName">所在的分组</param>
        /// <param name="tabName">所在的Tab页</param>
        public EditFormDtoFieldAttribute(int isRequired = 0, int isEdit = 0, ControlType controlType = ControlType.Input, int length = int.MaxValue, string groupName = "", string tabName = "")
        {
            this.IsRequired = isRequired;
            this.IsEdit = isEdit;
            this.CntType = controlType;
            this.GroupName = groupName;
            this.Length = length;
            this.TabName = tabName;
        }

        /// <summary>
        /// Gets or sets 是否编辑0、可以编辑；1、不可编辑
        /// </summary>
        public int IsEdit { get; set; }

        /// <summary>
        /// Gets or sets 是否必须字段，0、非必填；1、必填
        /// </summary>
        public int IsRequired { get; set; }

        /// <summary>
        /// Gets or sets 所在分组
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets 所在的Tab页
        /// </summary>
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets controlType
        /// </summary>
        public ControlType CntType { get; set; }

        /// <summary>
        /// Gets or sets 长度
        /// </summary>
        public int Length { get; set; }
    }
}