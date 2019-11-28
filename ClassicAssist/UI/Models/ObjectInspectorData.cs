using System;

namespace ClassicAssist.UI.Models
{
    public class ObjectInspectorData
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Category { get; set; }
        public bool IsExpanded { get; set; } = true;
        public Action<object> OnDoubleClick { get; set; }
    }
}