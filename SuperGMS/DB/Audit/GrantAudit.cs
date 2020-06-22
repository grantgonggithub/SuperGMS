using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.DB.Audit
{
    /// <summary>
    /// 审计消息内容
    /// </summary>
    public class GrantAudit
    {
        public GrantAudit()
        {
            Properties = new List<FieldProperty>();
            CreatedDate = DateTime.Now;
        }
        /// <summary>
        /// ttid
        /// </summary>
        public string Ttid { get; set; }
        public string UserId { get; set; }
        public string TableName { get; set; }
        /// <summary>
        /// 联合主键，需要字符串连接
        /// </summary>
        public string PrimarkKey { get; set; }
        public string StateName { get; set; }

        public bool IsSql { get; set; }
        public string Sql { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<FieldProperty> Properties { get; set; }

    }

    public class FieldProperty
    {

        /// <summary>Gets or sets the name of the property audited.</summary>
        /// <value>The name of the property audited.</value>
        public string PropertyName { get; set; }

        /// <summary>Gets or sets the name of the relation audited.</summary>
        /// <value>The name of the relation audited.</value>
        public string RelationName { get; set; }

        /// <summary>Gets or sets the name of the property internally.</summary>
        /// <value>The name of the property internally.</value>
        public string InternalPropertyName { get; set; }

        /// <summary>Gets or sets the new value audited formatted.</summary>
        /// <value>The new value audited formatted.</value>
        public string NewValueFormatted { get; set; }

        public string OldValueFormatted { get; set; }
    }
}
