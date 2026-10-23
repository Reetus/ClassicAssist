using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Friends;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Sentry;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class FriendsTabViewModel : BaseViewModel, ISettingProvider
    {
        private ICommand _addFriendCommand;
        private ICommand _changeRehueOption;
        private ICommand _deselectAllLayersCommand;
        private ICommand _importCommand;
        private ICommand _openRehueLayersCommand;
        private Options _options;
        private ObservableCollection<RehueLayerEntry> _rehueLayers = new ObservableCollection<RehueLayerEntry>();
        private bool _rehueLayersOpen;
        private ICommand _removeFriendCommand;
        private ICommand _selectAllLayersCommand;
        private FriendEntry _selectedItem;
        private ICommand _selectHueCommand;

        public ICommand AddFriendCommand =>
            _addFriendCommand ?? ( _addFriendCommand = new RelayCommandAsync( AddFriend, o => true ) );

        public ICommand ChangeRehueOption =>
            _changeRehueOption ?? ( _changeRehueOption = new RelayCommand( ChangeRehue, o => true ) );

        public ICommand DeselectAllLayersCommand =>
            _deselectAllLayersCommand ??
            ( _deselectAllLayersCommand = new RelayCommand( o => SetAllLayers( false ), o => true ) );

        public ICommand ImportCommand => _importCommand ?? ( _importCommand = new RelayCommand( Import ) );

        public ICommand OpenRehueLayersCommand =>
            _openRehueLayersCommand ?? ( _openRehueLayersCommand = new RelayCommand( o => RehueLayersOpen = true,
                o => Options.CurrentOptions.RehueFriends ) );

        public Options Options
        {
            get => _options;
            set => SetProperty( ref _options, value );
        }

        public ObservableCollection<RehueLayerEntry> RehueLayers
        {
            get => _rehueLayers;
            set => SetProperty( ref _rehueLayers, value );
        }

        public bool RehueLayersOpen
        {
            get => _rehueLayersOpen;
            set
            {
                // Rebuild on open so the list always reflects the current profile
                if ( value )
                {
                    RefreshRehueLayers();
                }

                SetProperty( ref _rehueLayersOpen, value );
            }
        }

        public ICommand RemoveFriendCommand =>
            _removeFriendCommand ??
            ( _removeFriendCommand = new RelayCommandAsync( RemoveFriend, o => SelectedItem != null ) );

        public FriendEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ICommand SelectAllLayersCommand =>
            _selectAllLayersCommand ??
            ( _selectAllLayersCommand = new RelayCommand( o => SetAllLayers( true ), o => true ) );

        public ICommand SelectHueCommand =>
            _selectHueCommand ??
            ( _selectHueCommand = new RelayCommand( SelectHue, o => Options.CurrentOptions.RehueFriends ) );

        public void Serialize( JObject json, bool global = false )
        {
            JObject config = new JObject
            {
                ["IncludePartyMembersInFriends"] = Options.IncludePartyMembersInFriends,
                ["PreventAttackingFriendsInWarMode"] = Options.PreventAttackingFriendsInWarMode,
                ["PreventTargetingFriendsWithHarmful"] = Options.PreventTargetingFriendsWithHarmful
            };

            JArray friends = new JArray();

            foreach ( FriendEntry friend in Options.Friends )
            {
                friends.Add( new JObject { { "Name", friend.Name }, { "Serial", friend.Serial } } );
            }

            config.Add( "Items", friends );

            json?.Add( "Friends", config );
        }

        public void Deserialize( JObject json, Options options, bool global = false )
        {
            Options = options;
            Options.Friends.Clear();

            if ( json?["Friends"] == null )
            {
                return;
            }

            JToken config = json["Friends"];

            Options.IncludePartyMembersInFriends = config["IncludePartyMembersInFriends"]?.ToObject<bool>() ?? true;
            Options.PreventAttackingFriendsInWarMode =
                config["PreventAttackingFriendsInWarMode"]?.ToObject<bool>() ?? true;
            Options.PreventTargetingFriendsWithHarmful =
                config["PreventTargetingFriendsWithHarmful"]?.ToObject<bool>() ?? false;

            if ( config["Items"] == null )
            {
                return;
            }

            foreach ( JToken token in config["Items"] )
            {
                Options.Friends.Add( new FriendEntry
                {
                    Name = token["Name"].ToObject<string>(), Serial = token["Serial"].ToObject<int>()
                } );
            }
        }

        private static void ChangeRehue( object obj )
        {
            MainCommands.Resync();
        }

        private void RefreshRehueLayers()
        {
            List<RehueLayerEntry> entries = new List<RehueLayerEntry>
            {
                new RehueLayerEntry( Strings.Body, null, Options.CurrentOptions.RehueFriendsBody, OnLayerChanged )
            };

            IEnumerable<Layer> layers = Enum.GetValues( typeof( Layer ) ).Cast<Layer>().Where( l =>
                l > Layer.Invalid && l <= Layer.Mount && l != Layer.Unused_xF );

            entries.AddRange( layers.Select( l => new RehueLayerEntry( l.ToString(), l,
                !Options.CurrentOptions.RehueFriendsExcludedLayers.Contains( l ), OnLayerChanged ) ) );

            RehueLayers = new ObservableCollection<RehueLayerEntry>( entries );

            void OnLayerChanged( RehueLayerEntry entry )
            {
                ApplyRehueLayers();
            }
        }

        private void SetAllLayers( bool enabled )
        {
            foreach ( RehueLayerEntry entry in RehueLayers )
            {
                entry.SetEnabledSilent( enabled );
            }

            ApplyRehueLayers();
        }

        private void ApplyRehueLayers()
        {
            // Replace rather than mutate, the set is read from the packet thread
            Options.CurrentOptions.RehueFriendsExcludedLayers = new HashSet<Layer>( RehueLayers
                .Where( e => e.Layer.HasValue && !e.Enabled ).Select( e => e.Layer.Value ) );
            Options.CurrentOptions.RehueFriendsBody = RehueLayers.Any( e => !e.Layer.HasValue && e.Enabled );

            if ( Options.CurrentOptions.RehueFriends )
            {
                MainCommands.Resync();
            }
        }

        private static void SelectHue( object obj )
        {
            if ( !HuePickerWindow.GetHue( out int hue ) )
            {
                return;
            }

            Options.CurrentOptions.RehueFriendsHue = hue;
            MainCommands.Resync();
        }

        private static async Task AddFriend( object arg )
        {
            await Task.Run( () => MobileCommands.AddFriend() );
        }

        private static Task RemoveFriend( object arg )
        {
            if ( !( arg is FriendEntry fe ) )
            {
                return Task.CompletedTask;
            }

            MobileCommands.RemoveFriend( fe.Serial );

            return Task.CompletedTask;
        }

        private void Import( object obj )
        {
            string profileDirectory = AssistantOptions.GetProfilePath();

            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = profileDirectory,
                Filter = "Json Files|*.json",
                CheckFileExists = true,
                Multiselect = false
            };

            bool? result = ofd.ShowDialog();

            if ( !result.HasValue || result == false )
            {
                return;
            }

            try
            {
                string json = File.ReadAllText( ofd.FileName );

                JObject jObject = JObject.Parse( json );

                JToken config = jObject?["Friends"];

                if ( config?["Items"] == null )
                {
                    return;
                }

                foreach ( JToken token in config["Items"] )
                {
                    string name = token["Name"]?.ToObject<string>() ?? string.Empty;
                    int serial = token["Serial"]?.ToObject<int>() ?? -1;

                    FriendEntry existing = Options.Friends.FirstOrDefault( e => e.Serial == serial );

                    if ( existing != null )
                    {
                        Options.Friends.Remove( existing );
                    }

                    if ( serial != -1 )
                    {
                        Options.Friends.Add( new FriendEntry { Name = name, Serial = serial } );
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message );
                SentrySdk.CaptureException( ex );
            }
        }
    }
}