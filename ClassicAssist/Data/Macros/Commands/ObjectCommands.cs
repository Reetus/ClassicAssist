using System;
using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Data.Abilities;
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
    public static class ObjectCommands
    {
        internal static List<int> IgnoreList { get; set; } = new List<int>();

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void IgnoreObject( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return;
            }

            if ( !IgnoreList.Contains( serial ) )
            {
                IgnoreList.Add( serial );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static void ClearIgnoreList()
        {
            IgnoreList.Clear();
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void UseObject( object obj, bool skipQueue = false )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return;
            }

            ActionPacketQueue.EnqueueActionPacket( new UseObject( serial ),
                skipQueue ? QueuePriority.Immediate : QueuePriority.Low );
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Hue ),
                nameof( ParameterType.SerialOrAlias )
            } )]
        public static void UseType( object type, int hue = -1, object container = null )
        {
            int serial = AliasCommands.ResolveSerial( type );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return;
            }

            if ( container == null )
            {
                container = Engine.Player?.Backpack?.Serial;
            }

            int containerSerial = AliasCommands.ResolveSerial( container );

            if ( !Engine.Items.GetItem( containerSerial, out Item containerItem ) )
            {
                UOC.SystemMessage( Strings.Cannot_find_container___ );

                return;
            }

            Item useItem = hue == -1
                ? containerItem.Container?.SelectEntity( i => i.ID == serial )
                : containerItem.Container?.SelectEntity( i => i.ID == serial && i.Hue == hue );

            if ( useItem == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );

                return;
            }

            if ( !AbilitiesManager.GetInstance().CheckHands( useItem.Serial ) )
            {
                Engine.SendPacketToServer( new UseObject( useItem.Serial ) );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Actions ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.SerialOrAlias ) } )]
        public static void UseTargetedItem( object item, object target )
        {
            int serial = AliasCommands.ResolveSerial( item );
            int targetSerial = AliasCommands.ResolveSerial( target );

            if ( serial == 0 || targetSerial == 0 )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            Engine.SendPacketToServer( new UseTargetedItem( serial, targetSerial ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[]
            {
                nameof( ParameterType.ItemID ), nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Hue )
            } )]
        public static int CountType( int graphic, object source = null, int hue = -1 )
        {
            if ( source == null )
            {
                source = "backpack";
            }

            int sourceSerial = AliasCommands.ResolveSerial( source );

            Item countainerItem = Engine.Items.GetItem( sourceSerial );

            if ( countainerItem?.Container == null )
            {
                UOC.SystemMessage( Strings.Invalid_container___ );

                return 0;
            }

            Item[] matches =
                countainerItem.Container.SelectEntities( i => i.ID == graphic && ( hue == -1 || i.Hue == hue ) );

            return matches?.Sum( i => i.Count ) ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[]
            {
                nameof( ParameterType.ItemID ), nameof( ParameterType.Hue ), nameof( ParameterType.Range )
            } )]
        public static int CountTypeGround( int graphic, int hue = -1, int range = -1 )
        {
            IEnumerable<Item> matches = Engine.Items.Where( i =>
                i.ID == graphic && ( hue == -1 || hue == i.ID ) && ( range == -1 || i.Distance <= range ) );

            int count = matches.Sum( match => match.Count );

            if ( count > 0 )
            {
                return count;
            }

            IEnumerable<Mobile> mobileMatches = Engine.Mobiles.Where( i =>
                i.ID == graphic && ( hue == -1 || hue == i.ID ) && ( range == -1 || i.Distance <= range ) );

            count += mobileMatches.Count();

            return count;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[]
            {
                nameof( ParameterType.ItemID ), nameof( ParameterType.Range ),
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Hue )
            } )]
        public static bool FindType( int graphic, int range = -1, object findLocation = null, int hue = -1 )
        {
            int owner = 0;

            if ( findLocation != null )
            {
                owner = AliasCommands.ResolveSerial( findLocation );
            }

            Entity entity;

            bool Predicate( Entity i )
            {
                return i.ID == graphic && ( hue == -1 || i.Hue == hue ) && !IgnoreList.Contains( i.Serial );
            }

            if ( owner != 0 )
            {
                entity = Engine.Items.SelectEntities( i => Predicate( i ) && i.IsDescendantOf( owner, range ) )
                    ?.FirstOrDefault();
            }
            else
            {
                entity =
                    (Entity) Engine.Mobiles
                        .SelectEntities( i => Predicate( i ) && ( range == -1 || i.Distance < range ) )
                        ?.FirstOrDefault() ?? Engine.Items.SelectEntities( i =>
                        Predicate( i ) && ( range == -1 || i.Distance < range ) && i.Owner == 0 )?.FirstOrDefault();
            }

            if ( entity == null )
            {
                AliasCommands.UnsetAlias( "found" );
                return false;
            }

            AliasCommands.SetMacroAlias( "found", entity.Serial );

            if ( MacroManager.QuietMode )
            {
                return true;
            }

            UOC.SystemMessage( string.Format( Strings.Object___0___updated___, "found" ) );

            return true;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Range ),
                nameof( ParameterType.SerialOrAlias )
            } )]
        public static bool FindObject( object obj, int range = -1, object findLocation = null )
        {
            int owner = 0;

            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return false;
            }

            if ( findLocation != null )
            {
                owner = AliasCommands.ResolveSerial( findLocation );
            }

            Entity entity;

            bool Predicate( Entity i )
            {
                return i.Serial == serial;
            }

            if ( owner != 0 )
            {
                entity = Engine.Items.SelectEntities( i => Predicate( i ) && i.IsDescendantOf( owner, range ) )
                    ?.FirstOrDefault();
            }
            else
            {
                entity =
                    (Entity) Engine.Mobiles
                        .SelectEntities( i => Predicate( i ) && ( range == -1 || i.Distance < range ) )
                        ?.FirstOrDefault() ?? Engine.Items.SelectEntities( i =>
                        Predicate( i ) && ( range == -1 || i.Distance < range ) && i.Owner == 0 )?.FirstOrDefault();
            }

            if ( entity == null )
            {
                AliasCommands.UnsetAlias( "found" );
                return false;
            }

            AliasCommands.SetMacroAlias( "found", entity.Serial );

            if ( MacroManager.QuietMode )
            {
                return true;
            }

            UOC.SystemMessage( string.Format( Strings.Object___0___updated___, "found" ) );

            return true;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.SerialOrAlias ),
                nameof( ParameterType.Amount ), nameof( ParameterType.XCoordinate ),
                nameof( ParameterType.YCoordinate )
            } )]
        public static void MoveItem( object item, object destination, int amount = -1, int x = -1, int y = -1 )
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

            ActionPacketQueue.EnqueueDragDrop( itemSerial, amount, containerSerial, QueuePriority.Low, false, true,
                false, x, y );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.XCoordinateOffset ),
                nameof( ParameterType.YCoordinateOffset ), nameof( ParameterType.ZCoordinateOffset ),
                nameof( ParameterType.Amount )
            } )]
        public static void MoveItemOffset( object obj, int xOffset, int yOffset, int zOffset, int amount = -1 )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return;
            }

            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            int x = player.X + xOffset;
            int y = player.Y + yOffset;
            int z = player.Z + zOffset;

            ActionPacketQueue.EnqueueDragDropGround( serial, amount, x, y, z );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[]
            {
                nameof( ParameterType.ItemID ), nameof( ParameterType.SerialOrAlias ),
                nameof( ParameterType.XCoordinateOffset ), nameof( ParameterType.YCoordinateOffset ),
                nameof( ParameterType.ZCoordinateOffset ), nameof( ParameterType.Amount )
            } )]
        public static bool MoveTypeOffset( int id, object findLocation, int xOffset, int yOffset, int zOffset,
            int amount = -1 )
        {
            if ( findLocation == null ||
                 ( (string) findLocation ).Equals( "ground", StringComparison.InvariantCultureIgnoreCase ) )
            {
                UOC.SystemMessage( Strings.Invalid_container___ );
                return false;
            }

            int owner = AliasCommands.ResolveSerial( findLocation );

            bool Predicate( Item i )
            {
                return i.ID == id && i.IsDescendantOf( owner );
            }

            Item entity = Engine.Items.SelectEntities( Predicate )?.FirstOrDefault();

            if ( entity == null )
            {
                return false;
            }

            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return false;
            }

            int x = player.X + xOffset;
            int y = player.Y + yOffset;
            int z = player.Z + zOffset;

            ActionPacketQueue.EnqueueDragDropGround( entity.Serial, amount, x, y, z );

            return true;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[]
            {
                nameof( ParameterType.ItemID ), nameof( ParameterType.SerialOrAlias ),
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.XCoordinate ),
                nameof( ParameterType.YCoordinate ), nameof( ParameterType.ZCoordinate ),
                nameof( ParameterType.Hue ), nameof( ParameterType.Amount )
            } )]
        public static void MoveType( int id, object sourceContainer, object destinationContainer, int x = -1,
            int y = -1, int z = 0, int hue = -1, int amount = -1 )
        {
            int sourceSerial = AliasCommands.ResolveSerial( sourceContainer );

            if ( sourceSerial == -1 )
            {
                UOC.SystemMessage( Strings.Invalid_source_container___ );
                return;
            }

            int destinationSerial;

            if ( destinationContainer is int destSerial )
            {
                destinationSerial = destSerial;
            }
            else
            {
                destinationSerial = AliasCommands.ResolveSerial( destinationContainer );
            }

            Item sourceItem = Engine.Items.GetItem( sourceSerial );

            if ( sourceItem == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            if ( sourceItem.Container == null )
            {
                UOC.SystemMessage( Strings.Invalid_container___ );
                return;
            }

            Item entity = sourceItem.Container.SelectEntities( i => i.ID == id && ( hue == -1 || i.Hue == hue ) )
                ?.FirstOrDefault();

            if ( entity == null )
            {
                return;
            }

            if ( amount == -1 )
            {
                amount = entity.Count;
            }

            if ( amount > entity.Count )
            {
                amount = entity.Count;
            }

            if ( destinationSerial == -1 )
            {
                ActionPacketQueue.EnqueueDragDropGround( entity.Serial, amount, x, y, z );
            }
            else
            {
                ActionPacketQueue.EnqueueDragDrop( entity.Serial, amount, destinationSerial );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.Layer ), nameof( ParameterType.SerialOrAlias ) } )]
        public static bool UseLayer( object layer, object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 || !UOMath.IsMobile( serial ) )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
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

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile == null )
            {
                UOC.SystemMessage( Strings.Mobile_not_found___ );
                return false;
            }

            int layerSerial = mobile.GetLayer( layerValue );

            if ( layerSerial == 0 )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return false;
            }

            UseObject( layerSerial );

            return true;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool InIgnoreList( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial > 0 )
            {
                return IgnoreList.Contains( serial );
            }

            UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

            return false;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Hue ) } )]
        public static void Rehue( object obj, int hue )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial > 0 )
            {
                if ( hue > 0 )
                {
                    Engine.RehueList.Add( serial, hue );
                }
                else
                {
                    Engine.RehueList.Remove( serial );
                }

                if ( UOMath.IsMobile( serial ) )
                {
                    Mobile m = Engine.Mobiles.GetMobile( serial );

                    if ( m == null )
                    {
                        return;
                    }

                    Engine.SendPacketToClient( new MobileIncoming( m, m.Equipment, hue ) );
                }
                else
                {
                    Item i = Engine.Items.GetItem( serial );

                    if ( i == null )
                    {
                        return;
                    }

                    Engine.RehueList.CheckItem( i );
                }

                return;
            }

            UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ), Parameters = new[] { nameof( ParameterType.Hue ) } )]
        public static void AutoColorPick( int hue )
        {
            Engine.RemoveReceiveFilter( new PacketFilterInfo( 0x95 ) );
            Engine.AddReceiveFilter( new PacketFilterInfo( 0x95, ( p, pfi ) =>
            {
                int serial = ( p[1] << 24 ) | ( p[2] << 16 ) | ( p[3] << 8 ) | p[4];
                int itemid = ( p[5] << 8 ) | p[6];

                Engine.SendPacketToServer( new HuePickerResponse( serial, itemid, hue ) );
                Engine.RemoveReceiveFilter( pfi );
            } ) );
        }
    }
}