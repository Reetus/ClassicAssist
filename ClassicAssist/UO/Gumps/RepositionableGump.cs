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
using System.Windows.Interop;
using Assistant;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Gumps
{
    public abstract class RepositionableGump : Gump
    {
        private const int REPOSITION_BUTTON_ID = 10;
        private readonly int _height;
        private readonly int _width;

        protected RepositionableGump( int width, int height, int serial, uint gumpID ) : base( 0, 0, serial,
            gumpID )
        {
            _width = width;
            _height = height;
        }

        public int GumpX { get; set; } = 100;
        public int GumpY { get; set; } = 100;

        public override void SendGump()
        {
            X = GumpX;
            Y = GumpY;
            AddButton( _width - 15, 5, 0x82C, 0x82C, REPOSITION_BUTTON_ID, GumpButtonType.Reply, 0 );

            base.SendGump();
        }

        public override void OnResponse( int buttonID, int[] switches, List<(int Key, string Value)> textEntries = null )
        {
            if ( buttonID == REPOSITION_BUTTON_ID )
            {
                Engine.Dispatcher.Invoke( () =>
                {
                    Thread t = new Thread( () =>
                    {
                        SetPosition( GumpX, GumpY );
                        RepositionableGumpViewModel vm = new RepositionableGumpViewModel( this, GumpX, GumpY );
                        RepositionableGumpWindow window = new RepositionableGumpWindow { DataContext = vm };
                        WindowInteropHelper helper = new WindowInteropHelper( window ) { Owner = Engine.WindowHandle };
                        window.ShowInTaskbar = false;
                        window.ShowDialog();
                    } ) { IsBackground = true };

                    t.SetApartmentState( ApartmentState.STA );
                    t.Start();
                } );
            }

            base.OnResponse( buttonID, switches, textEntries );
        }

        public virtual void SetPosition( int x, int y )
        {
            GumpX = x;
            GumpY = y;
        }
    }
}