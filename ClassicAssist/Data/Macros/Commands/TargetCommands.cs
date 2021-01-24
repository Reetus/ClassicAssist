using System;
using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Data.Regions;
using ClassicAssist.Data.Targeting;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using TargetTypeEnum = ClassicAssist.UO.Data.TargetType;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class TargetCommands
    {
        public enum TargetExistsType
        {
            Any,
            Beneficial,
            Harmful,
            Neutral
        }

        public enum TargetResourceType
        {
            Ore,
            Sand,
            Wood,
            Graves,
            Red_Mushrooms
        }

        private static readonly ushort[] _treeTiles =
        {
            0x0CCA, 0x0CCB, 0x0CCC, 0x0CCD, 0x0CD0, 0x0CD3, 0x0CD6, 0x0CD8, 0x0CDA, 0x0CDD, 0x0CE0, 0x0CE3, 0x0CE6,
            0x0D41, 0x0D42, 0x0D43, 0x0D44, 0x0D57, 0x0D58, 0x0D59, 0x0D5A, 0x0D5B, 0x0D6E, 0x0D6F, 0x0D70, 0x0D71,
            0x0D72, 0x0D84, 0x0D85, 0x0D86, 0x0D94, 0x0D98, 0x0D9C, 0x0DA0, 0x0DA4, 0x0DA8, 0x0C9E, 0x0CA8, 0x0CAA,
            0x0CAB, 0x0CC9, 0x0CF8, 0x0CFB, 0x0CFE, 0x0D01, 0x12B6, 0x12B7, 0x12B8, 0x12B9, 0x12BA, 0x12BB, 0x12BC,
            0x12BD
        };

        private static readonly ushort[] _caveTiles =
        {
            1339, 1340, 1341, 1342, 1343, 1344, 1345, 1346, 1347, 1348, 1349, 1350, 1351, 1352, 1353, 1354, 1355,
            1356, 1357, 1358, 1359, 1361, 1362, 1363
        };

        [CommandsDisplay( Category = nameof( Strings.Target ) )]
        public static void CancelTarget()
        {
            Engine.SendPacketToServer( new Target( TargetTypeEnum.Object, -1, TargetFlags.Cancel, -1, -1, -1, 0, 0,
                true ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ), Parameters = new[] { nameof( ParameterType.Timeout ) } )]
        public static bool WaitForTarget( int timeout = 5000 )
        {
            return UOC.WaitForTarget( timeout );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void Target( object obj, bool checkRange = false, bool useQueue = false )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                return;
            }

            if ( checkRange && UOMath.IsMobile( serial ) && Engine.TargetExists )
            {
                Mobile mobile = Engine.Mobiles.GetMobile( serial );

                if ( mobile != null )
                {
                    if ( mobile.Distance > Options.CurrentOptions.RangeCheckLastTargetAmount )
                    {
                        UOC.SystemMessage( Strings.Target_out_of_range__try_again___ );
                        UOC.ResendTargetToClient();
                        return;
                    }
                }
                else
                {
                    UOC.SystemMessage( Strings.Target_out_of_range__try_again___ );
                    UOC.ResendTargetToClient();
                    return;
                }
            }

            if ( Options.CurrentOptions.PreventTargetingInnocentsInGuardzone && Engine.TargetExists )
            {
                Mobile mobile = Engine.Mobiles.GetMobile( serial );

                if ( mobile != null && mobile.Notoriety == Notoriety.Innocent &&
                     mobile.Serial != Engine.Player?.Serial && Engine.TargetFlags == TargetFlags.Harmful &&
                     mobile.GetRegion().Attributes.HasFlag( RegionAttributes.Guarded ) )
                {
                    UOC.SystemMessage( Strings.Target_blocked____try_again___ );
                    UOC.ResendTargetToClient();

                    return;
                }
            }

            if ( useQueue && !Engine.TargetExists )
            {
                MsgCommands.HeadMsg( Strings.Target_Queued, Engine.Player?.Serial );
                Engine.LastTargetQueue.Enqueue( obj );
                return;
            }

            Engine.SendPacketToServer( new Target( TargetTypeEnum.Object, -1, TargetFlags.None, serial, -1, -1, -1, 0,
                true ) );
            Engine.TargetExists = false;
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.String ) } )]
        [CommandsDisplayStringSeeAlso( new[] { null, nameof( TargetResourceType ) } )]
        public static void TargetByResource( object toolObj, string resourceType )
        {
            int serial = AliasCommands.ResolveSerial( toolObj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            try
            {
                TargetResourceType resourceEnum = Utility.GetEnumValueByName<TargetResourceType>( resourceType );

                Engine.SendPacketToServer( new TargetByResource( serial, (int) resourceEnum ) );
            }
            catch ( InvalidOperationException )
            {
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.IntegerValue ),
                nameof( ParameterType.Boolean ), nameof( ParameterType.ItemID )
            } )]
        public static void TargetTileRelative( object obj, int distance, bool reverse = false, int itemID = 0 )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Entity entity = Engine.Mobiles.GetMobile( serial );

            if ( entity == null )
            {
                if ( !MacroManager.QuietMode )
                {
                    UOC.SystemMessage( Strings.Mobile_not_found___ );
                }

                return;
            }

            int x = entity.X;
            int y = entity.Y;

            int offsetX = 0;
            int offsetY = 0;

            // TODO
            Direction direction = (Direction) ( (int) entity.Direction & ~0x80 );

            switch ( direction )
            {
                case Direction.North:
                    offsetY = -1;
                    break;
                case Direction.Northeast:
                    offsetY = -1;
                    offsetX = 1;
                    break;
                case Direction.East:
                    offsetX = 1;
                    break;
                case Direction.Southeast:
                    offsetX = 1;
                    offsetY = 1;
                    break;
                case Direction.South:
                    offsetY = 1;
                    break;
                case Direction.Southwest:
                    offsetY = 1;
                    offsetX = -1;
                    break;
                case Direction.West:
                    offsetX = -1;
                    break;
                case Direction.Northwest:
                    offsetX = -1;
                    offsetY = -1;
                    break;
                case Direction.Invalid:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            int totalOffsetX = offsetX * distance;
            int totalOffsetY = offsetY * distance;

            if ( reverse )
            {
                totalOffsetX = -totalOffsetX;
                totalOffsetY = -totalOffsetY;
            }

            int destinationX = x + totalOffsetX;
            int destinationY = y + totalOffsetY;

            ( int surfaceZ, int surfaceID ) =
                MapInfo.GetMapSurface( (int) Engine.Player.Map, destinationX, destinationY );

            if ( itemID == 0 )
            {
                itemID = surfaceID;
            }

            TargetXYZ( destinationX, destinationY, surfaceZ, itemID );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[]
            {
                nameof( ParameterType.XCoordinateOffset ), nameof( ParameterType.YCoordinateOffset ),
                nameof( ParameterType.YCoordinateOffset ), nameof( ParameterType.ItemID )
            } )]
        public static void TargetTileOffset( int xOffset, int yOffset, int zOffset, int itemID = 0 )
        {
            TargetXYZ( Engine.Player.X + xOffset, Engine.Player.Y + yOffset, Engine.Player.Z + zOffset, itemID );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[]
            {
                nameof( ParameterType.XCoordinateOffset ), nameof( ParameterType.YCoordinateOffset ),
                nameof( ParameterType.YCoordinateOffset ), nameof( ParameterType.ItemID )
            } )]
        public static void TargetTileOffsetResource( int xOffset, int yOffset, int zOffset, int itemID = 0 )
        {
            int x = Engine.Player.X + xOffset;
            int y = Engine.Player.Y + yOffset;
            int z = Engine.Player.Z + zOffset;

            if ( itemID == 0 )
            {
                StaticTile[] staticTiles = Statics.GetStatics( (int) Engine.Player.Map, x, y );

                if ( staticTiles != null )
                {
                    StaticTile selectedStatic = staticTiles.FirstOrDefault( i =>
                        _treeTiles.Contains( i.ID ) || _caveTiles.Contains( i.ID ) ||
                        i.Flags.HasFlag( TileFlags.Wet ) );

                    if ( selectedStatic.ID == 0 )
                    {
                        selectedStatic = staticTiles.FirstOrDefault();
                    }

                    itemID = selectedStatic.ID;
                    z = selectedStatic.Z;
                }
            }

            TargetXYZ( x, y, z, itemID );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[]
            {
                nameof( ParameterType.Empty ), nameof( ParameterType.Empty ), nameof( ParameterType.Empty ),
                nameof( ParameterType.Empty )
            } )]
        [CommandsDisplayStringSeeAlso( new[]
        {
            nameof( TargetNotoriety ), nameof( TargetBodyType ), nameof( TargetDistance ),
            nameof( TargetInfliction )
        } )]
        public static bool GetEnemy( IEnumerable<string> notorieties, string bodyType = "Any", string distance = "Next",
            string infliction = "Any" )
        {
            TargetNotoriety notoFlags = TargetNotoriety.None;

            foreach ( string noto in notorieties )
            {
                if ( Enum.TryParse( noto, true, out TargetNotoriety flag ) )
                {
                    notoFlags |= flag;
                }
            }

            if ( !Enum.TryParse( bodyType, true, out TargetBodyType bt ) )
            {
                bt = TargetBodyType.Any;
            }

            if ( !Enum.TryParse( distance, true, out TargetDistance td ) )
            {
                td = TargetDistance.Next;
            }

            if ( !Enum.TryParse( infliction, true, out TargetInfliction ti ) )
            {
                ti = TargetInfliction.Any;
            }

            return TargetManager.GetInstance().GetEnemy( notoFlags, bt, td, TargetFriendType.None, ti );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[]
            {
                nameof( ParameterType.Empty ), nameof( ParameterType.Empty ), nameof( ParameterType.Empty )
            } )]
        [CommandsDisplayStringSeeAlso( new[]
        {
            nameof( TargetDistance ), nameof( TargetInfliction ), nameof( TargetBodyType )
        } )]
        public static bool GetFriendListOnly( string distance = "Next", string targetInfliction = "Any",
            string bodyType = "Any" )
        {
            if ( !Enum.TryParse( distance, true, out TargetDistance td ) )
            {
                td = TargetDistance.Next;
            }

            if ( !Enum.TryParse( targetInfliction, true, out TargetInfliction ti ) )
            {
                ti = TargetInfliction.Any;
            }

            if ( !Enum.TryParse( bodyType, true, out TargetBodyType bt ) )
            {
                bt = TargetBodyType.Any;
            }

            return TargetManager.GetInstance().GetFriend( TargetNotoriety.Any, bt, td, TargetFriendType.Only, ti );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[]
            {
                nameof( ParameterType.Empty ), nameof( ParameterType.Empty ), nameof( ParameterType.Empty ),
                nameof( ParameterType.Empty )
            } )]
        [CommandsDisplayStringSeeAlso( new[]
        {
            nameof( TargetNotoriety ), nameof( TargetBodyType ), nameof( TargetDistance ),
            nameof( TargetInfliction )
        } )]
        public static bool GetFriend( IEnumerable<string> notorieties, string bodyType = "Any",
            string distance = "Next", string infliction = "Any" )
        {
            TargetNotoriety notoFlags = TargetNotoriety.None;

            foreach ( string noto in notorieties )
            {
                if ( Enum.TryParse( noto, true, out TargetNotoriety flag ) )
                {
                    notoFlags |= flag;
                }
            }

            if ( !Enum.TryParse( bodyType, true, out TargetBodyType bt ) )
            {
                bt = TargetBodyType.Any;
            }

            if ( !Enum.TryParse( distance, true, out TargetDistance td ) )
            {
                td = TargetDistance.Next;
            }

            if ( !Enum.TryParse( infliction, true, out TargetInfliction ti ) )
            {
                ti = TargetInfliction.Any;
            }

            return TargetManager.GetInstance().GetFriend( notoFlags, bt, td, TargetFriendType.Include, ti );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[] { nameof( ParameterType.BeneficialHarmfulNeutral ) } )]
        [CommandsDisplayStringSeeAlso( new[] { nameof( TargetExistsType ) } )]
        public static bool TargetExists( string targetExistsType = "Any" )
        {
            if ( !Enum.TryParse( targetExistsType, out TargetExistsType enumValue ) )
            {
                enumValue = TargetExistsType.Any;
            }

            switch ( enumValue )
            {
                case TargetExistsType.Any:

                    return Engine.TargetExists;

                case TargetExistsType.Beneficial:

                    return Engine.TargetExists && Engine.TargetFlags == TargetFlags.Beneficial;

                case TargetExistsType.Harmful:

                    return Engine.TargetExists && Engine.TargetFlags == TargetFlags.Harmful;

                case TargetExistsType.Neutral:

                    return Engine.TargetExists && Engine.TargetFlags == TargetFlags.None;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Target ) )]
        public static bool WaitingForTarget()
        {
            return Engine.WaitingForTarget;
        }

        [CommandsDisplay( Category = nameof( Strings.Target ) )]
        public static void ClearTargetQueue()
        {
            Engine.LastTargetQueue?.Clear();
            UOC.SystemMessage( Strings.Target_queue_cleared___ );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Hue ), nameof( ParameterType.Range )
            } )]
        public static void TargetType( object obj, int hue = -1, int range = -1 )
        {
            int id = AliasCommands.ResolveSerial( obj );

            if ( id == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            if ( Engine.Player?.Backpack == null )
            {
                UOC.SystemMessage( Strings.Error__Cannot_find_player_backpack );
                return;
            }

            Item item = Engine.Items.SelectEntity( i =>
                i.ID == id && ( hue == -1 || i.Hue == hue ) &&
                i.IsDescendantOf( Engine.Player.Backpack.Serial, range ) );

            if ( item == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            Target( item.Serial, false, Options.CurrentOptions.QueueLastTarget );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Hue ), nameof( ParameterType.Range )
            } )]
        public static void TargetGround( object obj, int hue = -1, int range = -1 )
        {
            int id = AliasCommands.ResolveSerial( obj );

            if ( id == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Entity entity =
                (Entity) Engine.Items.SelectEntity( i =>
                    i.ID == id && ( hue == -1 || i.Hue == hue ) && ( range == -1 || i.Distance < range ) ) ??
                Engine.Mobiles.SelectEntity( m =>
                    m.ID == id && ( hue == -1 || m.Hue == hue ) && ( range == -1 || m.Distance < range ) );

            if ( entity == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            Target( entity.Serial, false, Options.CurrentOptions.QueueLastTarget );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void SetEnemy( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return;
            }

            Entity entity = Engine.Items.GetItem( serial ) ?? (Entity) Engine.Mobiles.GetMobile( serial );

            if ( entity == null )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            TargetManager.GetInstance().SetEnemy( entity );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void SetFriend( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return;
            }

            Entity entity = Engine.Items.GetItem( serial ) ?? (Entity) Engine.Mobiles.GetMobile( serial );

            if ( entity == null )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            TargetManager.GetInstance().SetFriend( entity );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void SetLastTarget( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return;
            }

            Entity entity = Engine.Items.GetItem( serial ) ?? (Entity) Engine.Mobiles.GetMobile( serial );

            if ( entity == null )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            TargetManager.GetInstance().SetLastTarget( entity );
        }

        [CommandsDisplay( Category = nameof( Strings.Target ), Parameters = new[] { nameof( ParameterType.Timeout ) } )]
        public static bool WaitForTargetOrFizzle( int timeout )
        {
            ( _, bool result ) = UOC.WaitForTargetOrFizzle( timeout );

            return result;
        }

        [CommandsDisplay( Category = nameof( Strings.Target ),
            Parameters = new[]
            {
                nameof( ParameterType.XCoordinate ), nameof( ParameterType.YCoordinate ),
                nameof( ParameterType.ZCoordinate ), nameof( ParameterType.ItemID )
            } )]
        public static void TargetXYZ( int x, int y, int z, int itemID = 0 )
        {
            Engine.SendPacketToServer( new Target( TargetTypeEnum.Tile, -1, Engine.TargetFlags, 0, x, y, z, itemID,
                true ) );
            Engine.TargetExists = false;
        }
    }
}