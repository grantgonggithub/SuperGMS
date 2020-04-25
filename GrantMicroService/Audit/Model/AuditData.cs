using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrantMicroService.Audit.Model
{
    /// <summary>
    /// 审计记录
    /// </summary>
    public class AuditData
    {
        public AuditData()
        {
            Properties = new List<AuditDataLine>();
            CreatedDate = DateTime.Now;
        }
        public string AuditId { get; set; }
        /// <summary>
        /// ttid
        /// </summary>
        public string Ttid { get; set; }
        public string SysId { get; set; }
        public string UserId { get; set; }
        public string TableName { get; set; }
        /// <summary>
        /// 联合主键，需要字符串连接,
        /// </summary>
        public string PrimarkKey { get; set; }
        public string TransactionId { get; set; }
        public string StateName { get; set; }

        public bool IsSql { get; set; }
        public string Sql { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<AuditDataLine> Properties { get; set; }
    }
}
