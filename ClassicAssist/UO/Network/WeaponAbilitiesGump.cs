using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Network
{
    public class WeaponAbilitiesGump : Gump
    {
        private readonly bool _primaryEnable;
        private readonly int _primaryId;
        private readonly bool _secondaryEnable;
        private readonly int _secondaryId;

        public WeaponAbilitiesGump( int primaryId, bool primaryEnable, int secondaryId, bool secondaryEnable ) : base(
            Options.CurrentOptions.AbilitiesGumpX, Options.CurrentOptions.AbilitiesGumpY, GumpSerial++,
            (uint) GumpSerial++ )
        {
            if ( Engine.Gumps.GetGumps( out Gump[] gumps ) )
            {
                foreach ( Gump gump in gumps )
                {
                    if ( gump is WeaponAbilitiesGump )
                    {
                        Commands.CloseClientGump( gump.ID );
                    }
                }
            }

            _primaryId = primaryId;
            _secondaryId = secondaryId;
            _primaryEnable = primaryEnable;
            _secondaryEnable = secondaryEnable;

            Movable = false;
            Closable = false;
            AddPage( 0 );
            AddImage( 0, 0, 0x5200 + ( primaryId - 1 ), primaryEnable ? 37 : 0 );
            AddImage( 45, 0, 0x5200 + ( secondaryId - 1 ), secondaryEnable ? 37 : 0 );
            AddButton( 15, 15, 1209, 1210, 1, GumpButtonType.Reply, 0 );
            AddButton( 60, 15, 1209, 1210, 2, GumpButtonType.Reply, 0 );
            AddButton( 81, 0, 0x82C, 0x82C, 4, GumpButtonType.Reply, 0 );

            if ( Editing )
            {
                AddButton( 36, 0, 0x15E0, 0x15E0, 5, GumpButtonType.Reply, 0 );
                AddButton( 0, 16, 0x15E3, 0x15E3, 6, GumpButtonType.Reply, 0 );
                AddButton( 73, 16, 0x15E1, 0x15E1, 7, GumpButtonType.Reply, 0 );
                AddButton( 36, 28, 0x15E2, 0x15E2, 8, GumpButtonType.Reply, 0 );
            }
        }

        public static bool Editing { get; set; }
        public static int GumpSerial { get; set; } = 0x0efe0000;

        public override void OnResponse( int buttonID, int[] switches )
        {
            switch ( buttonID )
            {
                case 1:
                    AbilitiesCommands.SetAbility( "primary" );
                    break;
                case 2:
                    AbilitiesCommands.SetAbility( "secondary" );
                    break;
                case 4:
                {
                    Editing = !Editing;
                    WeaponAbilitiesGump gump =
                        new WeaponAbilitiesGump( _primaryId, _primaryEnable, _secondaryId, _secondaryEnable );
                    gump.SendGump();
                    break;
                }
                case 5:
                {
                    Options.CurrentOptions.AbilitiesGumpY -= 100;

                    if ( Options.CurrentOptions.AbilitiesGumpY < 0 )
                    {
                        Options.CurrentOptions.AbilitiesGumpY = 0;
                    }

                    ResendGump();
                    break;
                }
                case 6:
                {
                    Options.CurrentOptions.AbilitiesGumpX -= 100;

                    if ( Options.CurrentOptions.AbilitiesGumpX < 0 )
                    {
                        Options.CurrentOptions.AbilitiesGumpX = 0;
                    }

                    ResendGump();
                    break;
                }
                case 7:
                {
                    Options.CurrentOptions.AbilitiesGumpX += 100;

                    ResendGump();
                    break;
                }
                case 8:
                {
                    Options.CurrentOptions.AbilitiesGumpY += 100;

                    ResendGump();
                    break;
                }
            }
        }

        private void ResendGump()
        {
            WeaponAbilitiesGump gump =
                new WeaponAbilitiesGump( _primaryId, _primaryEnable, _secondaryId, _secondaryEnable );
            gump.SendGump();
        }
    }
}