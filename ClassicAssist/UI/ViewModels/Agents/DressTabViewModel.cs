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
    public class DressTabViewModel : HotkeyEntryViewModel<DressAgentEntry>, ISettingProvider
    {
        private readonly DressManager _manager;

        private readonly Layer[] _validLayers =
        {
            Layer.Arms, Layer.Bracelet, Layer.Cloak, Layer.Earrings, Layer.Gloves, Layer.Helm, Layer.InnerLegs,
            Layer.InnerTorso, Layer.MiddleTorso, Layer.Neck, Layer.OneHanded, Layer.OuterLegs, Layer.OuterTorso,
            Layer.Pants, Layer.Ring, Layer.Shirt, Layer.Shoes, Layer.Talisman, Layer.TwoHanded, Layer.Waist
        };

        private ICommand _addDressItemCommand;
        private ICommand _changeDressType;
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
        private ICommand _undressItemsCommand;
        private bool _useUo3DPackets;

        public DressTabViewModel() : base( Strings.Dress )
        {
            _manager = DressManager.GetInstance();

            _manager.Items = Items;
        }

        public ICommand AddDressItemCommand =>
            _addDressItemCommand ?? ( _addDressItemCommand = new RelayCommandAsync( AddDressItem, o => true ) );

        public ICommand ChangeDressTypeCommand =>
            _changeDressType ??
            ( _changeDressType = new RelayCommand( ChangeDressType, o => SelectedDressItem != null ) );

        public ICommand ClearDressItemsCommand =>
            _clearDressItemsCommand ?? ( _clearDressItemsCommand = new RelayCommand( ClearDressItems,
                o => SelectedItem != null && SelectedItem.Items.Any() ) );

        public ICommand DressAllItemsCommand =>
            _dressAllItemsCommand ?? ( _dressAllItemsCommand = new RelayCommandAsync( DressAllItems,
                o => SelectedItem != null && Engine.Connected && !IsDressingOrUndressing ) );

        public ICommand ImportItemsCommand =>
            _importItemsCommand ?? ( _importItemsCommand = new RelayCommand( ImportItems, o => SelectedItem != null ) );

        public bool IsDressingOrUndressing
        {
            get => _isDressingOrUndressing;
            set => SetProperty( ref _isDressingOrUndressing, value );
        }

        public bool MoveConflictingItems
        {
            get => _moveConflictingItems;
            set => SetProperty( ref _moveConflictingItems, value );
        }

        public ICommand NewDressEntryCommand =>
            _newDressEntryCommand ?? ( _newDressEntryCommand = new RelayCommand( NewDressEntry, o => true ) );

        public ICommand RemoveDressEntryCommand =>
            _removeDressEntryCommand ?? ( _removeDressEntryCommand =
                new RelayCommand( RemoveDressEntry, o => SelectedItem != null ) );

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
            _undressAllItemsCommand ?? ( _undressAllItemsCommand =
                new RelayCommandAsync( UndressAllItems, o => Engine.Connected && !IsDressingOrUndressing ) );

        public ICommand UndressItemsCommand =>
            _undressItemsCommand ?? ( _undressItemsCommand = new RelayCommandAsync( UndressItems,
                o => SelectedItem != null && Engine.Connected && !IsDressingOrUndressing ) );

        public bool UseUO3DPackets
        {
            get => _useUo3DPackets;
            set
            {
                SetProperty( ref _useUo3DPackets, value );

                if ( _manager != null )
                {
                    _manager.UseUO3DPackets = value;
                }
            }
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
                        JObject item = new JObject
                        {
                            { "Layer", (int) dressAgentItem.Layer },
                            { "Serial", dressAgentItem.Serial },
                            { "ID", dressAgentItem.ID },
                            { "Type", (int) dressAgentItem.Type }
                        };

                        items.Add( item );
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
            Items.Clear();

            if ( json?["Dress"] == null )
            {
                return;
            }

            JToken dress = json["Dress"];

            MoveConflictingItems = GetJsonValue( dress["Options"], "MoveConflictingItems", false );
            UseUO3DPackets = _manager.UseUO3DPackets = GetJsonValue( dress["Options"], "UseUO3DPackets", false );

            foreach ( JToken entry in dress["Entries"] )
            {
                DressAgentEntry dae = new DressAgentEntry
                {
                    Name = GetJsonValue( entry, "Name", string.Empty ),
                    UndressContainer = GetJsonValue( entry, "UndressContainer", 0 ),
                    PassToUO = GetJsonValue( entry, "PassToUO", true ),
                    Hotkey = new ShortcutKeys( entry["Keys"] )
                };

                dae.Action = async hks => await DressAllItems( dae );

                List<DressAgentItem> items = new List<DressAgentItem>();

                if ( entry["Items"] != null )
                {
                    items.AddRange( entry["Items"].Select( itemEntry => new DressAgentItem
                    {
                        Layer = GetJsonValue( itemEntry, "Layer", Layer.Invalid ),
                        Serial = GetJsonValue( itemEntry, "Serial", 0 ),
                        ID = GetJsonValue( itemEntry, "ID", -1 ),
                        Type = GetJsonValue( itemEntry, "Type", DressAgentItemType.Serial )
                    } ) );
                }

                dae.Items = new List<DressAgentItem>( items );

                Items.Add( dae );
            }
        }

        private async Task UndressItems( object arg )
        {
            if ( !( arg is DressAgentEntry dae ) )
            {
                return;
            }

            try
            {
                IsDressingOrUndressing = true;

                await dae.Undress();
            }
            finally
            {
                IsDressingOrUndressing = false;
            }
        }

        private static async Task SetUndressContainer( object obj )
        {
            if ( !( obj is DressAgentEntry entry ) )
            {
                return;
            }

            int serial = await Commands.GetTargetSerialAsync( Strings.Select_undress_container___ );

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

            int serial = await Commands.GetTargetSerialAsync( Strings.Target_clothing_item___ );

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

            dae.AddOrReplaceDressItem( item );
        }

        private async Task UndressAllItems( object obj )
        {
            await _manager.UndressAll();
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

            IsDressingOrUndressing = true;
            await _manager.DressAllItems( dae, MoveConflictingItems );
            IsDressingOrUndressing = false;
        }

        private void ImportItems( object obj )
        {
            if ( !( obj is DressAgentEntry dae ) )
            {
                return;
            }

            _manager.ImportItems( dae );
        }

        public bool IsValidLayer( Layer layer )
        {
            return _validLayers.Any( l => l == layer );
        }

        private static void ChangeDressType( object obj )
        {
            if ( !( obj is DressAgentItem dai ) )
            {
                return;
            }

            dai.Type = dai.Type == DressAgentItemType.Serial ? DressAgentItemType.ID : DressAgentItemType.Serial;
        }
    }
}