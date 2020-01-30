using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Resources;

namespace ClassicAssist.Data.Filters
{
    public abstract class FilterEntry : INotifyPropertyChanged
    {
        private bool _enabled;
        private bool _isConfigurable;
        private string _name;

        protected FilterEntry()
        {
            Action = OnChanged;

            FilterOptionsAttribute a =
                (FilterOptionsAttribute) Attribute.GetCustomAttribute( GetType(), typeof( FilterOptionsAttribute ) );

            if ( a == null )
            {
                return;
            }

            string resourceName = Strings.ResourceManager.GetString( a.Name );

            if ( string.IsNullOrEmpty( resourceName ) )
            {
                throw new InvalidOperationException( $"No localized text for filter: {a.Name}" );
            }

            Name = string.IsNullOrEmpty( resourceName ) ? a.Name : resourceName;
            Enabled = a.DefaultEnabled;
            IsConfigurable = typeof( IConfigurableFilter ).IsAssignableFrom( GetType() );
        }

        public Action<bool> Action { get; set; }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                SetProperty( ref _enabled, value );
                Action?.Invoke( value );
            }
        }

        public bool IsConfigurable
        {
            get => _isConfigurable;
            set => SetProperty( ref _isConfigurable, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnChanged( bool enabled )
        {
            throw new NotImplementedException();
        }

        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        public void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = null )
        {
            field = value;
            OnPropertyChanged( propertyName );
        }
    }
}