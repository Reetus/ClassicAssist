using System;
using System.Collections.Generic;
using Assistant;
using ClassicAssist.Data.Targeting;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class TargetCommands
    {
        [CommandsDisplay( Category = "Target", Description = "Cancel an existing cursor/target.",
            InsertText = "CancelTarget()" )]
        public static void CancelTarget()
        {
            Engine.SendPacketToServer( new Target( TargetType.Object, -1, TargetFlags.Cancel, -1, -1, -1,
                0, 0 ) );
            Engine.SendPacketToClient( new Target( TargetType.Object, -1, TargetFlags.Cancel, -1, -1, -1,
                0, 0 ) );
        }

        [CommandsDisplay( Category = "Target",
            Description = "Wait for target packet from server, optional timeout parameter (default 5000 milliseconds).",
            InsertText = "WaitForTarget(5000)" )]
        public static bool WaitForTarget( int timeout = 5000 )
        {
            return UOC.WaitForTarget( timeout );
        }

        [CommandsDisplay( Category = "Target",
            Description = "Targets the given object (parameter can be serial or alias).",
            InsertText = "Target(\"self\")" )]
        public static void Target( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                return;
            }

            Engine.SendPacketToServer( new Target( TargetType.Object, -1, TargetFlags.None, serial, -1, -1, -1, 0 ) );
            Engine.SendPacketToClient( new Target( TargetType.Object, -1, TargetFlags.Cancel, -1, -1, -1,
                0, 0 ) );
        }

        [CommandsDisplay( Category = "Target",
            Description =
                "Target tile the given distance relative to the specified alias/serial, optional boolean for reverse mode.",
            InsertText = "TargetTileRelative(\"self\", 1, False)" )]
        public static void TargetTileRelative( object obj, int distance, bool reverse = false )
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
                UOC.SystemMessage( Strings.Mobile_not_found___ );
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

            Engine.SendPacketToServer( new Target( TargetType.Tile, -1, TargetFlags.None, 0, destinationX,
                destinationY, entity.Z, 0 ) );
            Engine.SendPacketToClient( new Target( TargetType.Object, -1, TargetFlags.Cancel, -1, -1, -1,
                0, 0 ) );
        }

        [CommandsDisplay( Category = "Target", Description = "Get mobile and set enemy alias.",
            InsertText = "GetEnemy([\"Murderer\"])" )]
        public static void GetEnemy( IEnumerable<string> notos, string bodyType = "Any", string distance = "Next" )
        {
            TargetNotoriety notoFlags = TargetNotoriety.None;

            foreach ( string noto in notos )
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

            TargetManager.GetInstance().GetEnemy( notoFlags, bt, td );
        }

        [CommandsDisplay( Category = "Target", Description = "Get mobile and set friend alias.",
            InsertText = "GetFriend([\"Murderer\"])" )]
        public static void GetFriend( IEnumerable<string> notos, string bodyType = "Any", string distance = "Next" )
        {
            TargetNotoriety notoFlags = TargetNotoriety.None;

            foreach ( string noto in notos )
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

            TargetManager.GetInstance().GetFriend( notoFlags, bt, td );
        }
    }
}