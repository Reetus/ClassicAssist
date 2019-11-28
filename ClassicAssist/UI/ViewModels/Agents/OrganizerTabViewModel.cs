using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Organizer;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class OrganizerTabViewModel : HotkeySettableViewModel<OrganizerEntry>, ISettingProvider
    {
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
            OrganizerManager manager = OrganizerManager.GetInstance();

            manager.Items = Items;
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
                NotifyPropertyChanged( nameof( PlayStopButtonText ) );
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
                new RelayCommandAsync( SetContainers, o => SelectedItem != null && !IsOrganizing ) );

        private CancellationTokenSource _cancellationTokenSource { get; set; } = new CancellationTokenSource();

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

                JArray itemsArray = new JArray();

                foreach ( OrganizerItem organizerItem in organizerEntry.Items )
                {
                    JObject itemsObj = new JObject();

                    SetJsonValue( itemsObj, "Item", organizerItem.Item );
                    SetJsonValue( itemsObj, "ID", organizerItem.ID );
                    SetJsonValue( itemsObj, "Amount", organizerItem.Amount );

                    itemsArray.Add( itemsObj );
                }

                entryObj.Add( "Items", itemsArray );

                organizer.Add( entryObj );
            }

            json.Add( "Organizer", organizer );
        }

        public void Deserialize( JObject json, Options options )
        {
            if ( json["Organizer"] == null )
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
                    DestinationContainer = GetJsonValue( token, "DestinationContainer", 0 )
                };

                entry.Action = hks => Task.Run( async () => await Organize( entry ) );
                entry.IsRunning = () => IsOrganizing;

                foreach ( JToken itemToken in token["Items"] )
                {
                    OrganizerItem item = new OrganizerItem
                    {
                        Item = GetJsonValue( itemToken, "Item", string.Empty ),
                        ID = GetJsonValue( itemToken, "ID", 0 ),
                        Amount = GetJsonValue( itemToken, "Amount", -1 )
                    };

                    entry.Items.Add( item );
                }

                Items.Add( entry );
            }
        }

        private void NewOrganizerEntry( object obj )
        {
            int count = Items.Count + 1;

            OrganizerEntry entry = new OrganizerEntry
            {
                Name = $"Organizer-{count}",
                Action = hks => Task.Run( async () => await Organize( SelectedItem ) ),
                IsRunning = () => IsOrganizing
            };

            Items.Add( entry );
            SelectedItem = entry;
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

            int serial = await Commands.GetTargeSerialAsync( Strings.Target_new_item___, 30000 );

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
                Item = TileData.GetStaticTile( item.ID ).Name, ID = item.ID, Amount = -1
            };

            entry.Items.Add( organizerItem );
        }

        private async Task Organize( object obj )
        {
            if ( !( obj is OrganizerEntry entry ) )
            {
                return;
            }

            if ( IsOrganizing )
            {
                _cancellationTokenSource.Cancel();
                return;
            }

            if ( entry.SourceContainer == 0 || entry.DestinationContainer == 0 )
            {
                await SetContainers( entry );
                return;
            }

            Item sourceContainer = Engine.Items.GetItem( entry.SourceContainer );

            if ( sourceContainer == null )
            {
                //TODO
                Commands.SystemMessage( Strings.Cannot_find_container___ );
                return;
            }

            PacketFilterInfo pfi = new PacketFilterInfo( 0x3C,
                new[] { PacketFilterConditions.IntAtPositionCondition( sourceContainer.Serial, 19 ) } );

            if ( Commands.WaitForIncomingPacket( pfi, 1000,
                () => Engine.SendPacketToServer( new UseObject( sourceContainer.Serial ) ) ) )
            {
                await Task.Delay(Options.CurrentOptions.ActionDelayMS);
            }

            if ( sourceContainer.Container == null )
            {
                //TODO
                Commands.SystemMessage( Strings.Cannot_find_container___ );
                return;
            }

            Item desinationContainer = Engine.Items.GetItem( entry.DestinationContainer );

            if ( desinationContainer == null )
            {
                //TODO
                Commands.SystemMessage( Strings.Cannot_find_container___ );
                return;
            }

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                IsOrganizing = true;

                Commands.SystemMessage( string.Format( Strings.Organizer__0__running___, entry.Name ) );

                int[] matchItems = entry.Items.Select( mi => mi.ID ).ToArray();

                Item[] moveItems = sourceContainer.Container.SelectEntities( i => matchItems.Contains( i.ID ) );

                if ( moveItems == null )
                {
                    return;
                }

                foreach ( Item moveItem in moveItems )
                {
                    if ( entry.Stack )
                    {
                        await Commands.DragDropAsync( moveItem.Serial, moveItem.Count, desinationContainer.Serial );
                    }
                    else
                    {
                        await Commands.DragDropAsync( moveItem.Serial, moveItem.Count, desinationContainer.Serial, 0,
                            0 );
                    }

                    if ( _cancellationTokenSource.IsCancellationRequested )
                    {
                        break;
                    }
                }
            }
            finally
            {
                Commands.SystemMessage( string.Format( Strings.Organizer__0__finished___, entry.Name ) );
                IsOrganizing = false;
            }
        }

        private static async Task SetContainers( object obj )
        {
            if ( !( obj is OrganizerEntry entry ) )
            {
                return;
            }

            int sourceContainer = await Commands.GetTargeSerialAsync( Strings.Select_source_container___, 30000 );

            if ( sourceContainer <= 0 )
            {
                Commands.SystemMessage( Strings.Invalid_source_container___ );
                return;
            }

            int desintationContainer =
                await Commands.GetTargeSerialAsync( Strings.Select_destination_container___, 30000 );

            if ( desintationContainer <= 0 )
            {
                Commands.SystemMessage( Strings.Invalid_destination_container___ );
                return;
            }

            entry.SourceContainer = sourceContainer;
            entry.DestinationContainer = desintationContainer;

            Commands.SystemMessage( Strings.Organizer_containers_set___ );
        }
    }
}