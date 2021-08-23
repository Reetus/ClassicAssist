using System;
using Assistant;
using ClassicAssist.Data.ClassicUO.Objects;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MovementCommands
    {
        private const int MOVEMENT_TIMEOUT = 500;
        private const int PATHFIND_MAX_DISTANCE = 32;
        private static bool _forceWalk;

        [CommandsDisplay( Category = nameof( Strings.Movement ),
            Parameters = new[] { nameof( ParameterType.Direction ) } )]
        public static bool Walk( string direction )
        {
            return Move( direction, false );
        }

        [CommandsDisplay( Category = nameof( Strings.Movement ) )]
        public static void SetForceWalk( bool force )
        {
            UOC.SetForceWalk( force );
            UOC.SystemMessage( force ? Strings.Force_Walk_On : Strings.Force_Walk_Off );
        }

        [CommandsDisplay( Category = nameof( Strings.Movement ) )]
        public static void ToggleForceWalk()
        {
            _forceWalk = !_forceWalk;

            UOC.SetForceWalk( _forceWalk );
            UOC.SystemMessage( _forceWalk ? Strings.Force_Walk_On : Strings.Force_Walk_Off );
        }

        [CommandsDisplay( Category = nameof( Strings.Movement ),
            Parameters = new[] { nameof( ParameterType.Direction ) } )]
        [CommandsDisplayStringSeeAlso( new[] { nameof( Direction ) } )]
        public static void Turn( string direction )
        {
            Direction directionEnum = Utility.GetEnumValueByName<Direction>( direction );

            if ( Engine.Player.Direction == directionEnum )
            {
                return;
            }

            Engine.Move( directionEnum, false );
            UOC.WaitForIncomingPacket( new PacketFilterInfo( 22 ), MOVEMENT_TIMEOUT );
        }

        [CommandsDisplay( Category = nameof( Strings.Movement ),
            Parameters = new[] { nameof( ParameterType.Direction ) } )]
        [CommandsDisplayStringSeeAlso( new[] { nameof( Direction ) } )]
        public static bool Run( string direction )
        {
            return Move( direction, true );
        }

        private static bool Move( string direction, bool run )
        {
            Direction directionEnum = Utility.GetEnumValueByName<Direction>( direction );

            if ( directionEnum == Direction.Invalid )
            {
                return false;
            }

            try
            {
                bool result = Engine.Move( directionEnum, run );
                UOC.WaitForIncomingPacket( new PacketFilterInfo( 22 ), MOVEMENT_TIMEOUT );
                return result;
            }
            catch ( IndexOutOfRangeException )
            {
            }

            return false;
        }

        [CommandsDisplay( Category = nameof( Strings.Movement ) )]
        public static bool Following()
        {
            dynamic gameScene = new GameScene();

            return gameScene._followingMode;
        }

        [CommandsDisplay( Category = nameof( Strings.Movement ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void Follow( object obj = null )
        {
            int serial = 0;

            if ( obj != null )
            {
                serial = AliasCommands.ResolveSerial( obj, false );
            }

            dynamic gameScene = new GameScene();

            if ( obj == null )
            {
                if ( gameScene._followingMode )
                {
                    UOC.SystemMessage( Strings.Deactivated_following, UOC.SystemMessageHues.Normal, true );
                }

                gameScene._followingMode = false;
            }
            else
            {
                gameScene._followingMode = true;
                gameScene._followingTarget = (uint) serial;
                UOC.SystemMessage( Strings.Activated_following, UOC.SystemMessageHues.Normal, true );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Movement ),
            Parameters = new[]
            {
                nameof( ParameterType.XCoordinate ), nameof( ParameterType.YCoordinate ),
                nameof( ParameterType.ZCoordinate )
            } )]
        public static bool Pathfind( int x, int y, int z )
        {
            int distance = Math.Max( Math.Abs( x - Engine.Player?.X ?? x ), Math.Abs( y - Engine.Player?.Y ?? y ) );

            if ( distance > PATHFIND_MAX_DISTANCE )
            {
                UOC.SystemMessage( Strings.Maximum_distance_exceeded_ );
                return false;
            }

            return Pathfinder.WalkTo( x, y, z, 0 );
        }

        [CommandsDisplay( Category = nameof( Strings.Movement ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Pathfind( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Entity_not_found___ );
                return false;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? (Entity) Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial );

            if ( entity == null )
            {
                UOC.SystemMessage( Strings.Entity_not_found___ );
                return false;
            }

            return Pathfind( entity.X, entity.Y, entity.Z );
        }

        [CommandsDisplay( Category = nameof( Strings.Movement ) )]
        public static bool Pathfinding()
        {
            return Pathfinder.AutoWalking;
        }
    }
}