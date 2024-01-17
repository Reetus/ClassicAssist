#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.TrapPouch;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class TrapPouchTabViewModel : BaseViewModel, ISettingProvider
    {
        private const int MAGIC_TRAP_ID = 13;
        private readonly TrapPouchManager _manager = TrapPouchManager.GetInstance();

        private readonly int[] POUCH_IDS = { 0xE79 };

        private bool _autoAddOnlyMagicTrap;
        private bool _autoAddTargetedPouches;
        private bool _autoRemoveItem;
        private ICommand _clearCommand;
        private ICommand _insertCommand;
        private ObservableCollection<TrapPouchEntry> _items = new ObservableCollection<TrapPouchEntry>();
        private bool _rehueItems;
        private int _rehueItemsHue;
        private ICommand _removeCommand;
        private TrapPouchEntry _selectedItem;
        private ICommand _selectHueCommand;
        private bool _warnItemCount;
        private int _warnItemCountAmount;
        private bool _warnOverheadMessage;

        public TrapPouchTabViewModel()
        {
            OutgoingPacketHandlers.TargetSentEvent += OnTargetSent;
            Engine.PacketSentEvent += OnPacketSent;
            Engine.InternalPacketSentEvent += OnPacketSent;
            IncomingPacketHandlers.ContainerContentsEvent += OnContainerContents;
            IncomingPacketHandlers.BackupItemAddedUpdateEvent += OnBackpackItemAdded;

            _manager.Use = UseItem;
            _manager.Clear = () => { _dispatcher.Invoke( () => Clear( null ) ); };
            _manager.Add = Add;
        }

        public bool AutoAddOnlyMagicTrap
        {
            get => _autoAddOnlyMagicTrap;
            set => SetProperty( ref _autoAddOnlyMagicTrap, value );
        }

        public bool AutoAddTargetedPouches
        {
            get => _autoAddTargetedPouches;
            set => SetProperty( ref _autoAddTargetedPouches, value );
        }

        public bool AutoRemoveItem
        {
            get => _autoRemoveItem;
            set => SetProperty( ref _autoRemoveItem, value );
        }

        public ICommand ClearCommand => _clearCommand ?? ( _clearCommand = new RelayCommand( Clear ) );

        public ICommand InsertCommand => _insertCommand ?? ( _insertCommand = new RelayCommandAsync( Insert, o => true ) );

        public ObservableCollection<TrapPouchEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public bool RehueItems
        {
            get => _rehueItems;
            set => SetProperty( ref _rehueItems, value );
        }

        public int RehueItemsHue
        {
            get => _rehueItemsHue;
            set => SetProperty( ref _rehueItemsHue, value );
        }

        public ICommand RemoveCommand => _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => o != null ) );

        public TrapPouchEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ICommand SelectHueCommand => _selectHueCommand ?? ( _selectHueCommand = new RelayCommand( SelectHue ) );

        public bool WarnItemCount
        {
            get => _warnItemCount;
            set => SetProperty( ref _warnItemCount, value );
        }

        public int WarnItemCountAmount
        {
            get => _warnItemCountAmount;
            set => SetProperty( ref _warnItemCountAmount, value );
        }

        public bool WarnOverheadMessage
        {
            get => _warnOverheadMessage;
            set => SetProperty( ref _warnOverheadMessage, value );
        }

        public void Serialize( JObject json, bool global = false )
        {
            JObject config = new JObject
            {
                ["AutoAddTargetedPouches"] = AutoAddTargetedPouches,
                ["AutoAddOnlyMagicTrap"] = AutoAddOnlyMagicTrap,
                ["AutoRemoveItem"] = AutoRemoveItem,
                ["RehueItems"] = RehueItems,
                ["RehueItemsHue"] = RehueItemsHue,
                ["WarnItemCount"] = WarnItemCount,
                ["WarnItemCountAmount"] = WarnItemCountAmount,
                ["WarnOverheadMessage"] = WarnOverheadMessage
            };

            JArray items = new JArray();

            foreach ( TrapPouchEntry entry in Items )
            {
                items.Add( new JObject { { "Name", entry.Name }, { "Serial", entry.Serial }, { "OriginalHue", entry.OriginalHue } } );
            }

            config.Add( "Items", items );

            json.Add( "UseOnceAgent", config );
        }

        public void Deserialize( JObject json, Options options, bool global = false )
        {
            Items.Clear();

            JToken config = json["UseOnceAgent"];

            AutoAddTargetedPouches = config?["AutoAddTargetedPouches"]?.ToObject<bool>() ?? false;
            AutoAddOnlyMagicTrap = config?["AutoAddOnlyMagicTrap"]?.ToObject<bool>() ?? false;
            AutoRemoveItem = config?["AutoRemoveItem"]?.ToObject<bool>() ?? false;
            RehueItems = config?["RehueItems"]?.ToObject<bool>() ?? false;
            RehueItemsHue = config?["RehueItemsHue"]?.ToObject<int>() ?? 61;
            WarnItemCount = config?["WarnItemCount"]?.ToObject<bool>() ?? false;
            WarnItemCountAmount = config?["WarnItemCountAmount"]?.ToObject<int>() ?? 5;
            WarnOverheadMessage = config?["WarnOverheadMessage"]?.ToObject<bool>() ?? false;

            if ( config?["Items"] == null )
            {
                return;
            }

            foreach ( JToken token in config["Items"] )
            {
                if ( token["Serial"] == null )
                {
                    continue;
                }

                Items.Add( new TrapPouchEntry
                {
                    Name = token["Name"]?.ToObject<string>(), Serial = token["Serial"].ToObject<int>(), OriginalHue = token["OriginalHue"]?.ToObject<int>() ?? 0
                } );
            }

            _manager.Items = Items;
        }

        private void Add( int serial )
        {
            if ( Items.Any( e => e.Serial == serial ) )
            {
                Commands.SystemMessage( Strings.Item_already_in_list___ );

                return;
            }

            if ( UOMath.IsMobile( serial ) )
            {
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                return;
            }

            if ( !POUCH_IDS.Contains( item.ID ) )
            {
                Commands.SystemMessage( Strings.Invalid_type___ );

                return;
            }

            _dispatcher.Invoke( () => { Items.Add( new TrapPouchEntry { Serial = serial, Name = item.Name } ); } );

            if ( RehueItems )
            {
                Rehue( serial, RehueItemsHue );
            }
        }

        private void UseItem()
        {
            TrapPouchEntry item = Items.FirstOrDefault();

            if ( item == null )
            {
                Commands.SystemMessage( Strings.Cannot_find_item___ );

                return;
            }

            Engine.SendPacketToServer( new UseObject( item.Serial ) );
        }

        private void OnBackpackItemAdded( Item item )
        {
            if ( !RehueItems )
            {
                return;
            }

            if ( Items.Any( e => e.Serial == item.Serial ) )
            {
                Rehue( item, RehueItemsHue );
            }
        }

        private void OnContainerContents( int serial, ItemCollection container )
        {
            if ( !RehueItems )
            {
                return;
            }

            if ( container == null )
            {
                return;
            }

            Item containerItem = Engine.Items.GetItem( serial );

            if ( containerItem == null )
            {
                return;
            }

            if ( serial != Engine.Player?.Backpack?.Serial && !containerItem.IsDescendantOf( Engine.Player?.Backpack?.Serial ?? 0 ) )
            {
                return;
            }

            foreach ( Item item in container.GetItems() )
            {
                if ( Items.Any( e => e.Serial == item.Serial ) )
                {
                    Rehue( item, RehueItemsHue );
                }
            }
        }

        private void OnPacketSent( byte[] data, int length )
        {
            if ( data[0] != 0x06 )
            {
                return;
            }

            if ( !AutoRemoveItem )
            {
                return;
            }

            int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];

            TrapPouchEntry item = Items.FirstOrDefault( e => e.Serial == serial );

            if ( item == null )
            {
                return;
            }

            _dispatcher.Invoke( () => Items.Remove( item ) );

            CheckWarn();

            if ( !RehueItems )
            {
                return;
            }

            Rehue( serial, item.OriginalHue );
        }

        private void CheckWarn()
        {
            if ( !WarnItemCount )
            {
                return;
            }

            if ( Items.Count >= WarnItemCountAmount )
            {
                return;
            }

            if ( WarnOverheadMessage )
            {
                Commands.OverheadMessage( string.Format( $"{Strings.Trap_pouch_count_below__0____}", WarnItemCountAmount ), (int) SystemMessageHues.Red,
                    Engine.Player?.Serial ?? -1 );
            }
            else
            {
                Commands.SystemMessage( string.Format( $"{Strings.Trap_pouch_count_below__0____}", WarnItemCountAmount ), SystemMessageHues.Red );
            }
        }

        private void OnTargetSent( TargetType targettype, int senderserial, int flags, int serial, int x, int y, int z, int id )
        {
            if ( !AutoAddTargetedPouches )
            {
                return;
            }

            if ( !POUCH_IDS.Contains( id ) )
            {
                return;
            }

            if ( AutoAddOnlyMagicTrap )
            {
                if ( Engine.LastSpellID != MAGIC_TRAP_ID )
                {
                    return;
                }
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                return;
            }

            if ( Engine.Player?.Backpack.Serial == null )
            {
                return;
            }

            if ( item.IsDescendantOf( Engine.Player.Backpack.Serial ) && Items.All( e => e.Serial != serial ) )
            {
                _dispatcher.Invoke( () =>
                {
                    Items.Add( new TrapPouchEntry { Serial = serial, Name = item.Name } );

                    if ( RehueItems )
                    {
                        Rehue( item, RehueItemsHue );
                    }
                } );
            }
        }

        private static void Rehue( Item item, int hue )
        {
            if ( item.Owner != 0 && item.IsDescendantOf( Engine.Player?.Backpack?.Serial ?? 0 ) )
            {
                Engine.SendPacketToClient( new ContainerContentUpdate( item.Serial, item.ID, item.Direction, item.Count, item.X, item.Y, item.Grid, item.Owner, hue ) );
            }
        }

        private static void Rehue( int serial, int hue )
        {
            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                return;
            }

            Rehue( item, hue );
        }

        private void SelectHue( object obj )
        {
            if ( !HuePickerWindow.GetHue( out int hue ) )
            {
                return;
            }

            RehueItemsHue = hue;
        }

        private void Clear( object obj )
        {
            foreach ( TrapPouchEntry item in Items )
            {
                Rehue( item.Serial, item.OriginalHue );
            }

            Items.Clear();
        }

        private void Remove( object obj )
        {
            if ( !( obj is TrapPouchEntry entry ) )
            {
                return;
            }

            Items.Remove( entry );
        }

        private async Task Insert( object arg )
        {
            ( TargetType _, TargetFlags _, int serial, int _, int _, int _, int id ) = await Commands.GetTargetInfoAsync();

            if ( serial <= 0 )
            {
                return;
            }

            if ( !POUCH_IDS.Contains( id ) )
            {
                Commands.SystemMessage( Strings.Invalid_type___ );

                return;
            }

            if ( UOMath.IsMobile( serial ) )
            {
                return;
            }

            if ( Items.Any( e => e.Serial == serial ) )
            {
                Commands.SystemMessage( Strings.Item_already_in_list___ );

                return;
            }

            Item item = Engine.Items.GetItem( serial );

            Items.Add( new TrapPouchEntry { Serial = serial, Name = item != null ? item.Name : "Unknown" } );

            if ( RehueItems )
            {
                Rehue( serial, RehueItemsHue );
            }
        }
    }
}