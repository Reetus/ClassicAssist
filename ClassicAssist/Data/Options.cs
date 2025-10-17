using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Assistant;
using ClassicAssist.Data.Friends;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Macros;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Misc;
using ClassicAssist.Data.Scavenger;
using ClassicAssist.Misc;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels;
using Newtonsoft.Json.Linq;
using Sentry;

namespace ClassicAssist.Data
{
    public class Options : SetPropertyNotifyChanged
    {
        public delegate void dLightLevelChanged( int level );

        public const string DEFAULT_SETTINGS_FILENAME = "settings.json";
        private static string _profilePath;
        private static readonly object _serializeLock = new object();
        private bool _abilitiesGump = true;
        private int _abilitiesGumpX = 100;
        private int _abilitiesGumpY = 100;
        private bool _actionDelay;
        private int _actionDelayMs;
        private bool _alwaysOnTop;
        private bool _autoAcceptPartyInvite;
        private bool _autoAcceptPartyOnlyFromFriends;
        private bool _autologin;
        private int _autologinCharacterIndex;
        private TimeSpan _autologinConnectDelay;
        private string _autologinPassword;
        private TimeSpan _autologinReconnectDelay;
        private int _autologinServerIndex;
        private string _autologinUsername;
        private double _chatWindowHeight = 350;
        private GridLength _chatWindowRightColumn;
        private double _chatWindowWidth = 650;
        private bool _checkHandsPotions;
        private char _commandPrefix = '+';
        private bool _debug;
        private bool _defaultMacroQuietMode;
        private bool _disableHotkeysLoad;
        private bool _dragDelay;
        private int _dragDelayMs;
        private string _enemyTargetMessage;
        private EntityCollectionViewerOptions _entityCollectionViewerOptions = new EntityCollectionViewerOptions();
        private int _expireTargetsMs;
        private ObservableCollection<FriendEntry> _friends = new ObservableCollection<FriendEntry>();
        private string _friendTargetMessage;
        private bool _getFriendEnemyUsesIgnoreList;
        private string _hash;
        private bool _hotkeysStatusGump;
        private int _hotkeysStatusGumpX;
        private int _hotkeysStatusGumpY;
        private bool _includePartyMembersInFriends;
        private string _lastTargetMessage;
        private int _lightLevel;
        private bool _limitHotkeyTrigger;
        private int _limitHotkeyTriggerMs;
        private bool _limitMouseWheelTrigger;
        private int _limitMouseWheelTriggerMS;
        private bool _logoutDisconnectedPrompt;
        private bool _macrosGump;
        private int _macrosGumpHeight = 180;
        private Color _macrosGumpTextColor = Colors.White;
        private bool _macrosGumpTransparent;
        private int _macrosGumpWidth = 190;
        private int _macrosGumpX;
        private int _macrosGumpY;
        private int _maxTargetQueueLength = 1;
        private string _name;
        private bool _persistUseOnce;
        private bool _preventAttackingFriendsInWarMode;
        private bool _preventAttackingInnocentsInGuardzone;
        private bool _preventTargetingFriendsWithHarmful;
        private bool _preventTargetingInnocentsInGuardzone;
        private bool _queueLastTarget;
        private bool _rangeCheckLastTarget;
        private int _rangeCheckLastTargetAmount = 11;
        private bool _rehueFriends;
        private int _rehueFriendsHue;
        private int _selectedTabIndex;
        private bool _setUOTitle;
        private bool _showProfileNameWindowTitle;
        private bool _showResurrectionWaypoints;
        private int _slowHandlerThreshold = 250;
        private SmartTargetOption _smartTargetOption;
        private bool _sortMacrosAlphabetical;
        private bool _sysTray;
        private bool _useDeathScreenWhilstHidden;
        private bool _useExperimentalFizzleDetection;
        private bool _useObjectQueue;
        private int _useObjectQueueAmount = 5;
        private int _selectedTabIndexAgents;

        public bool AbilitiesGump
        {
            get => _abilitiesGump;
            set => SetProperty( ref _abilitiesGump, value );
        }

        public int AbilitiesGumpX
        {
            get => _abilitiesGumpX;
            set => SetProperty( ref _abilitiesGumpX, value );
        }

        public int AbilitiesGumpY
        {
            get => _abilitiesGumpY;
            set => SetProperty( ref _abilitiesGumpY, value );
        }

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

        public bool AutoAcceptPartyInvite
        {
            get => _autoAcceptPartyInvite;
            set => SetProperty( ref _autoAcceptPartyInvite, value );
        }

        public bool AutoAcceptPartyOnlyFromFriends
        {
            get => _autoAcceptPartyOnlyFromFriends;
            set => SetProperty( ref _autoAcceptPartyOnlyFromFriends, value );
        }

        public bool Autologin
        {
            get => _autologin;
            set => SetProperty( ref _autologin, value );
        }

        public int AutologinCharacterIndex
        {
            get => _autologinCharacterIndex;
            set => SetProperty( ref _autologinCharacterIndex, value );
        }

        public TimeSpan AutologinConnectDelay
        {
            get => _autologinConnectDelay;
            set => SetProperty( ref _autologinConnectDelay, value );
        }

        public string AutologinPassword
        {
            get => _autologinPassword;
            set => SetProperty( ref _autologinPassword, value );
        }

        public TimeSpan AutologinReconnectDelay
        {
            get => _autologinReconnectDelay;
            set => SetProperty( ref _autologinReconnectDelay, value );
        }

        public int AutologinServerIndex
        {
            get => _autologinServerIndex;
            set => SetProperty( ref _autologinServerIndex, value );
        }

        public string AutologinUsername
        {
            get => _autologinUsername;
            set => SetProperty( ref _autologinUsername, value );
        }

        public double ChatWindowHeight
        {
            get => _chatWindowHeight;
            set => SetProperty( ref _chatWindowHeight, value );
        }

        public GridLength ChatWindowRightColumn
        {
            get => _chatWindowRightColumn;
            set => SetProperty( ref _chatWindowRightColumn, value );
        }

        public double ChatWindowWidth
        {
            get => _chatWindowWidth;
            set => SetProperty( ref _chatWindowWidth, value );
        }

        public bool CheckHandsPotions
        {
            get => _checkHandsPotions;
            set => SetProperty( ref _checkHandsPotions, value );
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

        public bool DefaultMacroQuietMode
        {
            get => _defaultMacroQuietMode;
            set => SetProperty( ref _defaultMacroQuietMode, value );
        }

        public bool DisableHotkeysLoad
        {
            get => _disableHotkeysLoad;
            set => SetProperty( ref _disableHotkeysLoad, value );
        }
        public bool DragDelay
        {
            get => _dragDelay;
            set => SetProperty( ref _dragDelay, value );
        }

        public int DragDelayMS
        {
            get => _dragDelayMs;
            set => SetProperty( ref _dragDelayMs, value );
        }

        public string EnemyTargetMessage
        {
            get => _enemyTargetMessage;
            set => SetProperty( ref _enemyTargetMessage, value );
        }

        public int ExpireTargetsMS
        {
            get => _expireTargetsMs;
            set => SetProperty( ref _expireTargetsMs, value );
        }

        public ObservableCollection<FriendEntry> Friends
        {
            get => _friends;
            set => SetProperty( ref _friends, value );
        }

        public string FriendTargetMessage
        {
            get => _friendTargetMessage;
            set => SetProperty( ref _friendTargetMessage, value );
        }

        public bool GetFriendEnemyUsesIgnoreList
        {
            get => _getFriendEnemyUsesIgnoreList;
            set => SetProperty( ref _getFriendEnemyUsesIgnoreList, value );
        }

        public string Hash
        {
            get => _hash;
            set => SetProperty( ref _hash, value );
        }

        public bool HotkeysStatusGump
        {
            get => _hotkeysStatusGump;
            set => SetProperty( ref _hotkeysStatusGump, value );
        }

        public int HotkeysStatusGumpX
        {
            get => _hotkeysStatusGumpX;
            set => SetProperty( ref _hotkeysStatusGumpX, value );
        }

        public int HotkeysStatusGumpY
        {
            get => _hotkeysStatusGumpY;
            set => SetProperty( ref _hotkeysStatusGumpY, value );
        }

        public bool IncludePartyMembersInFriends
        {
            get => _includePartyMembersInFriends;
            set => SetProperty( ref _includePartyMembersInFriends, value );
        }

        public string LastTargetMessage
        {
            get => _lastTargetMessage;
            set => SetProperty( ref _lastTargetMessage, value );
        }

        public int LightLevel
        {
            get => _lightLevel;
            set
            {
                if ( _lightLevel != value )
                {
                    LightLevelChanged?.Invoke( value );
                }

                SetProperty( ref _lightLevel, value );
            }
        }

        public bool LimitHotkeyTrigger
        {
            get => _limitHotkeyTrigger;
            set => SetProperty( ref _limitHotkeyTrigger, value );
        }

        public int LimitHotkeyTriggerMS
        {
            get => _limitHotkeyTriggerMs;
            set => SetProperty( ref _limitHotkeyTriggerMs, value );
        }

        public bool LimitMouseWheelTrigger
        {
            get => _limitMouseWheelTrigger;
            set => SetProperty( ref _limitMouseWheelTrigger, value );
        }

        public int LimitMouseWheelTriggerMS
        {
            get => _limitMouseWheelTriggerMS;
            set => SetProperty( ref _limitMouseWheelTriggerMS, value );
        }

        public bool LogoutDisconnectedPrompt
        {
            get => _logoutDisconnectedPrompt;
            set => SetProperty( ref _logoutDisconnectedPrompt, value );
        }

        public bool MacrosGump
        {
            get => _macrosGump;
            set => SetProperty( ref _macrosGump, value );
        }

        public int MacrosGumpHeight
        {
            get => _macrosGumpHeight;
            set
            {
                SetProperty( ref _macrosGumpHeight, value );
                UO.Gumps.MacrosGump.ResendGump( true );
            }
        }

        public Color MacrosGumpTextColor
        {
            get => _macrosGumpTextColor;
            set => SetProperty( ref _macrosGumpTextColor, value );
        }

        public bool MacrosGumpTransparent
        {
            get => _macrosGumpTransparent;
            set => SetProperty( ref _macrosGumpTransparent, value );
        }

        public int MacrosGumpWidth
        {
            get => _macrosGumpWidth;
            set
            {
                SetProperty( ref _macrosGumpWidth, value );
                UO.Gumps.MacrosGump.ResendGump( true );
            }
        }

        public int MacrosGumpX
        {
            get => _macrosGumpX;
            set => SetProperty( ref _macrosGumpX, value );
        }

        public int MacrosGumpY
        {
            get => _macrosGumpY;
            set => SetProperty( ref _macrosGumpY, value );
        }

        public int MaxTargetQueueLength
        {
            get => _maxTargetQueueLength;
            set => SetProperty( ref _maxTargetQueueLength, value );
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

        public bool PreventAttackingInnocentsInGuardzone
        {
            get => _preventAttackingInnocentsInGuardzone;
            set => SetProperty( ref _preventAttackingInnocentsInGuardzone, value );
        }

        public bool PreventTargetingFriendsWithHarmful
        {
            get => _preventTargetingFriendsWithHarmful;
            set => SetProperty( ref _preventTargetingFriendsWithHarmful, value );
        }

        public bool PreventTargetingInnocentsInGuardzone
        {
            get => _preventTargetingInnocentsInGuardzone;
            set => SetProperty( ref _preventTargetingInnocentsInGuardzone, value );
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

        public bool RehueFriends
        {
            get => _rehueFriends;
            set => SetProperty( ref _rehueFriends, value );
        }

        public int RehueFriendsHue
        {
            get => _rehueFriendsHue;
            set => SetProperty( ref _rehueFriendsHue, value );
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty( ref _selectedTabIndex, value );
        }

        public int SelectedTabIndexAgents
        {
            get => _selectedTabIndexAgents;
            set => SetProperty( ref _selectedTabIndexAgents, value );
        }

        public bool SetUOTitle
        {
            get => _setUOTitle;
            set
            {
                SetProperty( ref _setUOTitle, value );
                Engine.SetTitle();
            }
        }

        public bool ShowProfileNameWindowTitle
        {
            get => _showProfileNameWindowTitle;
            set
            {
                SetProperty( ref _showProfileNameWindowTitle, value );
                Engine.UpdateWindowTitle();
            }
        }

        public bool ShowResurrectionWaypoints
        {
            get => _showResurrectionWaypoints;
            set => SetProperty( ref _showResurrectionWaypoints, value );
        }

        public int SlowHandlerThreshold
        {
            get => _slowHandlerThreshold;
            set => SetProperty( ref _slowHandlerThreshold, value );
        }

        public SmartTargetOption SmartTargetOption
        {
            get => _smartTargetOption;
            set => SetProperty( ref _smartTargetOption, value );
        }

        public bool SortMacrosAlphabetical
        {
            get => _sortMacrosAlphabetical;
            set => SetProperty( ref _sortMacrosAlphabetical, value );
        }

        public bool SysTray
        {
            get => _sysTray;
            set => SetProperty( ref _sysTray, value );
        }

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

        public static event dLightLevelChanged LightLevelChanged;

        public static void Save( Options options )
        {
            lock ( _serializeLock )
            {
                BaseViewModel[] instances = BaseViewModel.Instances;

                JObject obj = new JObject
                {
                    { "Name", options.Name }, { "SelectedTabIndex", options.SelectedTabIndex }, { "SelectedTabIndexAgents", options.SelectedTabIndexAgents }
                };

                foreach ( BaseViewModel instance in instances )
                {
                    if ( instance is ISettingProvider settingProvider )
                    {
                        settingProvider.Serialize( obj );
                    }

                    if ( !( instance is IGlobalSettingProvider globalSettingProvider ) )
                    {
                        continue;
                    }

                    JObject global = new JObject();

                    globalSettingProvider.Serialize( global, true );

                    File.WriteAllText( Path.Combine( AssistantOptions.GetGlobalPath(), globalSettingProvider.GetGlobalFilename() ), global.ToString() );
                }

                string hash = obj.ToString().SHA1();
                obj["Hash"] = hash;

                if ( hash.Equals( options.Hash ) )
                {
                    // ReSharper disable once LocalizableElement
                    Console.WriteLine( "Profile hasn't changed, skipping profile save" );
                    return;
                }

                EnsureProfilePath( Engine.StartupPath ?? Environment.CurrentDirectory );

                CheckModifiedOnDisk( options.Name, options.Hash );

                File.WriteAllText( Path.Combine( _profilePath, options.Name ?? DEFAULT_SETTINGS_FILENAME ), obj.ToString() );

                options.Hash = hash;
            }
        }

        private static void CheckModifiedOnDisk( string profileFilename, string hash )
        {
            try
            {
                string profilePath = Path.Combine( _profilePath, profileFilename );

                if ( !File.Exists( profilePath ) )
                {
                    return;
                }

                string json = File.ReadAllText( profilePath );
                JObject obj = JObject.Parse( json );

                string hashOnDisk = obj["Hash"]?.ToObject<string>() ?? string.Empty;

                if ( hashOnDisk.Equals( hash ) )
                {
                    return;
                }

                // The hash of the profile isn't the same as when we loaded / saved it last, backup the existing profile to another directory
                string conflictPath = Path.Combine( _profilePath, "Conflict" );

                if ( !Directory.Exists( conflictPath ) )
                {
                    Directory.CreateDirectory( conflictPath );
                }

                string fileName = Path.Combine( conflictPath, $"{profileFilename}-{hashOnDisk}" );

                File.Copy( profilePath, fileName, true );
            }
            catch ( Exception )
            {
                // we tried
            }
        }

        private static void EnsureProfilePath( string startupPath )
        {
            lock ( _serializeLock )
            {
                _profilePath = Path.IsPathRooted( AssistantOptions.ProfileDirectory )
                    ? AssistantOptions.ProfileDirectory
                    : Path.Combine( startupPath, AssistantOptions.ProfileDirectory );

                if ( !Directory.Exists( _profilePath ) )
                {
                    Directory.CreateDirectory( _profilePath );
                }
            }
        }

        public static void ClearOptions()
        {
            HotkeyManager.GetInstance().ClearAllHotkeys();
            AliasCommands._aliases.Clear();
            ScavengerManager.GetInstance().Items.Clear();
            MacroManager.GetInstance().Items.Clear();
        }

        public static void Load( string optionsFile, Options options )
        {
            lock ( _serializeLock )
            {
                try
                {
                    AssistantOptions.OnProfileChanging( optionsFile );
                    AssistantOptions.LastProfile = optionsFile;

                    BaseViewModel[] instances = BaseViewModel.Instances;

                    EnsureProfilePath( Engine.StartupPath ?? Environment.CurrentDirectory );

                    JObject json = new JObject();

                    string fullPath = Path.Combine( _profilePath, optionsFile );

                    if ( File.Exists( fullPath ) )
                    {
                        json = JObject.Parse( File.ReadAllText( fullPath ) );
                    }

                    options.Name = Path.GetFileName( optionsFile );
                    options.SelectedTabIndex = json["SelectedTabIndex"]?.ToObject<int>() ?? 0;
                    options.SelectedTabIndexAgents = json["SelectedTabIndexAgents"]?.ToObject<int>() ?? 0;
                    options.Hash = json["Hash"]?.ToObject<string>() ?? string.Empty;

                    foreach ( BaseViewModel instance in instances )
                    {
                        if ( instance is ISettingProvider settingProvider )
                        {
                            settingProvider.Deserialize( json, options );
                        }

                        if ( !( instance is IGlobalSettingProvider globalSettingProvider ) )
                        {
                            continue;
                        }

                        string filePath = Path.Combine( AssistantOptions.GetGlobalPath(), globalSettingProvider.GetGlobalFilename() );

                        if ( !File.Exists( filePath ) )
                        {
                            continue;
                        }

                        JObject global = JObject.Parse( File.ReadAllText( filePath ) );

                        globalSettingProvider.Deserialize( global, options, true );
                    }
                }
                catch ( Exception e )
                {
#if DEBUG
                    MessageBox.Show( "Error loading profile: \n\n" + e );
                    Environment.Exit( 0 );
#endif
                    Console.WriteLine( e );

                    SentrySdk.CaptureException( e, scope => scope.SetTag( "Profile", optionsFile ) );
                }
                finally
                {
                    AssistantOptions.OnProfileChanged( optionsFile );
                }
            }
        }

        public static string[] GetProfiles()
        {
            lock ( _serializeLock )
            {
                EnsureProfilePath( Engine.StartupPath ?? Environment.CurrentDirectory );
                return Directory.EnumerateFiles( _profilePath, "*.json" ).ToArray();
            }
        }

        public static string GetProfilePath()
        {
            lock ( _serializeLock )
            {
                EnsureProfilePath( Engine.StartupPath ?? Environment.CurrentDirectory );
                return _profilePath;
            }
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