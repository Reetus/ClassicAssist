using System;
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
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Sentry;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class FriendsTabViewModel : BaseViewModel, ISettingProvider
    {
        private ICommand _addFriendCommand;
        private ICommand _changeRehueOption;
        private ICommand _importCommand;
        private Options _options;
        private ICommand _removeFriendCommand;
        private FriendEntry _selectedItem;
        private ICommand _selectHueCommand;

        public ICommand AddFriendCommand =>
            _addFriendCommand ?? ( _addFriendCommand = new RelayCommandAsync( AddFriend, o => true ) );

        public ICommand ChangeRehueOption =>
            _changeRehueOption ?? ( _changeRehueOption = new RelayCommand( ChangeRehue, o => true ) );

        public ICommand ImportCommand => _importCommand ?? ( _importCommand = new RelayCommand( Import ) );

        public Options Options
        {
            get => _options;
            set => SetProperty( ref _options, value );
        }

        public ICommand RemoveFriendCommand =>
            _removeFriendCommand ??
            ( _removeFriendCommand = new RelayCommandAsync( RemoveFriend, o => SelectedItem != null ) );

        public FriendEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

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
            string profileDirectory = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory );

            if ( Directory.Exists( Path.Combine( profileDirectory, "Profiles" ) ) )
            {
                profileDirectory = Path.Combine( profileDirectory, "Profiles" );
            }

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