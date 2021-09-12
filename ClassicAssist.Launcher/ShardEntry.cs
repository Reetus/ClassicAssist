using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClassicAssist.Launcher.Annotations;
using Newtonsoft.Json;

namespace ClassicAssist.Launcher
{
    public class ShardEntry : INotifyPropertyChanged, IEquatable<ShardEntry>
    {
        private const string RUNUO_REGEX = @".*Clients=(\d+),.*";
        private string _address;
        private bool _deleted;
        private bool _encryption;
        private string _name;
        private string _ping;
        private int _port;
        private int _shardType;

        private string _status;
        private string _website;

        [JsonProperty( "address" )]
        public string Address
        {
            get => _address;
            set => SetProperty( ref _address, value );
        }

        [JsonIgnore]
        public bool Deleted
        {
            get => _deleted;
            set => SetProperty( ref _deleted, value );
        }

        [JsonProperty( "encryption" )]
        public bool Encryption
        {
            get => _encryption;
            set => SetProperty( ref _encryption, value );
        }

        [JsonProperty( "has_status_protocol" )]
        public bool HasStatusProtocol { get; set; } = true;

        [JsonIgnore]
        public bool IsPreset { get; set; }

        [JsonProperty( "name" )]
        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        [JsonIgnore]
        public string Ping
        {
            get => _ping;
            set => SetProperty( ref _ping, value );
        }

        [JsonProperty( "port" )]
        public int Port
        {
            get => _port;
            set => SetProperty( ref _port, value );
        }

        [JsonProperty( "shard_type" )]
        public int ShardType
        {
            get => _shardType;
            set => SetProperty( ref _shardType, value );
        }

        [JsonIgnore]
        public string Status
        {
            get => _status;
            set => SetProperty( ref _status, value );
        }

        [JsonIgnore]
        public string StatusRegex { get; set; } = RUNUO_REGEX;

        [JsonProperty( "website" )]
        public string Website
        {
            get => _website;
            set => SetProperty( ref _website, value );
        }

        public bool Equals( ShardEntry other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }

            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }

            return _name == other._name && IsPreset == other.IsPreset;
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

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) )
            {
                return false;
            }

            if ( ReferenceEquals( this, obj ) )
            {
                return true;
            }

            if ( obj.GetType() != GetType() )
            {
                return false;
            }

            return Equals( (ShardEntry) obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ( ( _name != null ? _name.GetHashCode() : 0 ) * 397 ) ^ IsPreset.GetHashCode();
            }
        }
    }
}