using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Data.Organizer;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class OrganizerTabViewModel : HotkeyEntryViewModel<OrganizerEntry>, ISettingProvider
    {
        private readonly OrganizerManager _manager;
        private ICommand _insertItemCommand;
        private bool _isOrganizing;
        private ICommand _newOrganizerEntryCommand;
        private ICommand _organizeCommand;
        private ICommand _removeItemCommand;
        private ICommand _removeOrganizerAgentEntryCommand;
        private OrganizerEntry _selectedItem;
        private ICommand _setContainersCommand;

        public OrganizerTabViewModel() : base( Strings.Organizer )
        {
            _manager = OrganizerManager.GetInstance();

            _manager.Items = Items;
            _manager.InvokeByName = InvokeByName;

            HotkeyCommand stopHotkey = new HotkeyCommand
            {
                Name = Strings.Stop_Organizer, Action = ( entry, objects ) => _manager.Stop(), CanGlobal = false
            };

            _staticOptions.Add( stopHotkey );
        }

        public ICommand InsertItemCommand =>
            _insertItemCommand ?? ( _insertItemCommand =
                new RelayCommandAsync( InsertItem, o => SelectedItem != null && !IsOrganizing ) );

        public bool IsOrganizing
        {
            get => _isOrganizing;
            set
            {
                SetProperty( ref _isOrganizing, value );
                OnPropertyChanged( nameof( PlayStopButtonText ) );
            }
        }

        public ICommand NewOrganizerEntryCommand =>
            _newOrganizerEntryCommand ??
            ( _newOrganizerEntryCommand = new RelayCommand( NewOrganizerEntry, o => !IsOrganizing ) );

        public ICommand OrganizeCommand =>
            _organizeCommand ?? ( _organizeCommand = new RelayCommandAsync( Organize, o => SelectedItem != null ) );

        public string PlayStopButtonText => IsOrganizing ? Strings.Stop : Strings.Play;

        public ICommand RemoveItemCommand =>
            _removeItemCommand ?? ( _removeItemCommand = new RelayCommand( RemoveItem, o => !IsOrganizing ) );

        public ICommand RemoveOrganizerAgentEntryCommand =>
            _removeOrganizerAgentEntryCommand ?? ( _removeOrganizerAgentEntryCommand =
                new RelayCommand( RemoveOrganizerAgentEntry, o => !IsOrganizing ) );

        public OrganizerEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ICommand SetContainersCommand =>
            _setContainersCommand ?? ( _setContainersCommand =
                new RelayCommandAsync( _manager.SetContainers, o => SelectedItem != null && !IsOrganizing ) );

        public void Serialize( JObject json, bool global = false )
        {
            if ( json == null )
            {
                return;
            }

            JArray organizer = new JArray();

            JObject obj = new JObject();
            SerializeStatic( obj );
            json.Add( "OrganizerOptions", obj );

            foreach ( OrganizerEntry organizerEntry in Items )
            {
                JObject entryObj = new JObject();

                SetJsonValue( entryObj, "Name", organizerEntry.Name );
                SetJsonValue( entryObj, "Stack", organizerEntry.Stack );
                SetJsonValue( entryObj, "SourceContainer", organizerEntry.SourceContainer );
                SetJsonValue( entryObj, "DestinationContainer", organizerEntry.DestinationContainer );
                SetJsonValue( entryObj, "Keys", organizerEntry.Hotkey.ToJObject() );
                SetJsonValue( entryObj, "Complete", organizerEntry.Complete );

                JArray itemsArray = new JArray();

                foreach ( OrganizerItem organizerItem in organizerEntry.Items )
                {
                    JObject itemsObj = new JObject();

                    SetJsonValue( itemsObj, "Item", organizerItem.Item );
                    SetJsonValue( itemsObj, "ID", organizerItem.ID );
                    SetJsonValue( itemsObj, "Hue", organizerItem.Hue );
                    SetJsonValue( itemsObj, "Amount", organizerItem.Amount );

                    itemsArray.Add( itemsObj );
                }

                entryObj.Add( "Items", itemsArray );

                organizer.Add( entryObj );
            }

            json.Add( "Organizer", organizer );
        }

        public void Deserialize( JObject json, Options options, bool global = false )
        {
            Items = new ObservableCollectionEx<OrganizerEntry>();

            if ( json?["Organizer"] == null )
            {
                return;
            }

            JToken entriesArray = json["Organizer"];

            if ( json["OrganizerOptions"] is JObject obj )
            {
                DeserializeStatic( obj );
            }

            foreach ( JToken token in entriesArray )
            {
                OrganizerEntry entry = new OrganizerEntry
                {
                    Name = GetJsonValue( token, "Name", "Organizer" ),
                    Stack = GetJsonValue( token, "Stack", true ),
                    SourceContainer = GetJsonValue( token, "SourceContainer", 0 ),
                    DestinationContainer = GetJsonValue( token, "DestinationContainer", 0 ),
                    Hotkey = new ShortcutKeys( token["Keys"] ),
                    Complete = GetJsonValue( token, "Complete", false )
                };

                entry.Action = ( hks, _ ) => Task.Run( async () => await _manager.Organize( entry ) );
                entry.IsRunning = () => IsOrganizing;

                if ( token["Items"] is JArray )
                {
                    foreach ( JToken itemToken in token["Items"] )
                    {
                        OrganizerItem item = new OrganizerItem
                        {
                            Item = GetJsonValue( itemToken, "Item", string.Empty ),
                            ID = GetJsonValue( itemToken, "ID", 0 ),
                            Hue = GetJsonValue( itemToken, "Hue", -1 ),
                            Amount = GetJsonValue( itemToken, "Amount", -1 )
                        };

                        entry.Items.Add( item );
                    }
                }

                Items.Add( entry );
            }

            _manager.Items = Items;
        }

        private void InvokeByName( string name )
        {
            OrganizerEntry organizer = Items.FirstOrDefault( e => e.Name == name );

            if ( organizer != null )
            {
                Task.Run( () => organizer.Action( organizer, null ) );
            }
            else
            {
                Commands.SystemMessage( Strings.Invalid_organizer_agent_name___ );
            }
        }

        private async Task Organize( object arg )
        {
            if ( !( arg is OrganizerEntry entry ) )
            {
                return;
            }

            if ( IsOrganizing )
            {
                _manager.Stop();

                return;
            }

            IsOrganizing = true;

            await _manager.Organize( entry );

            IsOrganizing = false;
        }

        private void NewOrganizerEntry( object obj )
        {
            int count = Items.Count + 1;

            OrganizerEntry entry = new OrganizerEntry
            {
                Name = $"Organizer-{count}",
                Action = ( hks, _ ) => Task.Run( async () => await _manager.Organize( SelectedItem ) ),
                IsRunning = () => IsOrganizing
            };

            Items.Add( entry );
        }

        private void RemoveOrganizerAgentEntry( object obj )
        {
            if ( !( obj is OrganizerEntry entry ) )
            {
                return;
            }

            Items.Remove( entry );
        }

        private void RemoveItem( object obj )
        {
            if ( !( obj is OrganizerItem item ) )
            {
                return;
            }

            SelectedItem.Items.Remove( item );
        }

        private static async Task InsertItem( object arg )
        {
            if ( !( arg is OrganizerEntry entry ) )
            {
                return;
            }

            int serial = await Commands.GetTargetSerialAsync( Strings.Target_new_item___ );

            if ( serial <= 0 )
            {
                Commands.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                Commands.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            OrganizerItem organizerItem = new OrganizerItem
            {
                Item = TileData.GetStaticTile( item.ID ).Name, ID = item.ID, Hue = item.Hue, Amount = -1
            };

            entry.Items.Add( organizerItem );
        }
    }
}