using System.Collections.Generic;

namespace SuperGMS.ApiHelper
{
    public class FieldDescInfo
    {
        public string FieldName { get; set; }
        public string ControlType { get; set; }
        public string GroupName { get; set; }
        public int IsHidden { get; set; }
        public string DefaultValue { get; set; }
        public int ReadOnly { get; set; }
        public int EditReadOnly { get; set; }
        public int CanHidden { get; set; }
        public int IsRequired { get; set; }

        public string ValidateRule { get; set; }
        public int MaxLength { get; set; }
        public decimal NumberMax { get; set; }
        public decimal NumberMin { get; set; }
        public int IsDigits { get; set; }
        public int DecimalDigits { get; set; }
        public string DateFormat { get; set; }
        public string DateDefaultTime { get; set; }

        public Dictionary<string,string> DicSource { get; set; }
    }
}
