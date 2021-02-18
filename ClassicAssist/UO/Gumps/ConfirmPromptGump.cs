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
using System.Threading;
using Assistant;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Gumps
{
    public class ConfirmPromptGump : Gump
    {
        public ConfirmPromptGump( string message, bool closable = false ) : base( 500, 500 )
        {
            const int width = 300;
            const int height = 200;

            SetCenterPosition( width, height );
            Closable = closable;
            Resizable = false;
            Disposable = false;
            Movable = true;
            AddPage( 0 );
            AddBackground( 0, 0, width, height, 9200 );
            AddHtml( 10, 10, width - 20, height - 50, message, true, true );
            AddButton( width - 140, height - 30, 247, 248, 1, GumpButtonType.Reply, 0 );
            AddButton( width - 70, height - 30, 241, 242, 2, GumpButtonType.Reply, 0 );
        }

        public AutoResetEvent AutoResetEvent { get; set; } = new AutoResetEvent( false );

        public bool Result { get; set; }

        public override void OnResponse( int buttonID, int[] switches,
            List<(int Key, string Value)> textEntries = null )
        {
            Result = buttonID == 1;
            AutoResetEvent.Set();
        }

        public static bool ConfirmPrompt( string message, bool closable = false )
        {
            ConfirmPromptGump gump = new ConfirmPromptGump( message, closable );

            PacketFilterInfo pfi = new PacketFilterInfo( 0xB1,
                new[] { PacketFilterConditions.UIntAtPositionCondition( gump.ID, 7 ) } );

            Engine.AddSendPostFilter( pfi );

            gump.SendGump();

            gump.AutoResetEvent.WaitOne();

            Engine.RemoveSendPostFilter( pfi );

            return gump.Result;
        }
    }
}