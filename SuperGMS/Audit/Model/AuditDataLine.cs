using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperGMS.Audit.Model
{
    public class AuditDataLine
    {
        public string AuditId { get; set; }
        public string PropertyName { get; set; }

        public string NewValueFormatted { get; set; }

        public string OldValueFormatted { get; set; }
    }
}
