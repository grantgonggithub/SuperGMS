/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Extend.BaseAppExtend.EditFormFieldHelper
 文件名：  ControlType
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/24 13:54:18

 功能描述：

----------------------------------------------------------------*/

namespace SuperGMS.Extend.BaseAppExtend.EditFormFieldHelper
{
    /// <summary>
    /// ControlType
    /// </summary>
    public enum ControlType : int
    {
        /// <summary>
        /// 单行文本输入框
        /// </summary>
        Input = 1,

        /// <summary>
        /// 数字输入框
        /// </summary>
        InputNumber = 2,

        /// <summary>
        /// 复选框
        /// </summary>
        CheckBox = 3,

        /// <summary>
        /// 下拉框
        /// </summary>
        Select = 4,

        /// <summary>
        /// 下拉框带单位
        /// </summary>
        SelectUnit = 5,

        /// <summary>
        /// 搜索输入框
        /// </summary>
        InputSearch = 6,

        /// <summary>
        /// 日期选择框 年月日
        /// </summary>
        DateSelect = 7,

        /// <summary>
        /// 日期时间选择 年月日时分秒
        /// </summary>
        DateTimeSelect = 8,

        /// <summary>
        /// 级联选择框
        /// </summary>
        Cascader = 9,

        /// <summary>
        /// 地址选择
        /// </summary>
        AddressCascader = 10,

        /// <summary>
        /// 多行输入
        /// </summary>
        TextArea = 11,
    }
}
