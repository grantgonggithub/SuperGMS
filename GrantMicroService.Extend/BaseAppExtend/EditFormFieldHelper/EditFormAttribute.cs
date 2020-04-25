/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Extend.BaseAppExtend.EditFormFieldHelper
 文件名：  EditFormAttribute
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/24 15:10:07

 功能描述：

----------------------------------------------------------------*/

using System.Linq;

namespace GrantMicroService.Extend.BaseAppExtend.EditFormFieldHelper
{
    using System;

    /// <summary>
    /// EditFormAttribute
    /// </summary>
    public class EditFormAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditFormAttribute"/> class.
        /// 表单DTO的标记,例如：grantomsContext
        /// 这里之所以要用字符串，是想grant.RpcProxy跟业务的Model解耦合
        /// <param name="dbContextFullName">所在的dbContext</param>
        /// </summary>
        public EditFormAttribute(params string[] dbContextFullName)
        {
            this.DbContextFullName = dbContextFullName.Select(a => a.ToLower()).ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditFormAttribute"/> class.
        /// 暂时用这个构造
        /// </summary>
        public EditFormAttribute()
        {
        }

        /// <summary>
        /// Gets or sets 要查找的DbContext
        /// </summary>
        public string[] DbContextFullName { get; set; }
    }
}
