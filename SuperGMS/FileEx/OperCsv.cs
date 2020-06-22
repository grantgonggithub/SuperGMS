//
// 文件：OperCsv.cs
// 作者：Grant
// 最后更新日期：2014-06-05 14:29

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

#endregion

namespace SuperGMS.FileEx
{
    /// <summary>
    ///     OperCSV is a helper class handling csv files.
    /// </summary>
    public class OperCsv
    {
        /// <summary>
        ///     写入CSV文件
        /// </summary>
        /// <param name="filePathName">文件路径</param>
        /// <param name="listString"></param>
        public static void WriteFile(string filePathName, List<String[]> listString)
        {
            WriteFile(filePathName, false, listString);
        }

        /// <summary>
        ///     写入CSV文件
        /// </summary>
        /// <param name="filePathName">文件路径</param>
        /// <param name="append">是否追加写入，false时将重写文件</param>
        /// <param name="listString">字符串数组LIST</param>
        public static void WriteFile(string filePathName, bool append, List<String[]> listString)
        {
            using (var fileWriter = new StreamWriter(filePathName, append, Encoding.Default))
            {
                foreach (var strArr in listString)
                {
                    for (int i = 0; i < strArr.Length; i++)
                    {
                        strArr[i] = FormatCell(strArr[i]);
                    }
                    fileWriter.WriteLine(String.Join(",", strArr));
                }
                fileWriter.Flush();
                fileWriter.Close();
            }
        }

        /// <summary>
        ///     写入CSV文件
        /// </summary>
        /// <typeparam name="T">范型</typeparam>
        /// <param name="filePathName">文件路径</param>
        /// <param name="append">是否追加写入，false时将重写文件</param>
        /// <param name="listObj">范型LIST</param>
        public static void WriteFile<T>(string filePathName, bool append, List<T> listObj)
        {
            using (var fileWriter = new StreamWriter(filePathName, append, Encoding.Default))
            {
                List<PropertyInfo> props = typeof(T).GetProperties(BindingFlags.SetProperty | BindingFlags.Public).ToList();
                props.Sort(ComparePropertyByName);
                string line = "";

                foreach (PropertyInfo propinfo in props)
                {
                    line += FormatCell(propinfo.Name) + ",";
                }
                fileWriter.WriteLine(line.TrimEnd(','));

                foreach (T t in listObj)
                {
                    line = "";
                    for (int i = 0; i < props.Count; i++)
                    {
                        line += FormatCell(typeof(T).GetField(props[0].Name).GetValue(t).ToString()) + ",";
                    }
                    fileWriter.WriteLine(line.TrimEnd(','));
                }
                fileWriter.Flush();
                fileWriter.Close();
            }
        }

        /// <summary>
        ///     写入CSV文件
        /// </summary>
        /// <param name="filePathName">文件路径</param>
        /// <param name="append">是否追加写入，false时将重写文件</param>
        /// <param name="dataSource">DataTable数据源</param>
        public static void WriteFile(string filePathName, bool append, DataTable dataSource)
        {
            using (var fileWriter = new StreamWriter(filePathName, append, Encoding.Default))
            {
                string line = "";
                //添加列名
                foreach (DataColumn column in dataSource.Columns)
                {
                    line += FormatCell(column.ColumnName) + ",";
                }
                fileWriter.WriteLine(line.TrimEnd(','));

                foreach (DataRow dr in dataSource.Rows)
                {
                    line = "";
                    for (int i = 0; i < dataSource.Columns.Count; i++)
                    {
                        line += FormatCell(dr[i].ToString()) + ",";
                    }
                    fileWriter.WriteLine(line.TrimEnd(','));
                }

                fileWriter.Flush();
                fileWriter.Close();
            }
        }

        /// <summary>
        ///     写入内存流对象
        /// </summary>
        /// <param name="listString">字符串数组LIST</param>
        /// <returns>MemoryStream</returns>
        public static MemoryStream WriteStream(List<String[]> listString)
        {
            var csvStream = new MemoryStream();
            using (var fileWriter = new StreamWriter(csvStream, Encoding.Default))
            {
                foreach (var strArr in listString)
                {
                    for (int i = 0; i < strArr.Length; i++)
                    {
                        strArr[i] = FormatCell(strArr[i]);
                    }
                    fileWriter.WriteLine(String.Join(",", strArr));
                }
                fileWriter.Flush();
                fileWriter.Close();
            }
            return csvStream;
        }

        /// <summary>
        ///     写入内存流对象
        /// </summary>
        /// <typeparam name="T">范型</typeparam>
        /// <param name="listObj">范型LIST</param>
        /// <returns>MemoryStream</returns>
        public static MemoryStream WriteStream<T>(List<T> listObj)
        {
            var csvStream = new MemoryStream();
            using (var fileWriter = new StreamWriter(csvStream, Encoding.Default))
            {
                List<PropertyInfo> props = typeof(T).GetProperties(BindingFlags.SetProperty | BindingFlags.Public).ToList();
                props.Sort(ComparePropertyByName);
                string line = "";

                foreach (PropertyInfo propinfo in props)
                {
                    line += FormatCell(propinfo.Name) + ",";
                }
                fileWriter.WriteLine(line.TrimEnd(','));

                foreach (T t in listObj)
                {
                    line = "";
                    for (int i = 0; i < props.Count; i++)
                    {
                        line += FormatCell(typeof(T).GetField(props[0].Name).GetValue(t).ToString()) + ",";
                    }
                    fileWriter.WriteLine(line.TrimEnd(','));
                }
                fileWriter.Flush();
                fileWriter.Close();
            }
            return csvStream;
        }

        /// <summary>
        ///     写入内存流对象
        /// </summary>
        /// <param name="dataSource">DataTable数据源</param>
        /// <returns>MemoryStream</returns>
        public static MemoryStream WriteStream(DataTable dataSource)
        {
            var csvStream = new MemoryStream();
            using (var fileWriter = new StreamWriter(csvStream, Encoding.Default))
            {
                string line = "";
                //添加列名
                foreach (DataColumn column in dataSource.Columns)
                {
                    line += FormatCell(column.ColumnName) + ",";
                }
                fileWriter.WriteLine(line.TrimEnd(','));

                foreach (DataRow dr in dataSource.Rows)
                {
                    line = "";
                    for (int i = 0; i < dataSource.Columns.Count; i++)
                    {
                        line += FormatCell(dr[i].ToString()) + ",";
                    }
                    fileWriter.WriteLine(line.TrimEnd(','));
                }

                fileWriter.Flush();
                fileWriter.Close();
            }
            return csvStream;
        }

        /// <summary>
        ///     解析 CVS 文件内容为一个二维数组。
        /// </summary>
        /// <param name="src">CVS 文件内容字符串</param>
        /// <returns>二维数组。String[line count][column count]</returns>
        public static String[][] Read(String src)
        {
            // 如果输入为空，返回 0 长度字符串数组
            if (src == null || src.Length == 0) return new String[0][] { };
            String st = "";
            var lines = new ArrayList(); // 行集合。其元素为行
            var cells = new ArrayList(); // 单元格集合。其元素为一个单元格
            bool beginWithQuote = false;
            int maxColumns = 0;
            // 遍历字符串的字符
            for (int i = 0; i < src.Length; i++)
            {
                char ch = src[i];

                #region CR 或者 LF

                //A record separator may consist of a line feed (ASCII/LF=0x0A),
                //or a carriage return and line feed pair (ASCII/CRLF=0x0D 0x0A).
                // 这里“容错”一下，CRLF、LFCR、CR、LF都作为separator
                if (ch == '\r')
                {
                    #region CR

                    if (beginWithQuote)
                    {
                        st += ch;
                    }
                    else
                    {
                        if (i + 1 < src.Length && src[i + 1] == '\n')
                        {
                            // 如果紧接的是LF，那么直接把LF吃掉
                            i++;
                        }

                        //line = new String[cells.Count];
                        //System.Array.Copy (cells.ToArray(typeof(String)), line, line.Length);
                        //lines.Add(line); // 把上一行放到行集合中去

                        cells.Add(st);
                        st = "";
                        beginWithQuote = false;

                        maxColumns = (cells.Count > maxColumns ? cells.Count : maxColumns);
                        lines.Add(cells);
                        st = "";
                        cells = new ArrayList();
                    }

                    #endregion CR
                }
                else if (ch == '\n')
                {
                    #region LF

                    if (beginWithQuote)
                    {
                        st += ch;
                    }
                    else
                    {
                        if (i + 1 < src.Length && src[i + 1] == '\r')
                        {
                            // 如果紧接的是LF，那么直接把LF吃掉
                            i++;
                        }

                        //line = new String[cells.Count];
                        //System.Array.Copy (cells.ToArray(typeof(String)), line, line.Length);
                        //lines.Add(line); // 把上一行放到行集合中去

                        cells.Add(st);
                        st = "";
                        beginWithQuote = false;

                        maxColumns = (cells.Count > maxColumns ? cells.Count : maxColumns);
                        lines.Add(cells);
                        st = "";
                        cells = new ArrayList();
                    }

                    #endregion LF
                }

                #endregion CR 或者 LF

                else if (ch == '\"')
                {
                    // 双引号

                    #region 双引号

                    if (beginWithQuote)
                    {
                        i++;
                        if (i >= src.Length)
                        {
                            cells.Add(st);
                            st = "";
                            beginWithQuote = false;
                        }
                        else
                        {
                            ch = src[i];
                            if (ch == '\"')
                            {
                                st += ch;
                            }
                            else if (ch == ',')
                            {
                                cells.Add(st);
                                st = "";
                                beginWithQuote = false;
                            }
                            else
                            {
                                throw new Exception("Single double-quote char mustn't exist in filed " +
                                                    (cells.Count + 1) + " while it is begined with quote\nchar at:" + i);
                            }
                        }
                    }
                    else if (st.Length == 0)
                    {
                        beginWithQuote = true;
                    }
                    else
                    {
                        throw new Exception("Quote cannot exist in a filed which doesn't begin with quote!\nfield:" +
                                            (cells.Count + 1));
                    }

                    #endregion 双引号
                }
                else if (ch == ',')
                {
                    #region 逗号

                    if (beginWithQuote)
                    {
                        st += ch;
                    }
                    else
                    {
                        cells.Add(st);
                        st = "";
                        beginWithQuote = false;
                    }

                    #endregion 逗号
                }
                else
                {
                    #region 其它字符

                    st += ch;

                    #endregion 其它字符
                }
            }
            if (st.Length != 0)
            {
                if (beginWithQuote)
                {
                    throw new Exception("last field is begin with but not end with double quote");
                }
                cells.Add(st);
                maxColumns = (cells.Count > maxColumns ? cells.Count : maxColumns);
                lines.Add(cells);
            }

            var ret = new String[lines.Count][];
            for (int i = 0; i < ret.Length; i++)
            {
                cells = (ArrayList)lines[i];
                ret[i] = new String[maxColumns];
                for (int j = 0; j < maxColumns; j++)
                {
                    ret[i][j] = cells[j].ToString();
                }
            }
            //System.Array.Copy(lines.ToArray(typeof(String[])), ret, ret.Length);
            return ret;
        }

        /// <summary>
        /// 分割 CVS 文件内容为DataTable
        /// </summary>
        /// <param name="src">CVS 文件内容字符串</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable(string str)
        {
            DataTable dt = new DataTable();
            bool bwq = false;
            char[] c = str.ToCharArray();
            ArrayList rows = new ArrayList();
            ArrayList cells = new ArrayList();
            string tempStr = string.Empty;

            for (int i = 0; i < c.Length; i++)
            {
                switch (c[i])
                {
                    #region 双引号
                    case '"':
                        {
                            if (bwq)
                            {
                                i++;
                                if (i < c.Length)
                                {
                                    if (c[i] == '"')
                                        tempStr += c[i];
                                    else if (c[i] == ',')
                                    {
                                        cells.Add(tempStr);
                                        tempStr = string.Empty;
                                        bwq = false;
                                    }
                                    else if (c[i] == '\n')
                                    {
                                        cells.Add(tempStr);
                                        tempStr = string.Empty;
                                        bwq = false;
                                        rows.Add(cells);
                                        cells = new ArrayList();
                                    }
                                    else
                                    {
                                        //tempStr+=c[i];
                                        bwq = false;
                                    }
                                }
                                else
                                {
                                    cells.Add(tempStr);
                                    tempStr = string.Empty;
                                    rows.Add(cells);
                                }
                            }
                            else if (tempStr == string.Empty)
                            {
                                bwq = true;
                            }
                            else
                            {
                                i++;
                                if (i < c.Length)
                                {
                                    tempStr += c[i];
                                }
                            }
                            break;
                        }
                    #endregion

                    #region 逗号
                    case ',':
                        {
                            if (bwq)
                                tempStr += c[i];
                            else
                            {
                                cells.Add(tempStr);
                                tempStr = string.Empty;
                            }
                            break;
                        }
                    #endregion

                    #region 换行符
                    case '\n':
                        {
                            if (c[i - 1] == '\r')
                            {
                                if (c[i - 2] != ',')
                                {
                                    cells.Add(tempStr);
                                }
                                rows.Add(cells);
                                cells = new ArrayList();
                                tempStr = string.Empty;
                            }
                            break;
                        }
                    #endregion

                    #region 回车
                    case '\r':
                        {
                            break;
                        }
                    #endregion

                    default:
                        {
                            if (i + 1 > c.Length)
                            {
                                tempStr += c[i];
                                cells.Add(tempStr);
                                rows.Add(cells);
                                cells = new ArrayList();
                                tempStr = string.Empty;
                            }
                            else
                            {
                                tempStr += c[i];
                            }
                            break;
                        }

                }
            }

            if (rows != null && rows.Count > 0)
            {
                //添加列
                cells = (ArrayList)rows[0];
                for (int ci = 0; ci < cells.Count; ci++)
                {
                    dt.Columns.Add(cells[ci].ToString(), typeof(string));
                }

                //添加行
                for (int i = 1; i < rows.Count; i++)
                {
                    cells = (ArrayList)rows[i];

                    DataRow dr = dt.NewRow();
                    for (int ci = 0; ci < cells.Count; ci++)
                    {
                        dr[ci] = cells[ci].ToString();
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        /// <summary>
        ///     类属性排序比较函数,
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>x比y大返回1, 相等返回0, 小返回-1</returns>
        private static int ComparePropertyByName(PropertyInfo x, PropertyInfo y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                return -1;
            }
            if (y == null)
            {
                return 1;
            }
            return x.Name.CompareTo(y.Name);
        }

        /// <summary>
        ///     格式化CSV单元格字符串值
        /// </summary>
        /// <param name="cellText"></param>
        /// <returns></returns>
        public static string FormatCell(string cellText)
        {
            cellText = cellText.Replace("\"", "\"\"");
            return "\"" + cellText + "\"";
        }
    }
}