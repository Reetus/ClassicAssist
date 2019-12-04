using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.Data.Hotkeys
{
    public class HotkeyEntry : INotifyPropertyChanged, IComparable<HotkeyEntry>
    {
        private ObservableCollectionEx<HotkeySettable> _children;
        private bool _isCategory;
        private string _name;
        private object _o;

        public ObservableCollectionEx<HotkeySettable> Children
        {
            get => _children;
            set => SetProperty( ref _children, value );
        }

        public bool IsCategory
        {
            get => _isCategory;
            set => SetProperty( ref _isCategory, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public object Object
        {
            get => _o;
            set => SetProperty( ref _o, value );
        }

        public int CompareTo( HotkeyEntry other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            return other is null ? 1 : string.Compare( _name, other._name, StringComparison.Ordinal );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        private void SetProperty<T>( ref T name, T value, [CallerMemberName] string propertyName = "" )
        {
            name = value;
            OnPropertyChanged( propertyName );
        }

        public override string ToString()
        {
            return _name;
        }
    }
}