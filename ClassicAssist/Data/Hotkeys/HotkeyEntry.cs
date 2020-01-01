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
        private object _o;

        public HotkeyEntry( string name, bool isCategory )
        {
            Name = name;
            IsCategory = isCategory;
        }

        public ObservableCollectionEx<HotkeySettable> Children
        {
            get => _children;
            set => SetProperty( ref _children, value );
        }

        public bool IsCategory { get; }

        public string Name { get; }

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

            return other is null ? 1 : string.Compare( Name, other.Name, StringComparison.Ordinal );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override bool Equals( object obj )
        {
            if ( !( obj is HotkeyEntry hke ) )
            {
                return false;
            }

            return Name == hke.Name && IsCategory == hke.IsCategory;
        }

        protected bool Equals( HotkeyEntry other )
        {
            return IsCategory == other.IsCategory && Name == other.Name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ( IsCategory.GetHashCode() * 397 ) ^ ( Name != null ? Name.GetHashCode() : 0 );
            }
        }

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
            return Name;
        }
    }
}