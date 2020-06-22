//
// �ļ���QueryMethod.cs
// ���ߣ�Grant
// ���������ڣ�2014-06-05 14:29

#region

using System;
using System.ComponentModel;

#endregion

namespace SuperGMS.DB.EFEx.DynamicSearch.Model
{
    /// <summary>
    ///     Html��Ԫ�صļ�����ʽ
    ///     Add by Grant 2014-3-27
    /// </summary>
    public enum QueryMethod
    {
        /// <summary>
        ///     ����
        /// </summary>
        //[GlobalCode("=", OnlyAttribute = true)]
        Equal = 0,

        /// <summary>
        ///     С��
        /// </summary>
        //// [GlobalCode("<", OnlyAttribute = true)]
        LessThan = 1,

        /// <summary>
        ///     ����
        /// </summary>
        // [GlobalCode(">", OnlyAttribute = true)]
        GreaterThan = 2,

        /// <summary>
        ///     С�ڵ���
        /// </summary>
        // [GlobalCode("<=", OnlyAttribute = true)]
        LessThanOrEqual = 3,

        /// <summary>
        ///     ���ڵ���
        /// </summary>
        // [GlobalCode(">=", OnlyAttribute = true)]
        GreaterThanOrEqual = 4,

        /// <summary>
        ///     ����һ��ʱ���ȡ��ǰ���ʱ������, ToSqlδʵ�֣���ʵ����IQueryable
        /// </summary>
        // [GlobalCode("between", OnlyAttribute = true)]
        [Obsolete("û��ʵ�ִ����Թ���")]
        DateBlock = 8,

        /// <summary>
        ///     ������
        /// </summary>
        NotEqual = 9,

        /// <summary>
        ///     ��ʼ��
        /// </summary>
        StartsWith = 10,

        /// <summary>
        ///     ������
        /// </summary>
        EndsWith = 11,

        /// <summary>
        ///     ����Like������
        /// </summary>
        Contains = 12,

        /// <summary>
        ///     ����In������
        /// </summary>
        StdIn = 13,

        /// <summary>
        ///     ����Not In������
        /// </summary>
        StdNotIn = 14,

        /// <summary>
        /// ������
        /// </summary>
        NotLike = 15,
    }
}