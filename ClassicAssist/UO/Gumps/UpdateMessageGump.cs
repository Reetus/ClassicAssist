using System;
using System.Collections.Generic;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Gumps
{
    public sealed class UpdateMessageGump : Gump
    {
        private readonly string _version;

        public UpdateMessageGump( IntPtr hWnd, string message, string version ) : base( 500, 250, -1 )
        {
            if ( NativeMethods.GetWindowRect( hWnd, out NativeMethods.RECT rect ) )
            {
                int windowWidth = rect.Right - rect.Left;
                int windowHeight = rect.Bottom - rect.Top;

                X = ( windowWidth - 600 ) / 2;
                Y = ( windowHeight - 600 ) / 2;
            }

            _version = version;
            Closable = true;
            Disposable = false;

            AddPage( 0 );
            AddBackground( 0, 0, 500, 600, 9270 );
            AddHtml( 20, 20, 460, 530, message, true, true );
            AddButton( 420, 560, 247, 248, 0, GumpButtonType.Reply, 0 );
            AddButton( 350, 560, 0xEF, 0xF0, 1, GumpButtonType.Reply, 0 );
        }

        public override void OnResponse( int buttonID, int[] switches,
            List<(int Key, string Value)> textEntries = null )
        {
            AssistantOptions.UpdateGumpVersion = _version;

            if ( buttonID == 1 )
            {
                Engine.LaunchUpdater();
            }

            base.OnResponse( buttonID, switches, textEntries );
        }
    }
}