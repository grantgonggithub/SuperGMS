using SuperGMS.AttributeEx;
using SuperGMS.Tools;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SuperGMS.FileEx
{
    /// <summary>
    /// Excel表头信息
    /// </summary>
    internal class SheetColumnInfo
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 必须导入列吗
        /// </summary>
        public bool MustImport { get; set; }
        /// <summary>
        /// 注释
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 不导入的列吗
        /// </summary>
        public bool NotImport { get; set; }

        /// <summary>
        /// 列属性
        /// </summary>
        public PropertyInfo ColProperty { get; set; }
        
    }

    /// <summary>
    /// Excel 对应的类信息，包含多个子表
    /// </summary>
    internal class SheetInfo
    {
        /// <summary>
        /// sheet 资源名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 子属性信息 ，如果为给定父类型，则为null
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
        public List<SheetColumnInfo> ColumnInfos { get; set; }

        /// <summary>
        /// 解析的类型
        /// </summary>
        public Type Type { get; set; }
        public SheetInfo()
        {
            ColumnInfos = new List<SheetColumnInfo>();
        }
    }
    /// <summary>
    /// 辅助解析类的信息，包含属性，类名，及资源相关的转换
    /// 辅助用于Excel生成
    /// </summary>
    internal class ClassResourceParse
    {
        private Type _type;
        public ClassResourceParse(Type type)
        {
            _type = type;
        }

        /// <summary>
        /// 得到类型的 资源名称
        /// </summary>
        public string Name => _type.FullName;

        /// <summary>
        /// 从缓存获取类型的 属性信息
        /// </summary>
        public PropertyInfo[] PropertyInfos =>ReflectionTool.GetPropertyInfosFromCache(_type);

        /// <summary>
        /// 根据提供的类型 获取Excel 表头信息
        /// </summary>
        /// <param name="type">外界调用</param>
        /// <param name="pi">如果是泛型属性，则继续</param>
        /// <returns></returns>
        public List<SheetInfo> GetSheetInfo(Type type,PropertyInfo pi = null)
        {
            var sheets = new List<SheetInfo>();
            var propertyInfos = ReflectionTool.GetPropertyInfosFromCache(type);
            var sheetName = type.FullName;
            
            sheets.Add(new SheetInfo
            {
                Name = sheetName,
                Type = type,
                PropertyInfo = pi
            });
            var list = sheets[0].ColumnInfos;

            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                {
                    if (ReflectionTool.GetCustomAttributeEx<NotImportAttribute>(propertyInfo) == null)
                    {
                        var t = propertyInfo.PropertyType.GenericTypeArguments[0];
                        var dictTmp = GetSheetInfo(t,propertyInfo);
                        sheets.AddRange(dictTmp);
                    }
                }
                else
                {
                    var notImport = (ReflectionTool.GetCustomAttributeEx<NotImportAttribute>(propertyInfo) != null);
                    string columnName = propertyInfo.Name;
                    /*添加列定义的字段类型*/
                    string columnTypename = GetColumnTypeName(propertyInfo, type);

                    ////获取导入模板列中的备注内容
                    string columnComment = columnTypename;
                    if (ReflectionTool.GetCustomAttributeEx<ImportCommentAttribute>(propertyInfo) != null)
                    {
                        columnComment += ("," +
                                          ReflectionTool.GetCustomAttributeEx<ImportCommentAttribute>(propertyInfo).Commont);
                    }

                    list.Add(new SheetColumnInfo()
                    {
                        NotImport = notImport,
                        Name = columnName,
                        Note = columnComment,
                        ColProperty = propertyInfo,
                        MustImport = ReflectionTool.GetCustomAttributeEx<RequiredAttribute>(propertyInfo) != null
                    });
                }
            }
            return sheets;

        }

        /// <summary>
        /// 获取列类型
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetColumnTypeName(PropertyInfo propertyInfo, Type type)
        {
            string columnType = propertyInfo.PropertyType.ToString();
            string columnTypename = "";
            switch (columnType)
            {
                case "System.String":
                    int length = 0; //todo 此处未取数据长度
                    if (length > 0)
                    {
                        columnTypename = "字符" + "(" + length + ")";
                    }
                    else
                    { columnTypename = "字符"; }
                    break;
                case "System.DateTime":
                    columnTypename = "日期" + ":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    break;
                case "System.Decimal":
                    columnTypename = "0";
                    break;
            }

            return columnTypename;
        }
    }
}
