using ClassicAssist.Data.Autoloot;
using ClassicAssist.UI.ViewModels;

namespace ClassicAssist.UI.Models
{
    public class EntityCollectionFilter : SetPropertyNotifyChanged
    {
        private PropertyEntry _constraint;
        private AutolootOperator _operator = AutolootOperator.Equal;
        private int _value;

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