using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClassicAssist.Annotations;
using ClassicAssist.Data.Autoloot;

namespace ClassicAssist.UI.Models
{
    public class EntityCollectionFilter : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        public virtual void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
        }
    }
}