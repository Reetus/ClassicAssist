using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Misc;
using ClassicAssist.UO.Gumps;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class OptionsTabViewModel : BaseViewModel, ISettingProvider
    {
        private Options _currentOptions;
        private ICommand _setLanguageOverrideCommand;
        private ICommand _setUseClilocLanguageCommand;

        public Options CurrentOptions
        {
            get => _currentOptions;
            set => SetProperty( ref _currentOptions, value );
        }

        public ICommand SetLanguageOverrideCommand =>
            _setLanguageOverrideCommand ?? ( _setLanguageOverrideCommand = new RelayCommand( SetLanguageOverride ) );

        public ICommand SetUseClilocLanguageCommand =>
            _setUseClilocLanguageCommand ??
            ( _setUseClilocLanguageCommand = new RelayCommand( SetUseClilocLanguage, o => true ) );

        public void Serialize( JObject json, bool global = false )
        {
            JObject options = new JObject();

            JObject useOnce = new JObject { ["Persist"] = CurrentOptions.PersistUseOnce };

            if ( CurrentOptions.PersistUseOnce )
            {
                JArray useOnceItems = new JArray();

                foreach ( int serial in ActionCommands.UseOnceList )
                {
                    useOnceItems.Add( serial );
                }

                useOnce.Add( "Items", useOnceItems );
            }

            options.Add( "UseOnce", useOnce );
            options.Add( "UseDeathScreenWhilstHidden", CurrentOptions.UseDeathScreenWhilstHidden );
            options.Add( "CommandPrefix", CurrentOptions.CommandPrefix );
            options.Add( "RangeCheckLastTarget", CurrentOptions.RangeCheckLastTarget );
            options.Add( "RangeCheckLastTargetAmount", CurrentOptions.RangeCheckLastTargetAmount );
            options.Add( "UseExperimentalFizzleDetection", CurrentOptions.UseExperimentalFizzleDetection );
            options.Add( "UseObjectQueue", CurrentOptions.UseObjectQueue );
            options.Add( "UseObjectQueueAmount", CurrentOptions.UseObjectQueueAmount );
            options.Add( "QueueLastTarget", CurrentOptions.QueueLastTarget );
            options.Add( "MaxTargetQueueLength", CurrentOptions.MaxTargetQueueLength );
            options.Add( "SmartTargetOption", CurrentOptions.SmartTargetOption.ToString() );
            options.Add( "LimitMouseWheelTrigger", CurrentOptions.LimitMouseWheelTrigger );
            options.Add( "LimitMouseWheelTriggerMS", CurrentOptions.LimitMouseWheelTriggerMS );
            options.Add( "AutoAcceptPartyInvite", CurrentOptions.AutoAcceptPartyInvite );
            options.Add( "AutoAcceptPartyOnlyFromFriends", CurrentOptions.AutoAcceptPartyOnlyFromFriends );
            options.Add( "PreventTargetingInnocentsInGuardzone", CurrentOptions.PreventTargetingInnocentsInGuardzone );
            options.Add( "PreventAttackingInnocentsInGuardzone", CurrentOptions.PreventAttackingInnocentsInGuardzone );
            options.Add( "LastTargetMessage", CurrentOptions.LastTargetMessage );
            options.Add( "FriendTargetMessage", CurrentOptions.FriendTargetMessage );
            options.Add( "EnemyTargetMessage", CurrentOptions.EnemyTargetMessage );
            options.Add( "DefaultMacroQuietMode", CurrentOptions.DefaultMacroQuietMode );
            options.Add( "GetFriendEnemyUsesIgnoreList", CurrentOptions.GetFriendEnemyUsesIgnoreList );
            options.Add( "AbilitiesGump", CurrentOptions.AbilitiesGump );
            options.Add( "AbilitiesGumpX", CurrentOptions.AbilitiesGumpX );
            options.Add( "AbilitiesGumpY", CurrentOptions.AbilitiesGumpY );
            options.Add( "SetUOTitle", CurrentOptions.SetUOTitle );
            options.Add( "ShowProfileNameWindowTitle", CurrentOptions.ShowProfileNameWindowTitle );
            options.Add( "SortMacrosAlphabetical", CurrentOptions.SortMacrosAlphabetical );
            options.Add( "ShowResurrectionWaypoints", CurrentOptions.ShowResurrectionWaypoints );
            options.Add( "RehueFriends", CurrentOptions.RehueFriends );
            options.Add( "RehueFriendsHue", CurrentOptions.RehueFriendsHue );
            options.Add( "CheckHandsPotions", CurrentOptions.CheckHandsPotions );
            options.Add( "MacrosGump", CurrentOptions.MacrosGump );
            options.Add( "MacrosGumpX", CurrentOptions.MacrosGumpX );
            options.Add( "MacrosGumpY", CurrentOptions.MacrosGumpY );
            options.Add( "ChatWindowWidth", CurrentOptions.ChatWindowWidth );
            options.Add( "ChatWindowHeight", CurrentOptions.ChatWindowHeight );
            options.Add( "EntityCollectionViewerOptions", CurrentOptions.EntityCollectionViewerOptions.Serialize() );
            options.Add( "ExpireTargetsMS", CurrentOptions.ExpireTargetsMS );

            json?.Add( "Options", options );
        }

        public void Deserialize( JObject json, Options options, bool global = false )
        {
            CurrentOptions = options;

            CurrentOptions.PropertyChanged += OnOptionsChanged;

            ActionCommands.UseOnceList.Clear();

            JToken config = json?["Options"];

            if ( config?["UseOnce"] != null )
            {
                CurrentOptions.PersistUseOnce = config["UseOnce"]["Persist"]?.ToObject<bool>() ?? false;

                if ( CurrentOptions.PersistUseOnce )
                {
                    foreach ( JToken token in config["UseOnce"]["Items"] )
                    {
                        ActionCommands.UseOnceList.Add( token.ToObject<int>() );
                    }
                }
            }

            CurrentOptions.UseDeathScreenWhilstHidden =
                config?["UseDeathScreenWhilstHidden"]?.ToObject<bool>() ?? false;
            CurrentOptions.CommandPrefix = config?["CommandPrefix"]?.ToObject<char>() ?? '=';
            CurrentOptions.RangeCheckLastTarget = config?["RangeCheckLastTarget"]?.ToObject<bool>() ?? false;
            CurrentOptions.RangeCheckLastTargetAmount = config?["RangeCheckLastTargetAmount"]?.ToObject<int>() ?? 11;

            CurrentOptions.UseExperimentalFizzleDetection =
                config?["UseExperimentalFizzleDetection"]?.ToObject<bool>() ?? false;

            CurrentOptions.UseObjectQueue = config?["UseObjectQueue"]?.ToObject<bool>() ?? false;
            CurrentOptions.UseObjectQueueAmount = config?["UseObjectQueueAmount"]?.ToObject<int>() ?? 5;
            CurrentOptions.QueueLastTarget = config?["QueueLastTarget"]?.ToObject<bool>() ?? false;
            CurrentOptions.MaxTargetQueueLength = config?["MaxTargetQueueLength"]?.ToObject<int>() ?? 1;
            CurrentOptions.SmartTargetOption =
                config?["SmartTargetOption"]?.ToObject<SmartTargetOption>() ?? SmartTargetOption.None;
            CurrentOptions.LimitMouseWheelTrigger = config?["LimitMouseWheelTrigger"]?.ToObject<bool>() ?? true;
            CurrentOptions.LimitMouseWheelTriggerMS = config?["LimitMouseWheelTriggerMS"]?.ToObject<int>() ?? 200;
            CurrentOptions.AutoAcceptPartyInvite = config?["AutoAcceptPartyInvite"]?.ToObject<bool>() ?? false;
            CurrentOptions.AutoAcceptPartyOnlyFromFriends =
                config?["AutoAcceptPartyOnlyFromFriends"]?.ToObject<bool>() ?? false;
            CurrentOptions.PreventTargetingInnocentsInGuardzone =
                config?["PreventTargetingInnocentsInGuardzone"]?.ToObject<bool>() ?? false;
            CurrentOptions.PreventAttackingInnocentsInGuardzone =
                config?["PreventAttackingInnocentsInGuardzone"]?.ToObject<bool>() ?? false;
            CurrentOptions.LastTargetMessage = config?["LastTargetMessage"]?.ToObject<string>() ?? "[Last Target]";
            CurrentOptions.FriendTargetMessage = config?["FriendTargetMessage"]?.ToObject<string>() ?? "[Friend]";
            CurrentOptions.EnemyTargetMessage = config?["EnemyTargetMessage"]?.ToObject<string>() ?? "[Enemy]";
            CurrentOptions.DefaultMacroQuietMode = config?["DefaultMacroQuietMode"]?.ToObject<bool>() ?? false;
            CurrentOptions.GetFriendEnemyUsesIgnoreList =
                config?["GetFriendEnemyUsesIgnoreList"]?.ToObject<bool>() ?? false;
            CurrentOptions.AbilitiesGump = config?["AbilitiesGump"]?.ToObject<bool>() ?? true;
            CurrentOptions.AbilitiesGumpX = config?["AbilitiesGumpX"]?.ToObject<int>() ?? 100;
            CurrentOptions.AbilitiesGumpY = config?["AbilitiesGumpY"]?.ToObject<int>() ?? 100;
            CurrentOptions.ShowProfileNameWindowTitle =
                config?["ShowProfileNameWindowTitle"]?.ToObject<bool>() ?? false;
            CurrentOptions.SetUOTitle = config?["SetUOTitle"]?.ToObject<bool>() ?? true;
            CurrentOptions.SortMacrosAlphabetical = config?["SortMacrosAlphabetical"]?.ToObject<bool>() ?? false;
            CurrentOptions.ShowResurrectionWaypoints = config?["ShowResurrectionWaypoints"]?.ToObject<bool>() ?? true;
            CurrentOptions.RehueFriends = config?["RehueFriends"]?.ToObject<bool>() ?? false;
            CurrentOptions.RehueFriendsHue = config?["RehueFriendsHue"]?.ToObject<int>() ?? 35;
            CurrentOptions.CheckHandsPotions = config?["CheckHandsPotions"]?.ToObject<bool>() ?? false;
            CurrentOptions.MacrosGump = config?["MacrosGump"]?.ToObject<bool>() ?? true;
            CurrentOptions.MacrosGumpX = config?["MacrosGumpX"]?.ToObject<int>() ?? 100;
            CurrentOptions.MacrosGumpY = config?["MacrosGumpY"]?.ToObject<int>() ?? 100;
            CurrentOptions.ChatWindowWidth = config?["ChatWindowWidth"]?.ToObject<double>() ?? 650;
            CurrentOptions.ChatWindowHeight = config?["ChatWindowHeight"]?.ToObject<double>() ?? 350;

            if ( CurrentOptions.AbilitiesGumpX < 0 )
            {
                CurrentOptions.AbilitiesGumpX = 100;
            }

            if ( CurrentOptions.AbilitiesGumpY < 0 )
            {
                CurrentOptions.AbilitiesGumpY = 100;
            }

            if ( CurrentOptions.MacrosGump )
            {
                MacrosGump.Initialize();
            }

            CurrentOptions.EntityCollectionViewerOptions.Deserialize( config?["EntityCollectionViewerOptions"] );
            CurrentOptions.ExpireTargetsMS = config?["ExpireTargetsMS"]?.ToObject<int>() ?? -1;
        }

        // Replay CurrentOptions changes onto Options.CurrentOptions
        // TODO: Fix Options
        private void OnOptionsChanged( object sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "Name" )
            {
                return;
            }

            object val = CurrentOptions.GetType().GetProperty( args.PropertyName )?.GetValue( CurrentOptions );

            if ( val == null )
            {
                return;
            }

            object oldVal = Options.CurrentOptions.GetType().GetProperty( args.PropertyName )
                ?.GetValue( Options.CurrentOptions );

            if ( !val.Equals( oldVal ) )
            {
                Options.CurrentOptions.GetType().GetProperty( args.PropertyName )
                    ?.SetValue( Options.CurrentOptions, val );
            }
        }

        private static void SetLanguageOverride( object obj )
        {
            if ( !( obj is Language language ) )
            {
                return;
            }

            AssistantOptions.LanguageOverride = language;

            MessageBox.Show( Strings.Restart_game_for_changes_to_take_effect___,
                Strings.Restart_game_for_changes_to_take_effect___ );
        }

        private static void SetUseClilocLanguage( object obj )
        {
            if ( !( obj is bool useClilocLanguage ) )
            {
                return;
            }

            AssistantOptions.UseCUOClilocLanguage = useClilocLanguage;

            MessageBox.Show( Strings.Restart_game_for_changes_to_take_effect___,
                Strings.Restart_game_for_changes_to_take_effect___ );
        }
    }
}