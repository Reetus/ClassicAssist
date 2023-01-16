#region License

// Copyright (C) 2023 Reetus
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
using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Helpers;
using ClassicAssist.UO.Objects.Gumps;
using CUOGump = ClassicAssist.Data.ClassicUO.Objects.Gumps.Gump;
using CUOGumps = ClassicAssist.Data.ClassicUO.Gumps;

namespace ClassicAssist.UO.Gumps
{
    public class ReflectionRepositionableGump : RepositionableGump
    {
        private readonly bool _canReflection;
        private readonly object _sendLock = new object();

        static ReflectionRepositionableGump()
        {
            Engine.DisconnectedEvent += CloseGumps;
            Engine.ClientClosing += CloseGumps;
        }

        public ReflectionRepositionableGump( int width, int height, int serial, uint gumpID ) : base( width, height,
            serial, gumpID )
        {
            _canReflection = SetCanReflection();
        }

        private static void CloseGumps()
        {
            if ( !Engine.Gumps.GetGumps( out Gump[] gumps ) )
            {
                return;
            }

            foreach ( Gump gump in gumps.Where(
                         g => g.GetType().IsSubclassOf( typeof( ReflectionRepositionableGump ) ) ) )
            {
                gump.OnClosing();
            }
        }

        public override void SendGump()
        {
            lock ( _sendLock )
            {
                Commands.CloseClientGump( GetType() );

                if ( !_canReflection )
                {
                    base.SendGump();
                }
                else
                {
                    X = GumpX;
                    Y = GumpY;

                    Movable = true;

                    byte[] bytes = Compile();

                    Engine.Gumps.Add( this );
                    Engine.SendPacketToClient( bytes, bytes.Length );
                }
            }
        }

        private static bool SetCanReflection()
        {
            try
            {
                dynamic gumps = Reflection.GetTypePropertyValue<dynamic>( "ClassicUO.Game.Managers.UIManager", "Gumps",
                    null );

                return gumps != null;
            }
            catch ( Exception )
            {
                return false;
            }
        }

        protected (int, int) GetPosition()
        {
            int x = 0;
            int y = 0;

            if ( !_canReflection )
            {
                return ( x, y );
            }

            try
            {
                IEnumerable<dynamic> gumps = CUOGumps.GetGumps();

                List<CUOGump> cuoGumps = ( from g in gumps select new CUOGump( g ) ).ToList();

                CUOGump gump = cuoGumps.FirstOrDefault( g => g.ServerSerial == ID );

                if ( gump != null )
                {
                    x = gump.Location.X;
                    y = gump.Location.Y;
                }
            }
            catch ( Exception )
            {
                return ( x, y );
            }

            return ( x, y );
        }
    }
}