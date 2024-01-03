#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Gumps
{
    public class HotkeyStatusGump : ReflectionRepositionableGump, IExtension
    {
        private static bool _enabled;

        public HotkeyStatusGump()
        {
        }

        public HotkeyStatusGump( int width, int height, int serial, uint gumpID ) : base( width, height, serial,
            gumpID )
        {
            Closable = false;
            Resizable = false;
            Disposable = false;

            GumpX = Options.CurrentOptions.HotkeysStatusGumpX;
            GumpY = Options.CurrentOptions.HotkeysStatusGumpY;

            AddPage( 0 );
            AddImage( 0, 0, _enabled ? 5824 : 5830 );
        }

        public void Initialize()
        {
            _enabled = HotkeyManager.GetInstance().Enabled;
            Engine.PlayerInitializedEvent += OnPlayerInitializedEvent;
            HotkeyManager.HotkeysStatusChanged += OnHotkeysStatusChanged;
        }

        public override void SetPosition( int x, int y )
        {
            base.SetPosition( x, y );

            Options.CurrentOptions.HotkeysStatusGumpX = x;
            Options.CurrentOptions.HotkeysStatusGumpY = y;

            ResendGump();
        }

        public static void ResendGump()
        {
            try
            {
                Commands.CloseClientGump( typeof( HotkeyStatusGump ) );

                if ( !Options.CurrentOptions.HotkeysStatusGump )
                {
                    Commands.CloseClientGump( typeof( HotkeyStatusGump ) );

                    return;
                }

                HotkeyStatusGump gump = new HotkeyStatusGump( 10, 10, 0, 0 );
                gump.SendGump();
            }
            catch ( InvalidOperationException e )
            {
                Console.WriteLine( e.ToString() );
            }
        }

        private static void OnPlayerInitializedEvent( PlayerMobile player )
        {
            ResendGump();
        }

        private static void OnHotkeysStatusChanged( bool enabled )
        {
            _enabled = enabled;

            ResendGump();
        }

        public override void OnClosing()
        {
            base.OnClosing();

            ( int x, int y ) = GetPosition();

            if ( x == default || y == default )
            {
                return;
            }

            Options.CurrentOptions.HotkeysStatusGumpX = x;
            Options.CurrentOptions.HotkeysStatusGumpY = y;
        }
    }
}