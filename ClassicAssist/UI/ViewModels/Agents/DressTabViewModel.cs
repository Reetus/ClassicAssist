using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Dress;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class DressTabViewModel : HotkeySettableViewModel<DressAgentEntry>, ISettingProvider
    {
        private readonly Layer[] _validLayers =
        {
            Layer.Arms, Layer.Bracelet, Layer.Cloak, Layer.Earrings, Layer.Gloves, Layer.Helm, Layer.InnerLegs,
            Layer.InnerTorso, Layer.MiddleTorso, Layer.Neck, Layer.OneHanded, Layer.OuterLegs, Layer.OuterTorso,
            Layer.Pants, Layer.Ring, Layer.Shirt, Layer.Shoes, Layer.Talisman, Layer.TwoHanded, Layer.Waist
        };

        private ICommand _addDressItemCommand;
        private ICommand _clearDressItemsCommand;
        private ICommand _dressAllItemsCommand;
        private ICommand _importItemsCommand;
        private bool _isDressingOrUndressing;
        private bool _moveConflictingItems;
        private ICommand _newDressEntryCommand;
        private RelayCommand _removeDressEntryCommand;
        private ICommand _removeDressItemCommand;
        private DressAgentItem _selectedDressItem;
        private DressAgentEntry _selectedItem;
        private ICommand _setUndressContainerCommand;
        private ICommand _undressAllItemsCommand;
        private bool _useUo3DPackets;

        public DressTabViewModel() : base( Strings.Dress )
        {
            DressManager manager = DressManager.GetInstance();

            manager.Items = Items;
        }

        public ICommand AddDressItemCommand =>
            _addDressItemCommand ?? ( _addDressItemCommand = new RelayCommandAsync( AddDressItem, o => true ) );

        public ICommand ClearDressItemsCommand =>
            _clearDressItemsCommand ?? ( _clearDressItemsCommand = new RelayCommand( ClearDressItems,
                o => SelectedItem != null && SelectedItem.Items.Any() ) );

        public ICommand DressAllItemsCommand =>
            _dressAllItemsCommand ??
            ( _dressAllItemsCommand =
                new RelayCommandAsync( DressAllItems, o => Engine.Connected && !_isDressingOrUndressing ) );

        public ICommand ImportItemsCommand =>
            _importItemsCommand ?? ( _importItemsCommand = new RelayCommand( ImportItems, o => SelectedItem != null ) );

        public bool MoveConflictingItems
        {
            get => _moveConflictingItems;
            set => SetProperty( ref _moveConflictingItems, value );
        }

        public ICommand NewDressEntryCommand =>
            _newDressEntryCommand ?? ( _newDressEntryCommand = new RelayCommand( NewDressEntry, o => true ) );

        public ICommand RemoveDressEntryCommand =>
            _removeDressEntryCommand ??
            ( _removeDressEntryCommand = new RelayCommand( RemoveDressEntry, o => SelectedItem != null ) );

        public ICommand RemoveDressItemCommand =>
            _removeDressItemCommand ?? ( _removeDressItemCommand =
                new RelayCommand( RemoveDressItem, o => _selectedDressItem != null ) );

        public DressAgentItem SelectedDressItem
        {
            get => _selectedDressItem;
            set => SetProperty( ref _selectedDressItem, value );
        }

        public DressAgentEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ICommand SetUndressContainerCommand =>
            _setUndressContainerCommand ?? ( _setUndressContainerCommand =
                new RelayCommandAsync( SetUndressContainer, o => _selectedItem != null ) );

        public ICommand UndressAllItemsCommand =>
            _undressAllItemsCommand ??
            ( _undressAllItemsCommand = new RelayCommandAsync( UndressAllItems, o => Engine.Connected ) );

        public bool UseUO3DPackets
        {
            get => _useUo3DPackets;
            set => SetProperty( ref _useUo3DPackets, value );
        }

        public void Serialize( JObject json )
        {
            JObject dress = new JObject
            {
                {
                    "Options",
                    new JObject { ["MoveConflictingItems"] = MoveConflictingItems, ["UseUO3DPackets"] = UseUO3DPackets }
                }
            };

            JArray dressEntries = new JArray();

            foreach ( DressAgentEntry dae in Items )
            {
                JObject djson = new JObject();

                SetJsonValue( djson, "Name", dae.Name );
                SetJsonValue( djson, "UndressContainer", dae.UndressContainer );
                SetJsonValue( djson, "PassToUO", dae.PassToUO );
                SetJsonValue( djson, "Keys", dae.Hotkey.ToJObject() );

                JArray items = new JArray();

                if ( dae.Items != null )
                {
                    foreach ( DressAgentItem dressAgentItem in dae.Items )
                    {
                        items.Add( new JObject
                        {
                            ["Layer"] = (int) dressAgentItem.Layer, ["Serial"] = dressAgentItem.Serial
                        } );
                    }
                }

                djson.Add( "Items", items );
                dressEntries.Add( djson );
            }

            dress.Add( "Entries", dressEntries );
            json?.Add( "Dress", dress );
        }

        public void Deserialize( JObject json, Options options )
        {
            if ( json?["Dress"] == null )
            {
                return;
            }

            JToken dress = json["Dress"];

            MoveConflictingItems = GetJsonValue( dress["Options"], "MoveConflictingItems", false );
            UseUO3DPackets = GetJsonValue( dress["Options"], "UseUO3DPackets", false );

            Items.Clear();

            foreach ( JToken entry in dress["Entries"] )
            {
                DressAgentEntry dae = new DressAgentEntry
                {
                    Name = GetJsonValue( entry, "Name", string.Empty ),
                    UndressContainer = GetJsonValue( entry, "UndressContainer", 0 ),
                    PassToUO = GetJsonValue( entry, "PassToUO", true ),
                    Hotkey = new ShortcutKeys( GetJsonValue( entry["Keys"], "Modifier", Key.None ),
                        GetJsonValue( entry["Keys"], "Keys", Key.None ) )
                };

                dae.Action = async hks => await DressAllItems( dae );

                List<DressAgentItem> items = new List<DressAgentItem>();

                if ( entry["Items"] != null )
                {
                    items.AddRange( entry["Items"].Select( itemEntry => new DressAgentItem
                    {
                        Layer = GetJsonValue( itemEntry, "Layer", Layer.Invalid ),
                        Serial = GetJsonValue( itemEntry, "Serial", 0 )
                    } ) );
                }

                dae.Items = items.ToArray();

                Items.Add( dae );
            }
        }

        private static async Task SetUndressContainer( object obj )
        {
            if ( !( obj is DressAgentEntry entry ) )
            {
                return;
            }

            int serial = await Commands.GetTargeSerialAsync( Strings.Select_undress_container___ );

            if ( serial <= 0 )
            {
                Commands.SystemMessage( Strings.Invalid_container___ );
                return;
            }

            entry.UndressContainer = serial;
        }

        private static void ClearDressItems( object obj )
        {
            if ( !( obj is DressAgentEntry dae ) )
            {
                return;
            }

            dae.Items = new List<DressAgentItem>();
        }

        private void RemoveDressItem( object obj )
        {
            if ( !( obj is DressAgentItem removeItem ) )
            {
                return;
            }

            if ( !SelectedItem.Items.Contains( removeItem ) )
            {
                return;
            }

            List<DressAgentItem> list = SelectedItem.Items.ToList();
            list.Remove( removeItem );
            SelectedItem.Items = list;
        }

        private static async Task AddDressItem( object arg )
        {
            if ( !( arg is DressAgentEntry dae ) )
            {
                return;
            }

            int serial = await Commands.GetTargeSerialAsync( Strings.Target_clothing_item___ );

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                Commands.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            if ( item.Layer == Layer.Invalid )
            {
                Commands.SystemMessage( Strings.The_item_needs_to_be_equipped___ );
                return;
            }

            dae.AddOrReplaceDressItem( serial, item.Layer );
        }

        private async Task UndressAllItems( object obj )
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            int backpack = player.Backpack.Serial;

            if ( backpack <= 0 )
            {
                return;
            }

            int[] items = player.GetEquippedItems().Where( i => IsValidLayer( i.Layer ) ).Select( i => i.Serial )
                .ToArray();

            foreach ( int item in items )
            {
                await Commands.DragDropAsync( item, 1, backpack );
            }
        }

        private void NewDressEntry( object obj )
        {
            int count = Items.Count;

            DressAgentEntry dae =
                new DressAgentEntry { Name = $"Dress-{count + 1}", Items = new List<DressAgentItem>() };

            dae.Action = async hks => await DressAllItems( dae );

            Items.Add( dae );
        }

        private void RemoveDressEntry( object obj )
        {
            if ( !( obj is DressAgentEntry dae ) )
            {
                return;
            }

            dae.Hotkey = ShortcutKeys.Default;
            Items.Remove( dae );
        }

        private async Task DressAllItems( object obj )
        {
            if ( !( obj is DressAgentEntry dae ) )
            {
                return;
            }

            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            await Task.Run( async () =>
            {
                try
                {
                    _isDressingOrUndressing = true;

                    foreach ( DressAgentItem dai in dae.Items )
                    {
                        Item item = Engine.Items.GetItem( dai.Serial );

                        if ( item == null )
                        {
                            continue;
                        }

                        Commands.EquipItem( item, dai.Layer );

                        await Task.Delay( Options.CurrentOptions.ActionDelayMS );
                    }
                }
                finally
                {
                    _isDressingOrUndressing = false;
                }
            } );
        }

        private void ImportItems( object obj )
        {
            if ( !( obj is DressAgentEntry dae ) )
            {
                return;
            }

            PlayerMobile player = Engine.Player;

            List<DressAgentItem> items = player.GetEquippedItems().Where( i => IsValidLayer( i.Layer ) )
                .Select( i => new DressAgentItem { Serial = i.Serial, Layer = i.Layer } ).ToList();

            dae.Items = items;
        }

        public bool IsValidLayer( Layer layer )
        {
            return _validLayers.Any( l => l == layer );
        }
    }
}