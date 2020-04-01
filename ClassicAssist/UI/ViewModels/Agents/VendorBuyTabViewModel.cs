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
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class VendorBuyTabViewModel : BaseViewModel, ISettingProvider
    {
        private bool _enabled;
        private bool _includeBackpackAmount;
        private bool _includePurchasedAmount;
        private ICommand _insertCommand;
        private ObservableCollection<VendorBuyAgentEntry> _items = new ObservableCollection<VendorBuyAgentEntry>();
        private ICommand _removeCommand;
        private VendorBuyAgentEntry _selectedItem;

        public VendorBuyTabViewModel()
        {
            IncomingPacketHandlers.VendorBuyDisplayEvent += OnVendorBuyDisplayEvent;
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public bool IncludeBackpackAmount
        {
            get => _includeBackpackAmount;
            set => SetProperty( ref _includeBackpackAmount, value );
        }

        public bool IncludePurchasedAmount
        {
            get => _includePurchasedAmount;
            set => SetProperty( ref _includePurchasedAmount, value );
        }

        public ICommand InsertCommand =>
            _insertCommand ?? ( _insertCommand = new RelayCommandAsync( Insert, o => Engine.Connected ) );

        public ObservableCollection<VendorBuyAgentEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => SelectedItem != null ) );

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

            JObject config = new JObject
            {
                { "Enabled", Enabled },
                { "IncludeBackpackAmount", IncludeBackpackAmount },
                { "IncludePurchasedAmount", IncludePurchasedAmount }
            };

            JArray itemObj = new JArray();

            foreach ( VendorBuyAgentEntry entry in Items )
            {
                JObject entryObj = new JObject
                {
                    { "Enabled", entry.Enabled },
                    { "Name", entry.Name },
                    { "Graphic", entry.Graphic },
                    { "Hue", entry.Hue },
                    { "Amount", entry.Amount },
                    { "MaxPrice", entry.MaxPrice }
                };

                itemObj.Add( entryObj );
            }

            config.Add( "Items", itemObj );

            json.Add( "VendorBuy", config );
        }

        public void Deserialize( JObject json, Options options )
        {
            Items.Clear();

            if ( json?["VendorBuy"] == null )
            {
                return;
            }

            JToken config = json["VendorBuy"];

            Enabled = config["Enabled"]?.ToObject<bool>() ?? false;
            IncludeBackpackAmount = config["IncludeBackpackAmount"]?.ToObject<bool>() ?? false;
            IncludePurchasedAmount = config["IncludePurchasedAmount"]?.ToObject<bool>() ?? false;

            foreach ( JToken token in config["Items"] )
            {
                VendorBuyAgentEntry vbae = new VendorBuyAgentEntry
                {
                    Enabled = token["Enabled"]?.ToObject<bool>() ?? false,
                    Name = token["Name"]?.ToObject<string>() ?? "Unknown",
                    Graphic = token["Graphic"]?.ToObject<int>() ?? 0,
                    Hue = token["Hue"]?.ToObject<int>() ?? 0,
                    Amount = token["Amount"]?.ToObject<int>() ?? 0,
                    MaxPrice = token["MaxPrice"]?.ToObject<int>() ?? 0
                };

                Items.Add( vbae );
            }
        }

        private void OnVendorBuyDisplayEvent( int serial, ShopListEntry[] entries )
        {
            if ( !Enabled )
            {
                return;
            }

            IEnumerable<VendorBuyAgentEntry> items = Items.Where( i => i.Enabled );

            List<ShopListEntry> buyList = new List<ShopListEntry>();

            foreach ( VendorBuyAgentEntry entry in items )
            {
                IEnumerable<ShopListEntry> matches = entries.Where( i =>
                    i.Item.ID == entry.Graphic && ( entry.Hue == -1 || i.Item.Hue == entry.Hue ) &&
                    ( entry.MaxPrice == -1 || i.Price <= entry.MaxPrice ) );

                foreach ( ShopListEntry match in matches )
                {
                    if ( entry.Amount != -1 )
                    {
                        if ( match.Amount > entry.Amount )
                        {
                            match.Amount = entry.Amount;
                        }

                        if ( IncludeBackpackAmount )
                        {
                            int currentAmount = ObjectCommands.CountType( entry.Graphic, "backpack", entry.Hue );

                            if ( currentAmount + match.Amount > entry.Amount )
                            {
                                match.Amount = entry.Amount - currentAmount;
                            }
                        }
                    }

                    if ( match.Amount > 0 )
                    {
                        buyList.Add( match );
                    }
                }

                UOC.VendorBuy( serial, buyList.ToArray() );
            }

            if ( !MacroManager.QuietMode && buyList.Count == 0 )
            {
                UOC.SystemMessage( Strings.Buy_Agent__No_matches_found_ );
            }
        }

        private async Task Insert( object arg )
        {
            int serial = await UOC.GetTargeSerialAsync( Strings.Target_object___ );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            string name = TileData.GetStaticTile( item.ID ).Name ?? item.Name;

            Items.Add( new VendorBuyAgentEntry
            {
                Enabled = true,
                Name = name,
                Graphic = item.ID,
                Amount = -1,
                Hue = item.Hue,
                MaxPrice = -1
            } );
        }

        private void Remove( object obj )
        {
            if ( !( obj is VendorBuyAgentEntry entry ) )
            {
                return;
            }

            Items.Remove( entry );
        }
    }
}