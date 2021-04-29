using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Vendors;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class VendorBuyTabViewModel : BaseViewModel, ISettingProvider
    {
        private bool _autoDisableOnLogin;
        private ICommand _insertCommand;
        private ICommand _insertEntryCommand;
        private ObservableCollection<VendorBuyAgentEntry> _items = new ObservableCollection<VendorBuyAgentEntry>();
        private ICommand _removeCommand;
        private ICommand _removeEntryCommand;
        private VendorBuyAgentItem _selectedEntry;
        private VendorBuyAgentEntry _selectedItem;

        public VendorBuyTabViewModel()
        {
            IncomingPacketHandlers.VendorBuyDisplayEvent += OnVendorBuyDisplayEvent;
            VendorBuyManager manager = VendorBuyManager.GetInstance();
            manager.Items = Items;
        }

        public bool AutoDisableOnLogin
        {
            get => _autoDisableOnLogin;
            set => SetProperty( ref _autoDisableOnLogin, value );
        }

        public ICommand InsertCommand => _insertCommand ?? ( _insertCommand = new RelayCommand( Insert, o => true ) );

        public ICommand InsertEntryCommand =>
            _insertEntryCommand ?? ( _insertEntryCommand =
                new RelayCommandAsync( InsertEntry, o => Engine.Connected && SelectedItem != null ) );

        public ObservableCollection<VendorBuyAgentEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => SelectedItem != null ) );

        public ICommand RemoveEntryCommand =>
            _removeEntryCommand ??
            ( _removeEntryCommand = new RelayCommand( RemoveEntry, o => SelectedEntry != null ) );

        public VendorBuyAgentItem SelectedEntry
        {
            get => _selectedEntry;
            set => SetProperty( ref _selectedEntry, value );
        }

        public VendorBuyAgentEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public void Serialize( JObject json )
        {
            if ( json == null )
            {
                return;
            }

            JObject vendorBuy = new JObject();

            JArray items = new JArray();

            vendorBuy.Add( "AutoDisableOnLogin", AutoDisableOnLogin );

            foreach ( VendorBuyAgentEntry entry in Items )
            {
                JObject config = new JObject
                {
                    { "Name", entry.Name },
                    { "Enabled", entry.Enabled },
                    { "IncludeBackpackAmount", entry.IncludeBackpackAmount }
                };

                JArray itemObj = new JArray();

                foreach ( VendorBuyAgentItem item in entry.Items )
                {
                    JObject entryObj = new JObject
                    {
                        { "Enabled", item.Enabled },
                        { "Name", item.Name },
                        { "Graphic", item.Graphic },
                        { "Hue", item.Hue },
                        { "Amount", item.Amount },
                        { "MaxPrice", item.MaxPrice }
                    };

                    itemObj.Add( entryObj );
                }

                config.Add( "Items", itemObj );

                items.Add( config );
            }

            vendorBuy.Add( "Entries", items );
            json.Add( "VendorBuy", vendorBuy );
        }

        public void Deserialize( JObject json, Options options )
        {
            Items.Clear();

            JToken config = json?["VendorBuy"];

            if ( config == null )
            {
                return;
            }

            AutoDisableOnLogin = config["AutoDisableOnLogin"]?.ToObject<bool>() ?? false;

            if ( config["Items"] != null )
            {
                // Convert from Legacy "Items" to "Entries"
                VendorBuyAgentEntry entry = new VendorBuyAgentEntry
                {
                    Name = "Legacy",
                    Enabled = config["Enabled"]?.ToObject<bool>() ?? false,
                    IncludeBackpackAmount = config["IncludeBackpackAmount"]?.ToObject<bool>() ?? false
                };

                foreach ( JToken token in config["Items"] )
                {
                    VendorBuyAgentItem vbae = new VendorBuyAgentItem
                    {
                        Enabled = token["Enabled"]?.ToObject<bool>() ?? false,
                        Name = token["Name"]?.ToObject<string>() ?? "Unknown",
                        Graphic = token["Graphic"]?.ToObject<int>() ?? 0,
                        Hue = token["Hue"]?.ToObject<int>() ?? 0,
                        Amount = token["Amount"]?.ToObject<int>() ?? 0,
                        MaxPrice = token["MaxPrice"]?.ToObject<int>() ?? 0
                    };

                    entry.Items.Add( vbae );
                }

                Items.Add( entry );
            }

            if ( config["Entries"] == null )
            {
                return;
            }

            foreach ( JToken token in config["Entries"] )
            {
                VendorBuyAgentEntry entry = new VendorBuyAgentEntry
                {
                    Name = token["Name"]?.ToObject<string>() ?? "Unknown",
                    Enabled = !AutoDisableOnLogin && ( token["Enabled"]?.ToObject<bool>() ?? false ),
                    IncludeBackpackAmount = token["IncludeBackpackAmount"]?.ToObject<bool>() ?? false
                };

                if ( token["Items"] != null )
                {
                    foreach ( JToken item in token["Items"] )
                    {
                        VendorBuyAgentItem vbae = new VendorBuyAgentItem
                        {
                            Enabled = item["Enabled"]?.ToObject<bool>() ?? false,
                            Name = item["Name"]?.ToObject<string>() ?? "Unknown",
                            Graphic = item["Graphic"]?.ToObject<int>() ?? 0,
                            Hue = item["Hue"]?.ToObject<int>() ?? 0,
                            Amount = item["Amount"]?.ToObject<int>() ?? 0,
                            MaxPrice = item["MaxPrice"]?.ToObject<int>() ?? 0
                        };

                        entry.Items.Add( vbae );
                    }
                }

                Items.Add( entry );
            }
        }

        private void Insert( object obj )
        {
            Items.Add( new VendorBuyAgentEntry { Name = $"Buy-{Items.Count + 1}", Enabled = true } );
        }

        private void OnVendorBuyDisplayEvent( int serial, ShopListEntry[] entries )
        {
            List<ShopListEntry> buyList = new List<ShopListEntry>();

            foreach ( VendorBuyAgentEntry entry in Items.Where( e => e.Enabled ) )
            {
                foreach ( VendorBuyAgentItem item in entry.Items.Where( e => e.Enabled ) )
                {
                    IEnumerable<ShopListEntry> matches = entries.Where( i =>
                        i.Item.ID == item.Graphic && ( item.Hue == -1 || i.Item.Hue == item.Hue ) &&
                        ( item.MaxPrice == -1 || i.Price <= item.MaxPrice ) );

                    foreach ( ShopListEntry match in matches )
                    {
                        if ( item.Amount != -1 )
                        {
                            if ( match.Amount > item.Amount )
                            {
                                match.Amount = item.Amount;
                            }

                            if ( entry.IncludeBackpackAmount )
                            {
                                int currentAmount = ObjectCommands.CountType( item.Graphic, "backpack", item.Hue );

                                if ( currentAmount + match.Amount > item.Amount )
                                {
                                    match.Amount = item.Amount - currentAmount;
                                }
                            }
                        }

                        if ( match.Amount > 0 && buyList.All( e => e.Item.Serial != match.Item.Serial ) )
                        {
                            buyList.Add( match );
                        }
                    }
                }
            }

            if ( buyList.Count > 0 )
            {
                UOC.VendorBuy( serial, buyList.ToArray() );
            }

            if ( buyList.Count == 0 )
            {
                UOC.SystemMessage( Strings.Buy_Agent__No_matches_found_, true );
            }
        }

        private void Remove( object obj )
        {
            if ( !( obj is VendorBuyAgentEntry entry ) )
            {
                return;
            }

            Items.Remove( entry );
        }

        private static async Task InsertEntry( object arg )
        {
            if ( !( arg is VendorBuyAgentEntry entry ) )
            {
                return;
            }

            int serial = await UOC.GetTargetSerialAsync( Strings.Target_object___ );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            string name = TileData.GetStaticTile( item.ID ).Name ?? item.Name;

            entry.Items.Add( new VendorBuyAgentItem
            {
                Enabled = true,
                Name = name,
                Graphic = item.ID,
                Amount = -1,
                Hue = item.Hue,
                MaxPrice = -1
            } );
        }

        private void RemoveEntry( object obj )
        {
            if ( !( obj is VendorBuyAgentItem item ) )
            {
                return;
            }

            SelectedItem?.Items.Remove( item );
        }
    }
}