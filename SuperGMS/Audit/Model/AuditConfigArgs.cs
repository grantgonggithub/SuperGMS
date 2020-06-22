using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperGMS.Audit.Model
{
    internal class AuditConfigArgs
    {
        public string TtId { get; set; }
        public string Host { get; set; }
        public string DbName { get; set; }
        public string TableNames { get; set; }
}
}
