using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.Data.Regions;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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

            ObjectCommands.UseObject( match.Serial, true );

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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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
                ObjectCommands.UseObject( player.Serial, true );
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

            ObjectCommands.UseObject( mountSerial, true );
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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

            if ( Engine.Player.GetLayer( layerValue ) == serial )
            {
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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

            UOC.SystemMessage( string.Format( Strings.Object___0___updated___, "found" ), true );

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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.String ),
                nameof( ParameterType.Timeout )
            } )]
        public static bool WaitForContext( object obj, string entryName, int timeout )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return false;
            }

            bool result = false;

            AutoResetEvent are = new AutoResetEvent( false );

            PacketFilterInfo pfi = new PacketFilterInfo( 0xBF,
                new[]
                {
                    PacketFilterConditions.ShortAtPositionCondition( 0x14, 3 ),
                    PacketFilterConditions.IntAtPositionCondition( serial, 7 )
                }, ( bytes, info ) =>
                {
                    IEnumerable<ContextMenuEntry> entries = ParseContextMenuEntries( bytes );

                    ContextMenuEntry entry =
                        entries.FirstOrDefault( e => e.Text.Trim().ToLower().Equals( entryName.Trim().ToLower() ) );

                    if ( entry == null )
                    {
                        UOC.SystemMessage( Strings.Context_menu_entry_not_found___, (int) SystemMessageHues.Yellow,
                            true, true );
                        are.Set();
                        return;
                    }

                    if ( entry.Flags.HasFlag( ContextMenuFlags.Disabled ) )
                    {
                        UOC.SystemMessage( Strings.Context_menu_entry_disabled, (int) SystemMessageHues.Yellow,
                            true, true );
                        are.Set();
                        return;
                    }

                    Engine.SendPacketToServer( new ContextMenuClick( serial, entry.Index ) );
                    result = true;
                    are.Set();
                } );

            Engine.AddReceiveFilter( pfi );

            Engine.SendPacketToServer( new ContextMenuRequest( serial ) );

            try
            {
                are.WaitOne( timeout );

                return result;
            }
            finally
            {
                Engine.RemoveReceiveFilter( pfi );
            }
        }

        private static IEnumerable<ContextMenuEntry> ParseContextMenuEntries( byte[] packet )
        {
            PacketReader reader = new PacketReader( packet, packet.Length, false );

            reader.ReadInt16();
            int type = reader.ReadInt16();
            int serial = reader.ReadInt32();
            int len = reader.ReadByte();

            int entry, cliloc, flags, hue;

            List<ContextMenuEntry> entries = new List<ContextMenuEntry>();

            switch ( type )
            {
                case 1: // Old Type
                    for ( int x = 0; x < len; x++ )
                    {
                        entry = reader.ReadInt16();
                        cliloc = reader.ReadInt16() + 3000000;
                        flags = reader.ReadInt16();
                        hue = 0;

                        if ( ( flags & 0x20 ) == 0x20 )
                        {
                            hue = reader.ReadInt16();
                        }

                        string text = Cliloc.GetProperty( cliloc );
                        entries.Add( new ContextMenuEntry
                        {
                            Index = entry,
                            Cliloc = cliloc,
                            Flags = (ContextMenuFlags) flags,
                            Hue = hue,
                            Text = text
                        } );
                    }

                    break;
                case 2: // KR -> SA3D -> 2D post 7.0.0.0
                    for ( int x = 0; x < len; x++ )
                    {
                        cliloc = reader.ReadInt32();
                        entry = reader.ReadInt16();
                        flags = reader.ReadInt16();
                        hue = 0;

                        if ( ( flags & 0x20 ) == 0x20 )
                        {
                            hue = reader.ReadInt16();
                        }

                        string text = Cliloc.GetProperty( cliloc );

                        entries.Add( new ContextMenuEntry
                        {
                            Index = entry,
                            Cliloc = cliloc,
                            Flags = (ContextMenuFlags) flags,
                            Hue = hue,
                            Text = text
                        } );
                    }

                    break;
            }

            return entries;
        }

        private enum ShowNamesType
        {
            Mobiles,
            Corpses
        }

        internal class ContextMenuEntry
        {
            public int Cliloc { get; set; }
            public ContextMenuFlags Flags { get; set; }
            public int Hue { get; set; }
            public int Index { get; set; }
            public string Text { get; set; }
        }

        [Flags]
        internal enum ContextMenuFlags
        {
            Enabled = 0x00,
            Disabled = 0x01,
            Highlighted = 0x04,
            Coloured = 0x20
        }
    }
}