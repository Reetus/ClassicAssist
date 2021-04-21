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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Gumps
{
    public class MacrosGump : RepositionableGump
    {
        private static Timer _timer;
        private static string _lastList;
        private static int _serial = 0x0fe00000;

        public MacrosGump( string html ) : base( 190, 180, _serial++, (uint) _serial++ )
        {
            GumpX = Options.CurrentOptions.MacrosGumpX;
            GumpY = Options.CurrentOptions.MacrosGumpY;

            Movable = false;
            Closable = false;
            Resizable = false;
            Disposable = false;
            AddPage( 0 );
            AddBackground( 0, 0, 190, 180, 3500 );
            AddHtml( 20, 20, 150, 140, html, false, true );
        }

        public static void ResendGump( bool force = false )
        {
            try
            {
                MacroManager _macroManager = MacroManager.GetInstance();

                IEnumerable<MacroEntry> macro = _macroManager.Items.Where( e => e.IsRunning ).ToArray();

                string html = string.Empty;

                foreach ( MacroEntry entry in macro )
                {
                    if ( entry.IsBackground )
                    {
                        html += $"<BASEFONT COLOR=#000000><I>{entry.Name}</I></BASEFONT>\n";
                    }
                    else
                    {
                        html += $"<BASEFONT COLOR=#000000>{entry.Name}</BASEFONT>\n";
                    }
                }

                if ( html.Equals( _lastList ) && !force )
                {
                    return;
                }

                if ( Engine.Gumps.GetGumps( out Gump[] gumps ) )
                {
                    foreach ( Gump macrosGump in gumps.Where( g => g is MacrosGump ) )
                    {
                        Commands.CloseClientGump( macrosGump.ID );
                    }
                }

                MacrosGump gump = new MacrosGump( html );
                gump.SendGump();

                _lastList = html;
            }
            catch ( InvalidOperationException e )
            {
                Console.WriteLine( e.ToString() );
            }
        }

        public static void Initialize()
        {
            _timer = new Timer( o => ResendGump(), null, 1000, 250 );
        }

        public override void SetPosition( int x, int y )
        {
            base.SetPosition( x, y );

            Options.CurrentOptions.MacrosGumpX = x;
            Options.CurrentOptions.MacrosGumpY = y;

            ResendGump( true );
        }
    }
}