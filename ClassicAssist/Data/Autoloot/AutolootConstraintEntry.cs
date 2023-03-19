using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootConstraintEntry : SetPropertyNotifyChanged
    {
        private AutolootOperator _operator = AutolootOperator.Equal;
        private PropertyEntry _property;
        private int _value;
        private string _additional;

        public AutolootOperator Operator
        {
            get => _operator;
            set => SetProperty( ref _operator, value );
        }

        public PropertyEntry Property
        {
            get => _property;
            set => SetProperty( ref _property, value );
        }

        public int Value
        {
            get => _value;
            set => SetProperty( ref _value, value );
        }

        public string Additional
        {
            get => _additional;
            set => SetProperty( ref _additional, value );
        }
    }
}