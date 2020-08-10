using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.Data.Regions;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class ActionCommands
    {
        internal static UseOnceList UseOnceList { get; set; } = new UseOnceList();

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Timeout ) } )]
        public static bool WaitForContents( object obj, int timeout = 5000 )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            return UOC.WaitForContainerContentsUse( serial, timeout );
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int Contents( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return 0;
            }

            Item container = Engine.Items.GetItem( serial );

            if ( container?.Container != null )
            {
                return container.Container.GetTotalItemCount();
            }

            UOC.SystemMessage( Strings.Invalid_container___ );
            return 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ) )]
        public static void EquipLastWeapon()
        {
            Engine.SendPacketToServer( new EquipLastWeapon() );
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.ItemID ), nameof( ParameterType.Hue ) } )]
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

            ObjectCommands.UseObject( match.Serial );

            return true;
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ) )]
        public static void ClearUseOnce()
        {
            UseOnceList?.Clear();
            UOC.SystemMessage( Strings.UseOnce_cleared___ );
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void Attack( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            if ( Options.CurrentOptions.PreventAttackingInnocentsInGuardzone )
            {
                Mobile mobile = Engine.Mobiles.GetMobile( serial );

                if ( mobile != null && mobile.Notoriety == Notoriety.Innocent &&
                     mobile.GetRegion().Attributes.HasFlag( RegionAttributes.Guarded ) )
                {
                    UOC.SystemMessage( Strings.Attack_request_blocked___ );

                    return;
                }
            }

            Engine.SendPacketToClient( new ChangeCombatant( serial ) );
            Engine.SendPacketToServer( new AttackRequest( serial ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.LeftRightBoth ) } )]
        public static void ClearHands( string hand = "both" )
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
                ActionPacketQueue.EnqueueDragDrop( serial, 1, player.Backpack?.Serial ?? 0 );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Actions ) )]
        public static void ToggleMounted()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            if ( player.IsMounted )
            {
                ObjectCommands.UseObject( player.Serial );
                return;
            }

            if ( !AliasCommands.FindAlias( "mount" ) )
            {
                int serial = UOC.GetTargetSerialAsync( Strings.Target_new_mount___ ).Result;

                if ( serial == -1 )
                {
                    UOC.SystemMessage( Strings.Invalid_mount___ );
                    return;
                }

                AliasCommands.SetAlias( "mount", serial );
            }

            int mountSerial = AliasCommands.GetAlias( "mount" );

            ObjectCommands.UseObject( mountSerial );
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.ItemID ),
                nameof( ParameterType.Amount ), nameof( ParameterType.Hue )
            } )]
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

            ActionPacketQueue.EnqueueDragDrop( foodItem.Serial, amount, serial );
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Name ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.ShowType ) } )]
        [CommandsDisplayStringSeeAlso( new[] { nameof( ShowNamesType ) } )]
        public static void ShowNames( string showType )
        {
            const int MAX_DISTANCE = 32;
            const int corpseType = 0x2006;

            ShowNamesType enumValue = Utility.GetEnumValueByName<ShowNamesType>( showType );

            switch ( enumValue )
            {
                case ShowNamesType.Mobiles:

                    Mobile[] mobiles = Engine.Mobiles.SelectEntities( m => m.Distance < MAX_DISTANCE );

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

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.ItemID ), nameof( ParameterType.Layer ) } )]
        [CommandsDisplayStringSeeAlso( new[] { null, nameof( Layer ) } )]
        public static void EquipType( int id, object layer )
        {
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

            UOC.EquipType( id, layerValue );
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Layer ) } )]
        [CommandsDisplayStringSeeAlso( new[] { null, nameof( Layer ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.Layer ), nameof( ParameterType.SerialOrAlias ) } )]
        [CommandsDisplayStringSeeAlso( new[] { nameof( Layer ), null } )]
        public static bool FindLayer( object layer, object obj = null )
        {
            if ( obj == null )
            {
                obj = "self";
            }

            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
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
                return false;
            }

            if ( !UOMath.IsMobile( serial ) )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return false;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            int layerSerial = mobile?.GetLayer( layerValue ) ?? 0;

            if ( layerSerial == 0 )
            {
                AliasCommands.UnsetAlias( "found" );
                return false;
            }

            AliasCommands.SetMacroAlias( "found", layerSerial );

            if ( MacroManager.QuietMode )
            {
                return true;
            }

            UOC.SystemMessage( string.Format( Strings.Object___0___updated___, "found" ) );

            return true;
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ) )]
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

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.ContextMenuIndex ) } )]
        public static void ContextMenu( object obj, int entry )
        {
            if ( obj == null )
            {
                obj = "self";
            }

            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Engine.SendPacketToServer( new ContextMenuRequest( serial ) );
            Thread.Sleep( 400 );
            Engine.SendPacketToServer( new ContextMenuClick( serial, entry ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.ContextMenuIndex ),
                nameof( ParameterType.Timeout )
            } )]
        public static bool WaitForContext( object obj, int entry, int timeout )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

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