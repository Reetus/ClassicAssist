using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class ActionCommands
    {
        internal static UseOnceList UseOnceList { get; set; } = new UseOnceList();

        [CommandsDisplay( Category = "Actions",
            Description = "Use a specific item type (graphic) from your backpack, only once",
            InsertText = "UseOnce(0xff)" )]
        public static bool UseOnce( int graphic, int hue = -1 )
        {
            //TODO hue?
            if ( Engine.Player?.Backpack?.Container == null )
            {
                return false;
            }

            Item backpack = Engine.Player.Backpack;

            Item match = backpack.Container.SelectEntity( i =>
                i.ID == graphic && !UseOnceList.Contains( i.Serial ) && ( hue == -1 || hue == i.Hue ) );

            if ( match == null )
            {
                UOC.SystemMessage( Strings.UseOnce__Cannot_find_type___ );
                return false;
            }

            UseOnceList.Add( match );

            Engine.SendPacketToServer( new UseObject( match.Serial ) );

            return true;
        }

        [CommandsDisplay( Category = "Actions", Description = "Clear UseOnce list.", InsertText = "ClearUseOnce()" )]
        public static void ClearUseOnce()
        {
            UseOnceList?.Clear();
            UOC.SystemMessage( Strings.UseOnce_cleared___ );
        }

        [CommandsDisplay( Category = "Actions", Description = "Attack mobile (parameter can be serial or alias).",
            InsertText = "Attack(\"last\")" )]
        public static void Attack( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Engine.SendPacketToServer( new AttackRequest( serial ) );
        }

        [CommandsDisplay( Category = "Actions", Description = "Clear hands, \"left\", \"right\", or \"both\"",
            InsertText = "ClearHands(\"both\")" )]
        public static void ClearHands( string hand )
        {
            hand = hand.ToLower();
            List<Layer> unequipLayers = new List<Layer>();

            switch ( hand )
            {
                case "left":
                    unequipLayers.Add( Layer.OneHanded );
                    break;
                case "right":
                    unequipLayers.Add( Layer.TwoHanded );
                    break;
                case "both":
                    unequipLayers.Add( Layer.OneHanded );
                    unequipLayers.Add( Layer.TwoHanded );
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( hand ) );
            }

            PlayerMobile player = Engine.Player;

            List<int> serials = unequipLayers.Select( unequipLayer => Engine.Player.GetLayer( unequipLayer ) )
                .Where( serial => serial != 0 ).ToList();

            foreach ( int serial in serials )
            {
                UOC.DragDropAsync( serial, 1, player.Backpack?.Serial ?? 0 ).Wait();
                Thread.Sleep( Options.CurrentOptions.ActionDelayMS );
            }
        }

        [CommandsDisplay( Category = "Actions",
            Description = "Single click object (parameter can be serial or alias).",
            InsertText = "ClickObject(\"last\")" )]
        public static void ClickObject( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Engine.SendPacketToServer( new LookRequest( serial ) );
        }

        [CommandsDisplay( Category = "Actions",
            Description = "Move item to container (parameters can be serials or aliases).",
            InsertText = "MoveItem(\"source\", \"destination\")" )]
        public static void MoveItem( object item, object destination, int amount = -1 )
        {
            int itemSerial = AliasCommands.ResolveSerial( item );

            if ( itemSerial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Item itemObj = Engine.Items.GetItem( itemSerial );

            if ( itemObj == null )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            if ( amount == -1 )
            {
                amount = itemObj.Count;
            }

            int containerSerial = AliasCommands.ResolveSerial( destination );

            if ( containerSerial == 0 )
            {
                //TODO
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            UOC.DragDropAsync( itemSerial, amount, containerSerial ).Wait();
        }

        [CommandsDisplay( Category = "Actions",
            Description = "Unmounts if mounted, or mounts if unmounted, will prompt for mount if no \"mount\" alias.",
            InsertText = "ToggleMounted()" )]
        public static void ToggleMounted()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            if ( player.IsMounted )
            {
                Engine.SendPacketToServer( new UseObject( player.Serial ) );
                return;
            }

            if ( !AliasCommands.FindAlias( "mount" ) )
            {
                int serial = UOC.GetTargeSerialAsync( Strings.Target_new_mount___ ).Result;

                if ( serial == -1 )
                {
                    UOC.SystemMessage( Strings.Invalid_mount___ );
                    return;
                }

                AliasCommands.SetAlias( "mount", serial );
            }

            int mountSerial = AliasCommands.GetAlias( "mount" );

            Engine.SendPacketToServer( new UseObject( mountSerial ) );
        }

        [CommandsDisplay( Category = "Actions", Description = "Feed a given alias or serial with graphic.",
            InsertText = "Feed(\"mount\", 0xff)" )]
        public static void Feed( object obj, int graphic, int amount = 1, int hue = -1 )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            if ( Engine.Player?.Backpack == null )
            {
                UOC.SystemMessage( Strings.Error__Cannot_find_player_backpack );
                return;
            }

            Item foodItem =
                Engine.Player.Backpack?.Container.SelectEntity( i => i.ID == graphic && ( hue == -1 || i.Hue == hue ) );

            if ( foodItem == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            UOC.DragDropAsync( foodItem.Serial, amount, serial ).Wait();
        }

        [CommandsDisplay( Category = "Actions", Description = "Sends rename request.",
            InsertText = "Rename(\"mount\", \"Snoopy\"" )]
        public static void Rename( object obj, string name )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            UOC.RenameRequest( serial, name );
        }

        [CommandsDisplay( Category = "Actions",
            Description = "Display corpses and/or mobiles names (parameter \"mobiles\" or \"corpses\".",
            InsertText = "ShowNames(\"corpses\")" )]
        public static void ShowNames( string showType )
        {
            const int MAX_DISTANCE = 32;
            const int corpseType = 0x2006;

            ShowNamesType enumValue = Utility.GetEnumValueByName<ShowNamesType>( showType );

            switch ( enumValue )
            {
                case ShowNamesType.Mobiles:

                    Mobile[] mobiles = Engine.Mobiles.SelectEntities( m =>
                        UOMath.Distance( m.X, m.Y, Engine.Player.X, Engine.Player.Y ) < MAX_DISTANCE );

                    if ( mobiles == null )
                    {
                        return;
                    }

                    foreach ( Mobile mobile in mobiles )
                    {
                        Engine.SendPacketToServer( new LookRequest( mobile.Serial ) );
                    }

                    break;
                case ShowNamesType.Corpses:

                    Item[] corpses = Engine.Items.SelectEntities( i =>
                        UOMath.Distance( i.X, i.Y, Engine.Player.X, Engine.Player.Y ) < MAX_DISTANCE &&
                        i.ID == corpseType );

                    if ( corpses == null )
                    {
                        return;
                    }

                    foreach ( Item corpse in corpses )
                    {
                        Engine.SendPacketToServer( new LookRequest( corpse.Serial ) );
                    }

                    break;
            }
        }

        [CommandsDisplay( Category = "Actions",
            Description = "Equip a specific item into a given layer. Use object inspector to determine layer value.",
            InsertText = "EquipItem(\"axe\", \"TwoHanded\")" )]
        public static void EquipItem( object obj, object layer )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Layer layerValue = Layer.Invalid;

            switch ( layer )
            {
                case string s:
                    layerValue = Utility.GetEnumValueByName<Layer>( s );
                    break;
                case int i:
                    layerValue = (Layer) i;
                    break;
                case Layer l:
                    layerValue = l;
                    break;
            }

            if ( layerValue == Layer.Invalid )
            {
                UOC.SystemMessage( Strings.Invalid_layer_value___ );
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            UOC.EquipItem( item, layerValue );
        }

        [CommandsDisplay( Category = "Actions",
            Description = "Retrieve an approximated ping with server. -1 on failure.", InsertText = "Ping()" )]
        public static long Ping()
        {
            Random random = new Random();

            byte value = (byte) random.Next( 1, byte.MaxValue );

            Stopwatch sw = new Stopwatch();
            sw.Start();

            PacketWaitEntry we = Engine.PacketWaitEntries.Add(
                new PacketFilterInfo( 0x73, new[] { new PacketFilterCondition( 1, new[] { value }, 1 ) } ),
                PacketDirection.Incoming, true );

            Engine.SendPacketToServer( new Ping( value ) );

            bool result = we.Lock.WaitOne( 5000 );

            sw.Stop();

            if ( result )
            {
                return sw.ElapsedMilliseconds;
            }

            return -1;
        }

        [CommandsDisplay( Category = "Actions", Description = "Request a context menu option.",
            InsertText = "ContextMenu(0x00aabbcc, 1)" )]
        public static void ContextMenu( int serial, int entry )
        {
            Engine.SendPacketToServer( new ContextMenuRequest( serial ) );
            Thread.Sleep( 400 );
            Engine.SendPacketToServer( new ContextMenuClick( serial, entry ) );
        }

        [CommandsDisplay( Category = "Actions", Description = "Request or wait for a context menu option.",
            InsertText = "WaitForContext(0x00aabbcc, 1, 5000)" )]
        public static bool WaitForContext( int serial, int entry, int timeout )
        {
            AutoResetEvent are = new AutoResetEvent( false );

            PacketFilterInfo pfi = new PacketFilterInfo( 0xBF,
                new[]
                {
                    PacketFilterConditions.ShortAtPositionCondition( 0x14, 3 ),
                    PacketFilterConditions.IntAtPositionCondition( serial, 7 )
                }, ( bytes, info ) =>
                {
                    Engine.SendPacketToServer( new ContextMenuClick( serial, entry ) );
                    are.Set();
                } );

            Engine.AddReceiveFilter( pfi );

            Engine.SendPacketToServer( new ContextMenuRequest( serial ) );

            try
            {
                bool result = are.WaitOne( timeout );

                return result;
            }
            finally
            {
                Engine.RemoveReceiveFilter( pfi );
            }
        }

        private enum ShowNamesType
        {
            Mobiles,
            Corpses
        }
    }
}