using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Misc;
using ClassicAssist.UI.Views.OptionsTab;
using ClassicAssist.UO.Gumps;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class OptionsTabViewModel : BaseViewModel, ISettingProvider
    {
        private static ICommand _macrosGumpChangedCommand;
        private ICommand _selectMacroTextColorCommand;
        private ICommand _setLanguageOverrideCommand;
        private ICommand _setUseClilocLanguageCommand;

        public ICommand MacrosGumpChangedCommand =>
            _macrosGumpChangedCommand ?? ( _macrosGumpChangedCommand = new RelayCommand( MacrosGumpChanged ) );

        public ICommand SelectMacroTextColorCommand =>
            _selectMacroTextColorCommand ?? ( _selectMacroTextColorCommand = new RelayCommand( SelectMacroTextColor ) );

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
            options.Add( "LimitHotkeyTrigger", CurrentOptions.LimitHotkeyTrigger );
            options.Add( "LimitHotkeyTriggerMS", CurrentOptions.LimitHotkeyTriggerMS );
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
            options.Add( "MacrosGumpHeight", CurrentOptions.MacrosGumpHeight );
            options.Add( "MacrosGumpWidth", CurrentOptions.MacrosGumpWidth );
            options.Add( "MacrosGumpTextColor", CurrentOptions.MacrosGumpTextColor.ToString() );
            options.Add( "MacrosGumpTransparent", CurrentOptions.MacrosGumpTransparent );
            options.Add( "ChatWindowHeight", CurrentOptions.ChatWindowHeight );
            options.Add( "ChatWindowWidth", CurrentOptions.ChatWindowWidth );
            options.Add( "ExpireTargetsMS", CurrentOptions.ExpireTargetsMS );
            options.Add( "LogoutDisconnectedPrompt", CurrentOptions.LogoutDisconnectedPrompt );
            options.Add( "DisableHotkeysLoad", CurrentOptions.DisableHotkeysLoad );
            options.Add( "HotkeysStatusGump", CurrentOptions.HotkeysStatusGump );
            options.Add( "HotkeysStatusGumpX", CurrentOptions.HotkeysStatusGumpX );
            options.Add( "HotkeysStatusGumpY", CurrentOptions.HotkeysStatusGumpY );

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
            CurrentOptions.LimitHotkeyTrigger = config?["LimitHotkeyTrigger"]?.ToObject<bool>() ?? false;
            CurrentOptions.LimitHotkeyTriggerMS = config?["LimitHotkeyTriggerMS"]?.ToObject<int>() ?? 0;
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
            CurrentOptions.MacrosGumpHeight = config?["MacrosGumpHeight"]?.ToObject<int>() ?? 190;
            CurrentOptions.MacrosGumpWidth = config?["MacrosGumpWidth"]?.ToObject<int>() ?? 180;
            CurrentOptions.MacrosGumpTextColor = config?["MacrosGumpTextColor"]?.ToObject<Color>() ?? Colors.White;
            CurrentOptions.MacrosGumpTransparent = config?["MacrosGumpTransparent"]?.ToObject<bool>() ?? true;
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

            CurrentOptions.ExpireTargetsMS = config?["ExpireTargetsMS"]?.ToObject<int>() ?? -1;
            CurrentOptions.LogoutDisconnectedPrompt = config?["LogoutDisconnectedPrompt"]?.ToObject<bool>() ?? false;
            CurrentOptions.DisableHotkeysLoad = config?["DisableHotkeysLoad"]?.ToObject<bool>() ?? false;
            CurrentOptions.HotkeysStatusGump = config?["HotkeysStatusGump"]?.ToObject<bool>() ?? false;
            CurrentOptions.HotkeysStatusGumpX = config?["HotkeysStatusGumpX"]?.ToObject<int>() ?? 10;
            CurrentOptions.HotkeysStatusGumpY = config?["HotkeysStatusGumpY"]?.ToObject<int>() ?? 30;

            HotkeyManager manager = HotkeyManager.GetInstance();
            manager.Enabled = !CurrentOptions.DisableHotkeysLoad;
        }

        private static void MacrosGumpChanged( object obj )
        {
            MacrosGump.ResendGump( true );
        }

        private void SelectMacroTextColor( object obj )
        {
            if ( !( obj is Color selectedColor ) )
            {
                return;
            }

            MacrosGumpTextColorSelectorViewModel vm =
                new MacrosGumpTextColorSelectorViewModel { SelectedColor = selectedColor };

            MacrosGumpTextColorSelectorWindow window = new MacrosGumpTextColorSelectorWindow { DataContext = vm };

            window.ShowDialog();

            if ( !vm.Result )
            {
                return;
            }

            CurrentOptions.MacrosGumpTextColor = vm.SelectedColor;
            MacrosGump.ResendGump( true );
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