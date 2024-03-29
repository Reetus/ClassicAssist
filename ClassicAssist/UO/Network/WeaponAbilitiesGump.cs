﻿using System.Collections.Generic;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Gumps;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.UO.Network
{
    public class WeaponAbilitiesGump : ReflectionRepositionableGump
    {
        private readonly bool _primaryEnable;
        private readonly int _primaryId;
        private readonly bool _secondaryEnable;
        private readonly int _secondaryId;

        public WeaponAbilitiesGump( int primaryId, bool primaryEnable, int secondaryId, bool secondaryEnable ) : base(
            90, 40, GumpSerial++, (uint) GumpSerial++ )
        {
            Commands.CloseClientGump( typeof( WeaponAbilitiesGump ) );

            GumpX = Options.CurrentOptions.AbilitiesGumpX;
            GumpY = Options.CurrentOptions.AbilitiesGumpY;

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
        }

        public static int GumpSerial { get; set; } = 0x0fff0000;

        public override void OnResponse( int buttonID, int[] switches,
            List<(int Key, string Value)> textEntries = null )
        {
            switch ( buttonID )
            {
                case 1:
                    AbilitiesCommands.SetAbility( "primary" );
                    break;
                case 2:
                    AbilitiesCommands.SetAbility( "secondary" );
                    break;
            }

            base.OnResponse( buttonID, switches, textEntries );
        }

        public override void OnClosing()
        {
            base.OnClosing();

            ( int x, int y ) = GetPosition();

            if ( x == default || y == default )
            {
                return;
            }

            Options.CurrentOptions.AbilitiesGumpX = x;
            Options.CurrentOptions.AbilitiesGumpY = y;
        }

        private void ResendGump()
        {
            Commands.CloseClientGump( typeof( WeaponAbilitiesGump ) );

            WeaponAbilitiesGump gump =
                new WeaponAbilitiesGump( _primaryId, _primaryEnable, _secondaryId, _secondaryEnable );
            gump.SendGump();
        }

        public override void SetPosition( int x, int y )
        {
            base.SetPosition( x, y );

            Options.CurrentOptions.AbilitiesGumpX = x;
            Options.CurrentOptions.AbilitiesGumpY = y;

            ResendGump();
        }
    }
}