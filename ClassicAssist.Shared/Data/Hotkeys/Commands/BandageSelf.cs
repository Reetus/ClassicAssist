using System.Linq;
using ClassicAssist.Shared;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Resources;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.Shared.UO.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Bandage Self" )]
    public class BandageSelf : HotkeyCommand
    {
        private const int TIMEOUT = 5000;
        private readonly int[] _bandageTypes = { 0xe21 };

        public override void Execute()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                UOC.SystemMessage( Strings.Error__No_Player );
                return;
            }

            Item backpack = player.Backpack;

            if ( backpack == null )
            {
                UOC.SystemMessage( Strings.Error__Cannot_find_player_backpack );
                return;
            }

            Item bandage = backpack.Container?.SelectEntity( i => _bandageTypes.Contains( i.ID ) );

            if ( bandage == null )
            {
                UOC.SystemMessage( Strings.Error__Cannot_find_type );
                return;
            }

            ObjectCommands.UseObject( bandage.Serial );
            bool result = UOC.WaitForTarget( TIMEOUT );

            if ( result )
            {
                Engine.SendPacketToServer( new Target( -1, player, true ) );
            }
        }
    }
}