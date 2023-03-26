using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Vendors;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;
using static ClassicAssist.UO.Commands;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class VendorSellTabViewModel : BaseViewModel, ISettingProvider
    {
        private int _containerSerial;
        private ICommand _insertCommand;
        private ICommand _insertMatchAnyCommand;
        private ObservableCollection<VendorSellAgentEntry> _items = new ObservableCollection<VendorSellAgentEntry>();
        private ICommand _removeCommand;
        private ICommand _resetContainerCommand;
        private VendorSellAgentEntry _selectedItem;
        private ICommand _setContainerCommand;

        public VendorSellTabViewModel()
        {
            IncomingPacketHandlers.VendorSellDisplayEvent += OnVendorSellDisplayEvent;
        }

        public int ContainerSerial
        {
            get => _containerSerial;
            set => SetProperty( ref _containerSerial, value );
        }

        public ICommand InsertCommand =>
            _insertCommand ?? ( _insertCommand = new RelayCommandAsync( Insert, o => Engine.Connected ) );

        public ICommand InsertMatchAnyCommand =>
            _insertMatchAnyCommand ?? ( _insertMatchAnyCommand = new RelayCommand( InsertMatchAny, o => true ) );

        public ObservableCollection<VendorSellAgentEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommandAsync( Remove, o => SelectedItem != null ) );

        public ICommand ResetContainerCommand =>
            _resetContainerCommand ??
            ( _resetContainerCommand = new RelayCommand( ResetContainer, o => ContainerSerial != 0 ) );

        public VendorSellAgentEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ICommand SetContainerCommand =>
            _setContainerCommand ?? ( _setContainerCommand =
                new RelayCommandAsync( SetContainer, o => Engine.Connected ) );

        public void Serialize( JObject json, bool global = false )
        {
            if ( json == null )
            {
                return;
            }

            JArray itemsObj = new JArray();

            foreach ( VendorSellAgentEntry item in Items )
            {
                JObject itemObj = new JObject
                {
                    { "Enabled", item.Enabled },
                    { "Graphic", item.Graphic },
                    { "Hue", item.Hue },
                    { "Amount", item.Amount },
                    { "MinPrice", item.MinPrice },
                    { "Name", item.Name }
                };

                itemsObj.Add( itemObj );
            }

            JObject config = new JObject { { "Items", itemsObj }, { "ContainerSerial", ContainerSerial } };

            json.Add( "VendorSell", config );
        }

        public void Deserialize( JObject json, Options options, bool global = false )
        {
            Items.Clear();

            JToken config = json?["VendorSell"];

            if ( config?["Items"] == null )
            {
                return;
            }

            foreach ( JToken items in config["Items"] )
            {
                VendorSellAgentEntry vsae = new VendorSellAgentEntry
                {
                    Enabled = items["Enabled"]?.ToObject<bool>() ?? true,
                    Graphic = items["Graphic"]?.ToObject<int>() ?? 0,
                    Hue = items["Hue"]?.ToObject<int>() ?? 0,
                    Amount = items["Amount"]?.ToObject<int>() ?? -1,
                    MinPrice = items["MinPrice"]?.ToObject<int>() ?? 0,
                    Name = items["Name"]?.ToObject<string>() ?? string.Empty
                };

                Items.Add( vsae );
            }

            ContainerSerial = config["ContainerSerial"]?.ToObject<int>() ?? 0;
        }

        private void InsertMatchAny( object arg )
        {
            Items.Add( new VendorSellAgentEntry
            {
                Enabled = true,
                Name = Strings.Any,
                Graphic = -1,
                Hue = -1,
                Amount = -1,
                MinPrice = 0
            } );
        }

        private void ResetContainer( object obj )
        {
            ContainerSerial = 0;
        }

        private async Task Remove( object arg )
        {
            if ( !( arg is VendorSellAgentEntry entry ) )
            {
                return;
            }

            Items.Remove( entry );

            await Task.CompletedTask;
        }

        private void OnVendorSellDisplayEvent( int serial, SellListEntry[] entries )
        {
            List<SellListEntry> sellList = new List<SellListEntry>();

            foreach ( SellListEntry entry in entries )
            {
                VendorSellAgentEntry match = Items.FirstOrDefault( i =>
                    ( i.Graphic == -1 || i.Graphic == entry.ID ) && ( i.Hue == -1 || i.Hue == entry.Hue ) &&
                    entry.Price >= i.MinPrice && i.Enabled );

                if ( match != null && match.Amount != -1 )
                {
                    entry.Amount = Math.Min( match.Amount, entry.Amount );
                }

                if ( match != null )
                {
                    sellList.Add( entry );
                }
            }

            if ( ContainerSerial != 0 )
            {
                if ( !Engine.Player.Backpack.Container.GetItem( ContainerSerial, out Item container ) )
                {
                    SystemMessage( Strings.Invalid_container___ );

                    return;
                }

                int[] containerIds =
                    container.Container?.GetItems().Select( i => i.ID ).ToArray() ?? Array.Empty<int>();

                WaitForContainerContentsUse( ContainerSerial, 1000 );

                List<SellListEntry> filteredList = sellList.Where( e => containerIds.Contains( e.ID ) ).ToList();

                foreach ( SellListEntry entry in filteredList )
                {
                    int totalAmount = container.Container?.Where( e => e.ID == entry.ID ).Sum( e => e.Count ) ?? 0;

                    entry.Amount = Math.Min( entry.Amount, totalAmount );
                }

                sellList = filteredList;
            }

            if ( sellList.Count > 0 )
            {
                Sell( serial, sellList.ToArray() );
            }
        }

        private async Task SetContainer( object arg )
        {
            int serial = await GetTargetSerialAsync( Strings.Target_container___ );

            if ( serial == 0 )
            {
                SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return;
            }

            ContainerSerial = serial;
        }

        public static void Sell( int vendorSerial, SellListEntry[] entries )
        {
            if ( entries == null || entries.Length == 0 )
            {
                return;
            }

            int len = 9 + entries.Length * 6;

            PacketWriter pw = new PacketWriter( len );
            pw.Write( (byte) 0x9F );
            pw.Write( (short) len );
            pw.Write( vendorSerial );
            pw.Write( (short) entries.Length );

            foreach ( SellListEntry entry in entries )
            {
                pw.Write( entry.Serial );
                pw.Write( (short) entry.Amount );
            }

            Engine.SendPacketToServer( pw );
        }

        private async Task Insert( object arg )
        {
            int serial = await GetTargetSerialAsync( Strings.Target_object___ );

            if ( serial == 0 )
            {
                SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            string name = TileData.GetStaticTile( item.ID ).Name ?? item.Name;

            Items.Add( new VendorSellAgentEntry
            {
                Enabled = true,
                Name = name,
                Graphic = item.ID,
                Hue = item.Hue,
                Amount = -1,
                MinPrice = 0
            } );
        }
    }
}