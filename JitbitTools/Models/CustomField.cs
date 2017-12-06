namespace JitbitTools.Models
{
    public class CustomField
    {
        public string Value { get; set; }
        public object ValueWithOption { get; set; }
        public string FieldName { get; set; }
        public int? FieldID { get; set; }
        public int? Type { get; set; }
        public int? UsageType { get; set; }
        public bool? ForTechsOnly { get; set; }
        public bool? Mandatory { get; set; }
        public int? OrderByNumber { get; set; }
        public bool? ShowInGrid { get; set; }
        public object SelectionComboOptions { get; set; }
    }
}