using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Organizer;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UO;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace ClassicAssist.Shared.UI.ViewModels.Agents
{
    public class OrganizerTabViewModel : HotkeyEntryViewModel<OrganizerEntry>, ISettingProvider
    {
        private readonly OrganizerManager _manager;
        private ReactiveCommand<OrganizerEntry, Unit> _insertItemCommand;
        private bool _isOrganizing;
        private ICommand _newOrganizerEntryCommand;
        private ReactiveCommand<OrganizerEntry, Unit> _organizeCommand;
        private ReactiveCommand<OrganizerItem, Unit> _removeItemCommand;
        private ReactiveCommand<OrganizerEntry, Unit> _removeOrganizerAgentEntryCommand;
        private OrganizerEntry _selectedItem;
        private OrganizerItem _selectedOrganizerItem;
        private ReactiveCommand<OrganizerEntry, Unit> _setContainersCommand;

        public OrganizerTabViewModel() : base( Strings.Organizer )
        {
            _manager = OrganizerManager.GetInstance();

            _manager.Items = Items;
        }

        public ReactiveCommand<OrganizerEntry, Unit> InsertItemCommand =>
            _insertItemCommand ?? ( _insertItemCommand = ReactiveCommand.CreateFromTask<OrganizerEntry>( InsertItem,
                this.WhenAnyValue( e => e.SelectedItem, e => e.IsOrganizing, ( e, f ) => e != null && !f ) ) );

        public bool IsOrganizing
        {
            get => _isOrganizing;
            set
            {
                SetProperty( ref _isOrganizing, value );
                NotifyPropertyChanged( nameof( PlayStopButtonText ) );
            }
        }

        public ICommand NewOrganizerEntryCommand =>
            _newOrganizerEntryCommand ??
            ( _newOrganizerEntryCommand = new RelayCommand( NewOrganizerEntry, o => !IsOrganizing ) );

        public ReactiveCommand<OrganizerEntry, Unit> OrganizeCommand =>
            _organizeCommand ?? ( _organizeCommand = ReactiveCommand.CreateFromTask<OrganizerEntry>( Organize,
                this.WhenAnyValue( e => e.SelectedItem, selector: e => e != null ) ) );

        public string PlayStopButtonText => IsOrganizing ? Strings.Stop : Strings.Play;

        public ReactiveCommand<OrganizerItem, Unit> RemoveItemCommand =>
            _removeItemCommand ?? ( _removeItemCommand = ReactiveCommand.Create<OrganizerItem>( RemoveItem,
                this.WhenAnyValue( e => e.IsOrganizing, e => e.SelectedOrganizerItem, ( e, f ) => !e && f != null ) ) );

        public ReactiveCommand<OrganizerEntry, Unit> RemoveOrganizerAgentEntryCommand =>
            _removeOrganizerAgentEntryCommand ?? ( _removeOrganizerAgentEntryCommand =
                ReactiveCommand.Create<OrganizerEntry>( RemoveOrganizerAgentEntry,
                    this.WhenAnyValue( e => e.SelectedItem, selector: e => e != null ) ) );

        public OrganizerEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public OrganizerItem SelectedOrganizerItem
        {
            get => _selectedOrganizerItem;
            set => SetProperty( ref _selectedOrganizerItem, value );
        }

        public ReactiveCommand<OrganizerEntry, Unit> SetContainersCommand =>
            _setContainersCommand ?? ( _setContainersCommand =
                ReactiveCommand.CreateFromTask<OrganizerEntry>( _manager.SetContainers,
                    this.WhenAnyValue( e => e.SelectedItem, f => f.IsOrganizing, ( e, f ) => e != null && !f ) ) );
        //new RelayCommandAsync( _manager.SetContainers, o => SelectedItem != null && !IsOrganizing ) );

        public void Serialize( JObject json )
        {
            JArray organizer = new JArray();

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

            json?.Add( "Organizer", organizer );
        }

        public void Deserialize( JObject json, Options options )
        {
            Items.Clear();

            if ( json?["Organizer"] == null )
            {
                return;
            }

            foreach ( JToken token in json["Organizer"] )
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

                entry.Action = hks => Task.Run( async () => await _manager.Organize( entry ) );
                entry.IsRunning = () => IsOrganizing;

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

                Items.Add( entry );
            }
        }

        private async Task Organize( object arg )
        {
            if ( !( arg is OrganizerEntry entry ) )
            {
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
                Action = hks => Task.Run( async () => await _manager.Organize( SelectedItem ) ),
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

            int serial = await Commands.GetTargeSerialAsync( Strings.Target_new_item___ );

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