using System;

namespace TestModel.Models
{
    [Serializable]
    public partial class itemDetail
    {
        public string ItemGID { get; set; }
        public string GUID { get; set; }
        public string LineName { get; set; }

        public Nullable<int> LineId { get; set; }
        public virtual item item { get; set; }
        public System.DateTime TS { get; set; }
    }
}
