using System;

namespace ClassicAssist.Data.Filters
{
    public class FilterOptionsAttribute : Attribute
    {
        public bool DefaultEnabled { get; set; }
        public string Name { get; set; }
    }
}