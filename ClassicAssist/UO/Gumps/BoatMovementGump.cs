#region License

// Copyright (C) 2020 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Gumps
{
    public class BoatGumpExtension : IExtension
    {
        public void Initialize()
        {
            IncomingPacketHandlers.JournalEntryAddedEvent += OnJournalEntryAddedEvent;
        }

        private static void OnJournalEntryAddedEvent( JournalEntry je )
        {
            switch ( je.Cliloc )
            {
                /* You are now piloting this vessel. */
                case 1116727:
                    new BoatMovementGump().SendGump();
                    break;
                /* You are no longer piloting this vessel. */
                case 1149592:
                    Commands.CloseClientGump( typeof( BoatMovementGump ) );
                    break;
            }
        }
    }

    public class BoatMovementGump : Gump
    {
        private static BoatSpeed _lastSpeed = BoatSpeed.OneTile;

        public BoatMovementGump() : base( 100, 100 )
        {
            Movable = true;
            Closable = true;
            Resizable = false;
            Disposable = false;
            AddPage( 0 );
            AddBackground( 0, 0, 150, 175, 9200 );
            AddButton( 0, 0, 4507, 4507, (int) Direction.Northwest + 1, GumpButtonType.Reply, 0 );
            AddButton( 50, 0, 4500, 4500, (int) Direction.North + 1, GumpButtonType.Reply, 0 );
            AddButton( 100, 0, 4501, 4501, (int) Direction.Northeast + 1, GumpButtonType.Reply, 0 );
            AddButton( 10, 50, 4506, 4506, (int) Direction.West + 1, GumpButtonType.Reply, 0 );
            AddButton( 90, 50, 4502, 4502, (int) Direction.East + 1, GumpButtonType.Reply, 0 );
            AddButton( 0, 100, 4505, 4505, (int) Direction.Southwest + 1, GumpButtonType.Reply, 0 );
            AddButton( 50, 100, 4504, 4504, (int) Direction.South + 1, GumpButtonType.Reply, 0 );
            AddButton( 100, 100, 4503, 4503, (int) Direction.Southeast + 1, GumpButtonType.Reply, 0 );
            AddRadio( 10, 150, 208, 209, _lastSpeed == BoatSpeed.Normal, 1 );
            AddLabel( 40, 150, 0, "Fast" );
            AddButton( 60, 60, 2151, 2151, 10, GumpButtonType.Reply, 0 );
        }

        public override void OnResponse( int buttonID, int[] switches, Dictionary<int, string> textEntries )
        {
            if ( buttonID == 0 )
            {
                return;
            }

            BoatSpeed speed = BoatSpeed.OneTile;

            if ( switches?.Any( e => e == 1 ) ?? false )
            {
                speed = BoatSpeed.Normal;
            }

            _lastSpeed = speed;

            if ( buttonID < 9 )
            {
                Engine.SendPacketToServer( new WheelBoatMoving( (Direction) buttonID - 1, speed ) );
            }

            if ( buttonID == 10 )
            {
                Engine.SendPacketToServer( new WheelBoatMoving( Direction.North, BoatSpeed.Stop ) );
            }

            new BoatMovementGump().SendGump();
        }
    }
}