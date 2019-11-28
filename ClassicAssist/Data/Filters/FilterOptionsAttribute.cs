using System;

namespace ClassicAssist.Data.Filters
{
    public class FilterOptionsAttribute : Attribute
    {
        public string Name { get; set; }
        public bool DefaultEnabled { get; set; }

        public FilterOptionsAttribute()
        {
        }
    }
}