using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MovementCommands
    {
        private const int MOVEMENT_TIMEOUT = 500;

        [CommandsDisplay( Category = "Actions", Description = "Walk in the given direction.",
            InsertText = "Walk(\"east\")" )]
        public static bool Walk( string direction )
        {
            return Move( direction, false );
        }

        [CommandsDisplay( Category = "Actions", Description = "Turn in the given direction.",
            InsertText = "Turn(\"east\")" )]
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

        [CommandsDisplay( Category = "Actions", Description = "Run in the given direction.",
            InsertText = "Run(\"east\")" )]
        public static bool Run( string direction )
        {
            return Move( direction, true );
        }

        private static bool Move( string direction, bool run )
        {
            Direction directionEnum = Utility.GetEnumValueByName<Direction>( direction );

            bool result = Engine.Move( directionEnum, run );

            UOC.WaitForIncomingPacket( new PacketFilterInfo( 22 ), MOVEMENT_TIMEOUT );

            return result;
        }
    }
}