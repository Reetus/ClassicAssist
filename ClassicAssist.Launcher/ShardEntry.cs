using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClassicAssist.Launcher.Annotations;

namespace ClassicAssist.Launcher
{
    public class ShardEntry : INotifyPropertyChanged
    {
        private const string RUNUO_REGEX = @".*Clients=(\d+),.*";
        private string _address;
        private bool _encryption;
        private string _name;
        private string _ping;
        private int _port;
        private int _shardType;

        private string _status;
        private string _website;

        public string Address
        {
            get => _address;
            set => SetProperty( ref _address, value );
        }

        public bool Encryption
        {
            get => _encryption;
            set => SetProperty( ref _encryption, value );
        }

        public bool HasStatusProtocol { get; set; } = true;
        public bool IsPreset { get; set; } = false;

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public string Ping
        {
            get => _ping;
            set => SetProperty( ref _ping, value );
        }

        public int Port
        {
            get => _port;
            set => SetProperty( ref _port, value );
        }

        public int ShardType
        {
            get => _shardType;
            set => SetProperty( ref _shardType, value );
        }

        public string Status
        {
            get => _status;
            set => SetProperty( ref _status, value );
        }

        public string StatusRegex { get; set; } = RUNUO_REGEX;

        public string Website
        {
            get => _website;
            set => SetProperty( ref _website, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // ReSharper disable once RedundantAssignment
        public virtual void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
            CommandManager.InvalidateRequerySuggested();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}