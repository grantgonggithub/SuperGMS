/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Extensions
 文件名：  DataTableExtension
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/22 21:37:21

 功能描述：

----------------------------------------------------------------*/

namespace SuperGMS.Extend.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using SuperGMS.DB.EFEx.GrantDbContext;
    using SuperGMS.Extensions;
    using SuperGMS.JsonEx;
    using SuperGMS.Rpc.Server;
    using System.Text;
    using SuperGMS.ExceptionEx;

    /// <summary>
    /// DataTableExtension
    /// </summary>
    public static class DataTableExtension
    {
        /// <summary>
        /// 将list模型转换成datatable。类属性名 保存为ColumnName，类属性名资源保存为Caption
        /// 此处list直接读取属性，并按照属性读取资源，本地化后存为Excel，此处提供了datetime的最小值转换为空的操作，
        /// 若有更多转换操作，需要先组织好数据， 再利用该函数进行转换
        /// 注意：如果T是动态类型，需要业务自己保证 值均为字符串，否则导出excel后格式有可能不正确
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="modelsList">模型list</param>
        /// <param name="needResource">是否需要进行列头资源文件转换，默认不转换，一般从Execl上来的都需要转换</param>
        /// <param name="context">rpccontext用于资源文件转换</param>
        /// <param name="includeCols">列在此集合内</param>
        /// <param name="noIncludeCols">列不在此集合内</param>
        /// <returns>datatable</returns>
        public static DataTable ToDataTable<T>(this List<T> modelsList, bool needResource = false, RpcContext context = null, IEnumerable<string> includeCols = null, IEnumerable<string> noIncludeCols = null)
            where T : class
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var type = typeof(T);
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                throw new NotImplementedException();
            }

            DataTable dataTable = new DataTable();
            if (needResource)
            {
                dataTable.TableName = context.R(typeof(T).Name) ?? typeof(T).Name;
            }
            else
            {
                dataTable.TableName = typeof(T).Name;
            }
            // 对 dynamic 转Datatable的处理
            // 此处为定制功能，不支持字符串以外类型，如果使用，需要业务自己保证 值均为字符串
            var numberStr = "$number$";
            if (type == typeof(object) && modelsList.Count > 0 && modelsList[0] is IDictionary<string, object>)
            {
                var keys = ((IDictionary<string, object>) modelsList[0]).Keys;
                foreach (var p in keys)
                {
                    if ((includeCols?.Contains(p) ?? true) &&
                        (!(noIncludeCols?.Contains(p) ?? false)))
                    {
                        var sRealName = p;
                        DataColumn col;
                        if (p.EndsWith(numberStr))
                        {
                            sRealName = p.Substring(0, p.Length - numberStr.Length);
                            col = new DataColumn(p, typeof(decimal));
                            col.Caption = sRealName;
                        }
                        else
                        {
                            col = new DataColumn(p, typeof(string));
                        }

                        if (needResource)
                        {
                            var fieldResource = context.R(sRealName);
                            if (!string.IsNullOrEmpty(fieldResource))
                            {
                                col.Caption = fieldResource;
                            }                            
                        }

                        dataTable.Columns.Add(col);
                    }
                }

                // 增加数据到DataTable里
                modelsList.ForEach(x =>
                {
                    decimal dOut = 0;
                    var row = dataTable.NewRow();
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        var pVal = ((IDictionary<string, object>)x)[col.ColumnName]?.ToString();
                        if (col.DataType == typeof(decimal))
                        {
                            decimal.TryParse(pVal, out dOut);
                            row[col.ColumnName] = dOut;
                        }
                        else
                        {
                            row[col.ColumnName] = pVal;
                        }
                    }
                    var itemArray = row.ItemArray;
                    if (!itemArray.All(t => t == null || string.IsNullOrEmpty(t.ToString().Trim())))
                    {
                        dataTable.Rows.Add(row);
                    }
                });
            }
            else
            {
                var objFieldNames = typeof(T).GetProperties(flags);
                foreach (var p in objFieldNames)
                {
                    if ((includeCols?.Contains(p.Name) ?? true) &&
                        (!(noIncludeCols?.Contains(p.Name) ?? false)))
                    {
                        var t = p.PropertyType;

                        // 对 Nullable<> 类型做处理， 模型内对 int?  datetime? 进行支持
                        if (p.PropertyType.IsGenericType)
                        {
                            t = p.PropertyType.GetGenericArguments().FirstOrDefault();
                            t = (t == null) ? typeof(string) : t;
                        }

                        DataColumn col = new DataColumn(p.Name, t);
                        if (needResource)
                        {
                            var fieldResource = context.R(p.Name);
                            if (!string.IsNullOrEmpty(fieldResource))
                            {
                                col.Caption = fieldResource;
                            }
                        }

                        dataTable.Columns.Add(col);
                    }
                }

                // 增加数据到DataTable里
                modelsList.ForEach(x =>
                {
                    var row = dataTable.NewRow();
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        var p = objFieldNames.FirstOrDefault(y => y.Name == col.ColumnName);
                        if (p == null)
                        {
                            continue;
                        }

                        var pVal = p.GetValue(x);
                        if (p.PropertyType == typeof(DateTime) && IsNullDateTime(pVal))
                        {
                            row[col.ColumnName] = DBNull.Value;
                        }
                        else
                        {
                            row[col.ColumnName] = pVal ?? DBNull.Value;
                        }
                    }
                    var itemArray = row.ItemArray;
                    if (!itemArray.All(t => t == null || string.IsNullOrEmpty(t.ToString().Trim())))
                    {
                        dataTable.Rows.Add(row);
                    }
                });
            }

            
            return dataTable;
        }

        /// <summary>
        /// 将dataTable转换成T类型的实体列表
        /// </summary>
        /// <typeparam name="T">要转换的实体类型</typeparam>
        /// <param name="dataTable">数据源</param>
        /// <param name="needResource">是否需要进行列头资源文件转换，默认不转换，一般从Execl上来的都需要转换</param>
        /// <param name="context">rpccontext用于资源文件转换</param>
        /// <param name="allowDuplicates">是否允许重复(默认允许,为否时有重复会报错)</param>
        /// <returns>T的列表</returns>
        public static List<T> ToList<T>(this DataTable dataTable, bool needResource = false, RpcContext context = null, bool allowDuplicates=true)
            where T : new()
        {
            var dataList = new Dictionary<string, T>();

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            var objFieldNames = typeof(T).GetProperties(flags).Cast<PropertyInfo>().Select(item => new
            {
                Name = item.Name,
                Type = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType,
            }).ToList();
            if (needResource)
            {
                foreach (var p in objFieldNames)
                {
                    var fieldResource = context.R(p.Name);
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        // 如果实体的字段名称转换成资源名称，和模板上来的列名相同，直接把列名改成实体名称，减少循环
                        if (fieldResource?.ToLower().Trim() == col.ColumnName.ToLower())
                        {
                            col.ColumnName = p.Name;
                        }
                    }
                }
            }

            var dtlFieldNames = dataTable.Columns.Cast<DataColumn>().Select(item => new
            {
                Name = item.ColumnName,
                Type = item.DataType,
            }).ToList();

            StringBuilder key = new StringBuilder();
            int idx = -1;
            List<int> emptyRowIndex = new List<int>();
            List<int> duplicateRowIndexs = new List<int>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                idx += 1;
                var itemArray = dataRow.ItemArray;
                object[] values = null;
                if (itemArray.Length > 2)
                {
                    values = new object[itemArray.Length - 2];//有附加的两个列rowindex errormsg
                    Array.Copy(itemArray, values, values.Length);
                }
                if (values != null)
                {
                    if (itemArray.All(t => t == null || string.IsNullOrEmpty(t.ToString().Trim())))
                    {
                        emptyRowIndex.Add(idx + 2);
                        continue;
                    }
                }
                var classObj = new T();
                if (key.Length > 0)
                    key.Clear();
                string convertErrors = "";
                foreach (var objField in objFieldNames)
                {
                    PropertyInfo propertyInfos = classObj.GetType().GetProperty(objField.Name);
                    var dtField = dtlFieldNames.Find(x => x.Name == objField.Name);
                    var error = "";
                    if (dtField == null)
                    {
                        var k = setFieldValue(propertyInfos, classObj, null, out error);
                        if (objField.Name != "ExcelRowIndex" && objField.Name != "ErrorMsg") // 附加内容不算
                            key.Append(k);
                    }
                    else
                    {
                        var k = setFieldValue(propertyInfos, classObj, dataRow[dtField.Name], out error);
                        if (objField.Name != "ExcelRowIndex" && objField.Name != "ErrorMsg")
                            key.Append(k);
                    }
                    convertErrors += error;
                }
                if (!string.IsNullOrEmpty(convertErrors))
                {
                    PropertyInfo errorPropertyInfo = classObj.GetType().GetProperty("ErrorMsg");
                    if (errorPropertyInfo != null)
                    {
                        setFieldValue(errorPropertyInfo, classObj, convertErrors, out string e);
                    }
                }
                if (dataList.ContainsKey(key.ToString().ToLower()))
                    duplicateRowIndexs.Add(idx + 2);
                dataList[key.ToString().ToLower()] = classObj;
            }
            StringBuilder sbError = new StringBuilder();
            if (emptyRowIndex.Count > 0)
            {
                sbError.AppendLine($"Excel中包含空数据,行号{string.Join(',', emptyRowIndex)}请删除!");
            }
            if (duplicateRowIndexs.Count > 0)
            {
                sbError.AppendLine($"Excel中包含重复数据,行号{string.Join(',', duplicateRowIndexs)}请删除!");
            }
            if (!allowDuplicates && sbError.Length > 0)
                throw new BusinessException(sbError.ToString());
            return dataList.Values.ToList();
        }

        private static string setFieldValue(PropertyInfo propertyInfos,object classObj, object value, out string errorMsg)
        {
            Type t = propertyInfos.PropertyType;
            string returnValue = "";
            errorMsg = "";
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                    t = t.GetGenericArguments().FirstOrDefault();
                    t = (t == null) ? typeof(string) : t;
            }
            if (t == typeof(DateTime))
            {
                DateTime v = new DateTime();
                if (TryConvertToDateTime(value, out v))
                {
                    propertyInfos.SetValue(classObj, v, null);
                }
                else
                {
                    propertyInfos.SetValue(classObj, null, null);
                    errorMsg = $"日期值:{value?.ToString()}转换失败";
                }
                
                returnValue = ConvertToDateString(v);
            }
            else if (t == typeof(int))
            {
                var v = ConvertToInt(value);
                propertyInfos.SetValue(classObj,v , null);
                returnValue = ConvertToString(v);
            }
            else if (t == typeof(long))
            {
                var v = ConvertToLong(value);
                propertyInfos.SetValue(classObj, v, null);
                returnValue = ConvertToString(v);
            }
            else if (t == typeof(decimal))
            {
                var v = ConvertToDecimal(value);
                propertyInfos.SetValue(classObj,v , null);
                returnValue = ConvertToDecimalString(v);
            }
            else if (t == typeof(string))
            {
                if (value!=null&&value!=DBNull.Value&& value.GetType() == typeof(DateTime))
                {
                    var v = ConvertToDateString(value);
                    propertyInfos.SetValue(classObj,v , null);
                    returnValue = ConvertToDateString(v);
                }
                else
                {
                    var v = ConvertToString(value);
                    propertyInfos.SetValue(classObj,v , null);
                    returnValue = v;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// 这个转换了底层的必输字段的错误记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable"></param>
        /// <param name="context"></param>
        /// <param name="equalityComparer">上层需要告诉框架怎么去重</param>
        /// <param name="allowDuplicates">是否允许重复(默认允许,为否时有重复会报错)</param>
        /// <returns></returns>
        public static List<T> ToListEx<T>(this DataTable dataTable, RpcContext context = null, IEqualityComparer<T> equalityComparer = null, bool allowDuplicates = true)
            where T : ImportBaseDto,new() 
        {
            if (equalityComparer == null)
                return ToList<T>(dataTable, false, context, allowDuplicates);
            else
                return ToList<T>(dataTable, false, context, allowDuplicates)?.Distinct<T>(equalityComparer)?.ToList();
        }

        ///// <summary>
        ///// 一个可以按照业务层指定的IComparable<T>比较器进行去重的方法
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="dataTable"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static List<T> ToListDistinct<T>(this DataTable dataTable, RpcContext context = null)
        //   where T : ImportBaseDto,IComparable<T>, new()
        //{
        //    var list = ToListEx<T>(dataTable, context);
        //    return list.Distinct<T>()?.ToList();
        //}

        public static DataSet GetData(this IDapperDbContext dbContext, string sql)
        {
            var rep = dbContext.GetRepository();
            var data = rep.QueryDataSetBySql(sql);
            return data;
        }

        /// <summary>
        /// 得到列名集合
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static List<string> GetAllColumnNames(this DataTable dataTable)
        {
            var cols = new List<string>();
            foreach (DataColumn col in dataTable.Columns)
            {
                cols.Add(col.ColumnName);
            }

            return cols;
        }
        private static string ConvertToDateString(object date)
        {
            if (date == null)
            {
                return string.Empty;
            }
            DateTime dateValue = SuperGMSDateTimeJsonConvert.DefaultDateTimeValue;
            DateTime.TryParse(date.ToString(),out dateValue);
            if (dateValue <= SuperGMSDateTimeJsonConvert.DefaultDateTimeValue) // 小于等于默认值就返回空
            {
                return string.Empty;
            }
            return dateValue.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private static string ConvertToDecimalString(decimal decimalValue)
        {
           return decimalValue.ToString("#0.######");
        }

        private static string ConvertToString(object value)
        {
            return value?.ToString();
        }

        private static int ConvertToInt(object value)
        {
            int intValue = 0;
            if (value == null||value==DBNull.Value)
            {
                return intValue;
            }

            int.TryParse(value.ToString(), out intValue);
            return intValue;
        }

        private static long ConvertToLong(object value)
        {
            long longValue = 0;
            if (value == null || value == DBNull.Value)
            {
                return longValue;
            }

            long.TryParse(value.ToString(), out longValue);
            return longValue;
        }

        private static decimal ConvertToDecimal(object value)
        {
            decimal decimalValue = 0;
            if (value == null || value == DBNull.Value)
            {
                return decimalValue;
            }

            decimal.TryParse(value.ToString(), out decimalValue);
            return decimalValue;
        }

        private static bool IsNullDateTime(object date)
        {
            DateTime dateTimeValue = SuperGMSDateTimeJsonConvert.DefaultDateTimeValue;
            if (date == null || date == DBNull.Value) return true;
            var dt = (DateTime)date;
            return dt <= dateTimeValue;
        }
        /// <summary>
        /// 如果转换成功(值为null或者值小于1900-01-01均返回1900-01-01,否则返回实际值)
        /// 转换失败时,返回1900-01-01
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private static bool TryConvertToDateTime(object value , out DateTime dateTime)
        {
            if (value == null || value == DBNull.Value|| value.ToString()==string.Empty) // 如果是空就要转为默认值
            {
                dateTime = SuperGMSDateTimeJsonConvert.DefaultDateTimeValue;
                return true;
            }

            if (DateTime.TryParse(value.ToString(), out dateTime))
            {
                if (dateTime < SuperGMSDateTimeJsonConvert.DefaultDateTimeValue)
                    dateTime = SuperGMSDateTimeJsonConvert.DefaultDateTimeValue; // 防止用户输入000-00-00
                return true;
            }
            else
            {
                dateTime = SuperGMSDateTimeJsonConvert.DefaultDateTimeValue;
                return false;
            }
        }
    }

    public class ImportBaseDto
    {
        public ImportBaseDto()
        {
        }

        /// <summary>
        /// 错误内容
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 在excel中的rowIndex
        /// </summary>
        public string ExcelRowIndex { get; set; }

    }
}
