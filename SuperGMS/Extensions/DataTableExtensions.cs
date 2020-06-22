using System;
using System.Collections.Generic;
using System.Data;

namespace SuperGMS.Extensions
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// 按固定次数扩充DataTable行数据,新Table会追加新列EXPAND_ROW_INTERNAL_SEQ(如果已存在,则增加 EXPAND_ROW_INTERNAL_SEQ1)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="times">行扩充(复制)到的次数, 必须大于0</param>
        /// <param name="newTableName">新TableName,不传默认为原始表名</param>
        /// <returns>新Table</returns>
        public static DataTable ExpandRowsByFixTime(this DataTable dt, int times, string newTableName = null)
        {
            if (times <= 0)
            {
                throw new Exception("扩充次数必须大于0");
            }

            DataTable newTable = dt.Clone();
            DataColumn newCol = null;
            if (!newTable.Columns.Contains("EXPAND_ROW_INTERNAL_SEQ"))
            {
                newCol = new DataColumn("EXPAND_ROW_INTERNAL_SEQ", typeof(int));
            }
            else
            {
                newCol = new DataColumn("EXPAND_ROW_INTERNAL_SEQ1", typeof(int));
            }
            newTable.Columns.Add(newCol);

            if (newTable.PrimaryKey.Length > 0)
            {
                DataColumn[] newPrimayKey = new DataColumn[newTable.PrimaryKey.Length + 1];
                for (int columnIndex = 0; columnIndex < newTable.PrimaryKey.Length; columnIndex++)
                {
                    newPrimayKey[columnIndex] = newTable.PrimaryKey[columnIndex];
                }
                newPrimayKey[newPrimayKey.Length - 1] = newCol;
                newTable.PrimaryKey = newPrimayKey;
            }

            foreach (DataRow row in dt.Rows)
            {
                object[] objs = new object[row.ItemArray.Length + 1];
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    objs[i] = row.ItemArray[i];
                }

                for (int i = 0; i < times; i++)
                {
                    objs[row.ItemArray.Length] = i + 1;
                    newTable.Rows.Add(objs);
                }
            }

            if (!string.IsNullOrEmpty(newTableName)) newTable.TableName = newTableName;
            return newTable;
        }
        /// <summary>
        /// 按指定列数值扩充DataTable行数据,新Table会追加新列EXPAND_ROW_INTERNAL_SEQ(如果已存在,则增加 EXPAND_ROW_INTERNAL_SEQ1)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="colName">指定列明,如行项该列数据转换int异常或小于等于0,则仅保留原行</param>
        /// <param name="newTableName">新TableName,不传默认为原始表名</param>
        /// <returns>新Table</returns>
        public static DataTable ExpandRowsBySpecificColumn(this DataTable dt, string colName, string newTableName = null)
        {
            if (string.IsNullOrEmpty(colName) || !dt.Columns.Contains(colName))
            {
                throw new Exception("请提供正确的列名");
            }
            DataTable newTable = dt.Clone();

            DataColumn timeCol = null;
            if (!newTable.Columns.Contains("EXPAND_ROW_INTERNAL_SEQ"))
            {
                timeCol = new DataColumn("EXPAND_ROW_INTERNAL_SEQ", typeof(int));
            }
            else
            {
                timeCol = new DataColumn("EXPAND_ROW_INTERNAL_SEQ1", typeof(int));
            }
            newTable.Columns.Add(timeCol);

            if (newTable.PrimaryKey.Length > 0)
            {
                //添加主键列
                DataColumn[] newPrimayKey = new DataColumn[newTable.PrimaryKey.Length + 1];
                for (int i = 0; i < newTable.PrimaryKey.Length; i++)
                {
                    newPrimayKey[i] = newTable.PrimaryKey[i];
                }
                newPrimayKey[newPrimayKey.Length - 1] = timeCol;
                newTable.PrimaryKey = newPrimayKey;
            }
            //行数据扩充次数
            int repeatTimes = 1;
            foreach (DataRow row in dt.Rows)
            {
                int.TryParse(row[colName].ToString(), out repeatTimes);
                //如果指定列的数值转换为整形异常或小于等于0
                if (repeatTimes <= 0)
                {
                    repeatTimes = 1;
                }
                object[] objs = new object[row.ItemArray.Length + 1];
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    objs[i] = row.ItemArray[i];
                }

                for (int i = 0; i < repeatTimes; i++)
                {
                    objs[row.ItemArray.Length] = i + 1;
                    newTable.Rows.Add(objs);
                }
            }
            if (!string.IsNullOrEmpty(newTableName)) newTable.TableName = newTableName;
            return newTable;
        }
    }
}
