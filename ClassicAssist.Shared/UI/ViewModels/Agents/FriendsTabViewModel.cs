using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Data.Friends;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.UI.ViewModels;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace ClassicAssist.Shared.UI.ViewModels.Agents
{
    public class FriendsTabViewModel : BaseViewModel, ISettingProvider
    {
        private ICommand _addFriendCommand;
        private ICommand _changeRehueOption;
        private Options _options;
        private ICommand _removeFriendCommand;
        private FriendEntry _selectedItem;
        private ICommand _selectHueCommand;

        public ICommand AddFriendCommand =>
            _addFriendCommand ?? ( _addFriendCommand = new RelayCommandAsync( AddFriend, o => true ) );

        public ICommand ChangeRehueOption =>
            _changeRehueOption ?? ( _changeRehueOption = new RelayCommand( ChangeRehue, o => true ) );

        public Options Options
        {
            get => _options;
            set => SetProperty( ref _options, value );
        }

        public ICommand RemoveFriendCommand =>
            _removeFriendCommand ?? ( _removeFriendCommand = ReactiveCommand.CreateFromTask<FriendEntry>( RemoveFriend,
                this.WhenAnyValue( e => e.SelectedItem, selector: e => e != null ) ) );
        //new RelayCommandAsync( RemoveFriend, o => SelectedItem != null ) );

        public FriendEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ICommand SelectHueCommand =>
            _selectHueCommand ?? ( _selectHueCommand = ReactiveCommand.CreateFromTask( SelectHue,
                this.WhenAnyValue( e => e.Options.RehueFriends, selector: e => e ) ) );

        public void Serialize( JObject json )
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

        public void Deserialize( JObject json, Options options )
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

        private static async Task SelectHue( CancellationToken token )
        {
            int hue = await Engine.UIInvoker.GetHueAsync();

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
    }
}