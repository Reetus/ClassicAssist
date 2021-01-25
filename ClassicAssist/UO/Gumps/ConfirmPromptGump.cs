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
using System.Drawing;
using System.Threading;
using Assistant;
using ClassicAssist.Helpers;
using ClassicAssist.Misc;
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

        public override void OnResponse( int buttonID, int[] switches, Dictionary<int, string> textEntries )
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

        private static Point GetWindowSize()
        {
            if ( !NativeMethods.GetWindowRect( Engine.WindowHandle, out NativeMethods.RECT rect ) )
            {
                return Point.Empty;
            }

            int windowWidth = rect.Right - rect.Left;
            int windowHeight = rect.Bottom - rect.Top;

            return new Point( windowWidth, windowHeight );
        }

        public static Point GetGameWindowCenter()
        {
            dynamic settings = Reflection.GetTypeFieldValue<dynamic>( "ClassicUO.Configuration.Settings",
                "GlobalSettings", null );

            string[] possibleProperties = { "Current", "CurrentProfile" };

            dynamic currentProfile = null;

            foreach ( string possibleProperty in possibleProperties )
            {
                currentProfile = Reflection.GetTypePropertyValue<dynamic>( "ClassicUO.Configuration.ProfileManager",
                    possibleProperty, null );

                if ( currentProfile != null )
                {
                    break;
                }
            }

            if ( currentProfile == null )
            {
                return Point.Empty;
            }

            dynamic gameWindowSize =
                Reflection.GetTypePropertyValue<dynamic>( currentProfile.GetType(), "GameWindowSize", currentProfile );
            dynamic gameWindowPosition = Reflection.GetTypePropertyValue<dynamic>( currentProfile.GetType(),
                "GameWindowPosition", currentProfile );

            if ( gameWindowSize == null || gameWindowPosition == null )
            {
                return Point.Empty;
            }

            return new Point( gameWindowPosition.X + ( gameWindowSize.X >> 1 ),
                gameWindowPosition.Y + ( gameWindowSize.Y >> 1 ) );
        }

        private void SetCenterPosition( int width, int height )
        {
            Point gameCenterPosition = GetGameWindowCenter();

            if ( gameCenterPosition == Point.Empty )
            {
                Point size = GetWindowSize();

                X = ( size.X - ( width  ) ) >> 1;
                Y = ( size.Y - ( height ) ) >> 1;

                return;
            }

            X = gameCenterPosition.X - width / 2;
            Y = gameCenterPosition.Y - height / 2;
        }
    }
}