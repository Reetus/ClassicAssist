using System;
using System.Windows;
using System.Windows.Controls;

namespace ClassicAssist.UI.Misc
{
    public class SkillsGridViewColumn : GridViewColumn
    {
        public enum Enums
        {
            Name,
            Value,
            Base,
            Delta,
            Cap,
            LockStatus
        }

        public Enums SortField
        {
            get => (Enums)GetValue(SortFieldProperty);
            set => SetValue(SortFieldProperty, value);
        }

        public static readonly DependencyProperty SortFieldProperty =
            DependencyProperty.Register("SortField", typeof(Enums), typeof(SkillsGridViewColumn), new UIPropertyMetadata(null));

    }
}