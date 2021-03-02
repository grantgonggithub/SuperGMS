/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx.GrantDbContext
 文件名：  EFDbContextExtend
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/3 0:40:46

 功能描述：

----------------------------------------------------------------*/
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using SuperGMS.DB.EFEx.GrantDbContext;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace SuperGMS.DB.EFEx.MyDbContext
{
    /// <summary>
    ///
    /// <see cref="EFDbContextExtend" langword="" />
    /// </summary>
    public static class EFDbContextExtend
    {
        public static List<EFChangeInfo> GetEFChanges(this EFDbContext eFDbContext)
        {
            var changes = eFDbContext.GetChangeTracker()?.Entries();
            if (changes == null) return null;
            List<EFChangeInfo> list = new List<EFChangeInfo>();
            foreach (var change in changes)
            {
                var tableName = string.Empty;
                Type type = change.Entity.GetType();
                var thisTableAttribute = typeof(TableAttribute);
                TableAttribute attri = null;
                if (type.IsDefined(thisTableAttribute, true))
                {
                    attri = type.GetCustomAttributes(thisTableAttribute, true).FirstOrDefault() as TableAttribute;
                    tableName = attri?.Name;
                    tableName = string.IsNullOrEmpty(tableName) ? type.Name : tableName;
                }
                list.Add(new EFChangeInfo { TableName = tableName });
            }
            return list;
        }

        private static EFChangeInfo getChanges(EntityEntry entityEntry, string tableName)
        {
            var propertyList = entityEntry.CurrentValues.Properties.Where(i => entityEntry.Property(i.Name).IsModified);
            foreach (var prop in propertyList)
            {
                PropertyEntry entity = entityEntry.Property(prop.Name);

            }
            return null;
        }

        public class EFChangeInfo
        {
            /// <summary>
            /// 修改表名
            /// </summary>
            public string TableName { get; set; }
            /// <summary>
            /// 修改情况
            /// </summary>
            public List<ChangeValue> Changes { get; set; }
        }

        public class ChangeValue
        {
            /// <summary>
            /// 变化类型
            /// </summary>
            public EntityState EntityState { get; set; }
            /// <summary>
            /// 修改字段名
            /// </summary>
            public string Field { get; set; }
            /// <summary>
            /// 修改前的值
            /// </summary>
            public string OldValue { get; set; }
            /// <summary>
            /// 修改后的值
            /// </summary>
            public string NewValue { get; set; }
        }
    }
}