using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ClassicAssist.Data.Skills;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class SetSkillLockData
    {
        public LockStatus LockStatus { get; set; }
        public IEnumerable<SkillEntry> SkillEntries { get; set; }
    }

    public class SetSkillLockMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IList items = (IList)((BindingProxy)values[0]).Data;
            IEnumerable<SkillEntry> skills = items.Cast<SkillEntry>();

            LockStatus lockStatus = (LockStatus)values[1];

            return new SetSkillLockData { SkillEntries = skills, LockStatus = lockStatus };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}