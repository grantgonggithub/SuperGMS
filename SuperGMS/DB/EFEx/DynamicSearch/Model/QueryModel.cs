//
// �ļ���QueryModel.cs
// ���ߣ�Grant
// ���������ڣ�2014-06-05 14:29

#region

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#endregion

namespace SuperGMS.DB.EFEx.DynamicSearch.Model
{
    /// <summary>
    ///     �û��Զ��ռ�����������Model
    ///     Add by Grant 2014-3-27
    /// </summary>
    [DataContract]
    [Serializable]
    public class QueryModel
    {
        /// <summary>
        /// Ĭ�Ϲ��캯��
        /// </summary>
        public QueryModel()
        {
            Items = new List<ConditionItem>();
        }

        /// <summary>
        ///     ��ѯ����
        /// </summary>
        [DataMember]
        public List<ConditionItem> Items { get; set; }
    }
}