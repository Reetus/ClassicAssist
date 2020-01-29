using System;
using ClassicAssist.Data;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO
{
    public sealed class UpdateMessageGump : Gump
    {
        private readonly Version _version;

        public UpdateMessageGump( string message, Version version ) : base( 500, 250, -1 )
        {
            _version = version;
            Closable = true;
            Disposable = false;

            AddPage( 0 );
            AddBackground( 0, 0, 500, 200, 9270 );
            AddHtml( 20, 20, 460, 130, message, true, true );
            AddButton( 420, 160, 247, 248, 0, GumpButtonType.Reply, 0 );
        }

        public override void OnResponse( int buttonID, int[] switches )
        {
            Options.CurrentOptions.UpdateGumpVersion = _version;
            base.OnResponse( buttonID, switches );
        }
    }
}