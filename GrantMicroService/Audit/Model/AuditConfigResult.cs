using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrantMicroService.Audit.Model
{
    internal class AuditConfigResult
    {
        public string DbName { get; set; }
        public string TableName { get; set; }
        public string Ttid { get; set; }
        public string SysId { get; set; }
        public string WhId { get; set; }
        public string FieldName { get; set; }
        public int IsPrimaryKey { get; set; }
        public int IsAudit { get; set; }
        public string Createdby { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Updatedby { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Remark { get; set; }
    }
}
