#region License

// Copyright (C) 2021 Reetus
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
using System.Threading;
using Assistant;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Gumps
{
    public class SelectionPromptGump : Gump
    {
        public SelectionPromptGump( IEnumerable<string> options, string message, bool closable = false ) : base( 500,
            500 )
        {
            string[] optionsArray = options.ToArray();

            const int width = 400;
            int height = 150 + optionsArray.Length * 20;

            SetCenterPosition( width, height );
            Closable = closable;
            Resizable = false;
            Disposable = false;
            AddPage( 0 );
            AddBackground( 0, 0, width, height, 9200 );
            AddHtml( 10, 10, width - 20, 70, message, true, true );

            int y = 100;

            for ( int index = 0; index < optionsArray.Length; index++ )
            {
                AddRadio( 10, y, 208, 209, index == 0, index );
                AddLabel( 40, y, 0, optionsArray[index] );
                y += 20;
            }

            AddButton( width - 140, height - 30, 247, 248, 1, GumpButtonType.Reply, 0 );
            AddButton( width - 70, height - 30, 241, 242, 0, GumpButtonType.Reply, 0 );
        }

        public AutoResetEvent AutoResetEvent { get; set; } = new AutoResetEvent( false );
        public int Index { get; set; }
        public bool Result { get; set; }

        public override void OnResponse( int buttonID, int[] switches,
            List<(int Key, string Value)> textEntries = null )
        {
            Result = buttonID != 0;

            if ( switches != null && switches.Length > 0 )
            {
                Index = switches[0];
            }

            AutoResetEvent.Set();
        }

        public static (bool Result, int Index) SelectionPrompt( IEnumerable<string> options, string message,
            bool closable = false )
        {
            SelectionPromptGump gump = new SelectionPromptGump( options, message, closable );

            PacketFilterInfo pfi = new PacketFilterInfo( 0xB1,
                new[] { PacketFilterConditions.UIntAtPositionCondition( gump.ID, 7 ) } );

            Engine.AddSendPostFilter( pfi );

            gump.SendGump();

            gump.AutoResetEvent.WaitOne();

            Engine.RemoveSendPostFilter( pfi );

            return ( gump.Result, gump.Index );
        }
    }
}