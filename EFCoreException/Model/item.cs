using System;
using System.Collections.Generic;

namespace TestModel.Models
{
    [Serializable]
    public partial class item
    {
        public item()
        {
            this.itemDetail = new List<itemDetail>();
        }
        public string ItemGID { get; set; }
        public string ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemDesc { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public Nullable<decimal> Volumn { get; set; }
        public string UDF01 { get; set; }
        public string UDF02 { get; set; }
        public string UDF03 { get; set; }
        public DateTime CreatedDate { get; set; }
        public Nullable<DateTime> UpdatedDate { get; set; }
        public System.DateTime TS { get; set; }
        public virtual ICollection<itemDetail> itemDetail { get; set; }

    }
}
