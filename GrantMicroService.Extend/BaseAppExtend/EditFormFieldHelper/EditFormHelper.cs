/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Extend.BaseAppExtend.EditFormFieldHelper
 文件名：  EditFormHelper
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/24 15:15:44

 功能描述：

----------------------------------------------------------------*/
namespace GrantMicroService.Extend.BaseAppExtend.EditFormFieldHelper
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using GrantMicroService.Extend.EditFormFieldHelper;

    /// <summary>
    /// EditFormHelper
    /// </summary>
    public class EditFormHelper
    {
        /// <summary>
        /// 查找表单字段属性
        /// </summary>
        /// <param name="appArgs">appArgs</param>
        /// <param name="list">list</param>
        public static void GetEditFormField(Type appArgs, ref List<EditFormApiResult> list)
        {
            PropertyInfo[] pInfo = appArgs.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in pInfo)
            {
                if (p.PropertyType.IsGenericType)
                {
                }
                else if (p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                {
                    EditFormDtoFieldAttribute field = p.GetCustomAttribute<EditFormDtoFieldAttribute>();
                    if (field == null)
                    {
                        continue;
                    }

                    list.Add(new EditFormApiResult()
                    {
                        IsEdit = field.IsEdit,
                        CnType = field.CntType,
                        DataType = p.DeclaringType.Name,
                        FieldName = p.Name,
                        GroupName = field.GroupName,
                        IsRequired = field.IsRequired,
                        TabName = field.TabName,
                        Length = field.Length,
                    });
                }
                else
                {
                    GetEditFormField(p.GetType(), ref list);
                }
            }
        }

        private static int GetFieldLength(PropertyInfo p, Type appArgs)
        {
            return 0;
        }

        /// <summary>
        /// 是否表单编辑的DTO
        /// </summary>
        /// <param name="appArgs">appArgs</param>
        /// <returns>EditFormAttribute</returns>
        public static EditFormAttribute GetEditForm(Type appArgs)
        {
           return appArgs.GetCustomAttribute<EditFormAttribute>();
        }
    }
}
