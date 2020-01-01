using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Assistant;
using ClassicAssist.Annotations;
using ClassicAssist.Data.Friends;
using ClassicAssist.Misc;
using ClassicAssist.UI.ViewModels;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data
{
    public class Options : INotifyPropertyChanged
    {
        public const string DEFAULT_SETTINGS_FILENAME = "settings.json";
        private static string _profilePath;
        private bool _actionDelay;
        private int _actionDelayMs;
        private bool _alwaysOnTop;
        private char _commandPrefix = '+';
        private bool _debug;
        private ObservableCollection<FriendEntry> _friends = new ObservableCollection<FriendEntry>();
        private bool _includePartyMembersInFriends;
        private int _lightLevel;
        private string _name;
        private bool _persistUseOnce;
        private bool _preventAttackingFriendsInWarMode;
        private bool _preventTargetingFriendsWithHarmful;
        private bool _queueLastTarget;
        private bool _rangeCheckLastTarget;
        private int _rangeCheckLastTargetAmount = 11;
        private SmartTargetOption _smartTargetOption;
        private bool _useDeathScreenWhilstHidden;
        private bool _useExperimentalFizzleDetection;
        private bool _useObjectQueue;
        private int _useObjectQueueAmount = 5;

        public bool ActionDelay
        {
            get => _actionDelay;
            set => SetProperty( ref _actionDelay, value );
        }

        public int ActionDelayMS
        {
            get => _actionDelayMs;
            set => SetProperty( ref _actionDelayMs, value );
        }

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set => SetProperty( ref _alwaysOnTop, value );
        }

        public char CommandPrefix
        {
            get => _commandPrefix;
            set => SetProperty( ref _commandPrefix, value );
        }

        public static Options CurrentOptions { get; set; } = new Options();

        public bool Debug
        {
            get => _debug;
            set => SetProperty( ref _debug, value );
        }

        public ObservableCollection<FriendEntry> Friends
        {
            get => _friends;
            set => SetProperty( ref _friends, value );
        }

        public bool IncludePartyMembersInFriends
        {
            get => _includePartyMembersInFriends;
            set => SetProperty( ref _includePartyMembersInFriends, value );
        }

        public int LightLevel
        {
            get => _lightLevel;
            set => SetProperty( ref _lightLevel, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public bool PersistUseOnce
        {
            get => _persistUseOnce;
            set => SetProperty( ref _persistUseOnce, value );
        }

        public bool PreventAttackingFriendsInWarMode
        {
            get => _preventAttackingFriendsInWarMode;
            set => SetProperty( ref _preventAttackingFriendsInWarMode, value );
        }

        public bool PreventTargetingFriendsWithHarmful
        {
            get => _preventTargetingFriendsWithHarmful;
            set => SetProperty( ref _preventTargetingFriendsWithHarmful, value );
        }

        public bool QueueLastTarget
        {
            get => _queueLastTarget;
            set => SetProperty( ref _queueLastTarget, value );
        }

        public bool RangeCheckLastTarget
        {
            get => _rangeCheckLastTarget;
            set => SetProperty( ref _rangeCheckLastTarget, value );
        }

        public int RangeCheckLastTargetAmount
        {
            get => _rangeCheckLastTargetAmount;
            set => SetProperty( ref _rangeCheckLastTargetAmount, value );
        }

        public SmartTargetOption SmartTargetOption
        {
            get => _smartTargetOption;
            set => SetProperty( ref _smartTargetOption, value );
        }

        public Version UpdateGumpVersion { get; set; }

        public bool UseDeathScreenWhilstHidden
        {
            get => _useDeathScreenWhilstHidden;
            set => SetProperty( ref _useDeathScreenWhilstHidden, value );
        }

        public bool UseExperimentalFizzleDetection
        {
            get => _useExperimentalFizzleDetection;
            set => SetProperty( ref _useExperimentalFizzleDetection, value );
        }

        public bool UseObjectQueue
        {
            get => _useObjectQueue;
            set => SetProperty( ref _useObjectQueue, value );
        }

        public int UseObjectQueueAmount
        {
            get => _useObjectQueueAmount;
            set => SetProperty( ref _useObjectQueueAmount, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static void Save( Options options )
        {
            BaseViewModel[] instances = BaseViewModel.Instances;

            JObject obj = new JObject { { "Name", options.Name } };

            foreach ( BaseViewModel instance in instances )
            {
                if ( instance is ISettingProvider settingProvider )
                {
                    settingProvider.Serialize( obj );
                }
            }

            EnsureProfilePath( Engine.StartupPath ?? Environment.CurrentDirectory );

            File.WriteAllText( Path.Combine( _profilePath, options.Name ?? DEFAULT_SETTINGS_FILENAME ),
                obj.ToString() );
        }

        private static void EnsureProfilePath( string startupPath )
        {
            _profilePath = Path.Combine( startupPath, "Profiles" );

            if ( !Directory.Exists( _profilePath ) )
            {
                Directory.CreateDirectory( _profilePath );
            }
        }

        public static void Load( string optionsFile, Options options )
        {
            BaseViewModel[] instances = BaseViewModel.Instances;

            EnsureProfilePath( Engine.StartupPath ?? Environment.CurrentDirectory );

            JObject json = new JObject();

            string fullPath = Path.Combine( _profilePath, optionsFile );

            if ( File.Exists( fullPath ) )
            {
                json = JObject.Parse( File.ReadAllText( fullPath ) );
            }

            options.Name = options.Name ?? json["Name"]?.ToObject<string>() ?? DEFAULT_SETTINGS_FILENAME;

            foreach ( BaseViewModel instance in instances )
            {
                if ( instance is ISettingProvider settingProvider )
                {
                    settingProvider.Deserialize( json, options );
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        public void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
        }

        public static string[] GetProfiles()
        {
            EnsureProfilePath( Engine.StartupPath ?? Environment.CurrentDirectory );
            return Directory.EnumerateFiles( _profilePath, "*.json" ).ToArray();
        }
    }

    [Flags]
    public enum SmartTargetOption
    {
        None = 0b00,
        Friend = 0b01,
        Enemy = 0b10,
        Both = 0b11
    }
}