using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClassicAssist.Annotations;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootConstraints : INotifyPropertyChanged, IComparable<AutolootConstraints>
    {
        private int _clilocIndex;
        private int[] _clilocs;
        private AutolootConstraintType _constraintType;
        private string _name;
        private AutolootOperator _operator = AutolootOperator.Equal;
        private int _value;

        public int ClilocIndex
        {
            get => _clilocIndex;
            set => SetProperty( ref _clilocIndex, value );
        }

        public int[] Clilocs
        {
            get => _clilocs;
            set => SetProperty( ref _clilocs, value );
        }

        public AutolootConstraintType ConstraintType
        {
            get => _constraintType;
            set => SetProperty( ref _constraintType, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
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

        public int CompareTo( AutolootConstraints other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            if ( ReferenceEquals( null, other ) )
            {
                return 1;
            }

            int nameComparison = string.Compare( _name, other._name, StringComparison.Ordinal );

            return nameComparison;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return Name;
        }

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
            CommandManager.InvalidateRequerySuggested();
        }
    }
}