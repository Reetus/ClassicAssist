using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.ECV.Filter.Models
{
    public class EntityCollectionFilterItem : SetPropertyNotifyChanged
    {
        private string _additional;
        private PropertyEntry _constraint;
        private AutolootOperator _operator = AutolootOperator.Equal;
        private int _value;

        public string Additional
        {
            get => _additional;
            set => SetProperty( ref _additional, value );
        }

        public PropertyEntry Constraint
        {
            get => _constraint;
            set => SetProperty( ref _constraint, value );
        }

        public AutolootOperator Operator
        {
            get => _operator;
            set => SetProperty( ref _operator, value );
        }

        public int Value
        {
            get => _value;
            set => SetProperty( ref _value, value );
        }
    }
}