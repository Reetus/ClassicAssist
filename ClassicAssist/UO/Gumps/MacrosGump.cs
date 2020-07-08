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
using System.Threading;
using System.Threading.Tasks;
using ClassicAssist.Data.Macros;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Gumps
{
    public class MacrosGump : Gump
    {
        private static readonly object _lock = new object();

        public MacrosGump( string html ) : base( 100, 100 )
        {
            Movable = true;
            Closable = true;
            Resizable = false;
            Disposable = false;
            AddPage( 0 );
            AddBackground( 450, 180, 190, 180, 3500 );
            AddHtml( 470, 200, 150, 140, html, false, true );
        }

        public static async Task ResendGump()
        {
            if ( !Monitor.TryEnter( _lock ) )
            {
                return;
            }

            try
            {
                MacroManager _macroManager = MacroManager.GetInstance();

                IEnumerable<MacroEntry> macro = _macroManager.Items.Where( e => e.IsRunning );

                string html = string.Empty;

                foreach ( MacroEntry entry in macro )
                {
                    if ( entry.IsBackground )
                    {
                        html += $"<BASEFONT COLOR=#000000><I>{entry.Name}</I></FONT>\n";
                    }
                    else
                    {
                        html += $"<BASEFONT COLOR=#000000>{entry.Name}</FONT>\n";
                    }
                }

                MacrosGump gump = new MacrosGump( html );
                Commands.CloseClientGump( gump.ID );
                gump.SendGump();

                await Task.Delay( 50 );
            }
            finally
            {
                Monitor.Exit( _lock );
            }
        }
    }
}