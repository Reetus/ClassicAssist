#region License

// Copyright (C) 2021 Reetus
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
using System.Reflection;
using System.Threading;
using Assistant;
using ClassicAssist.UO.Network.Packets;

namespace ClassicAssist.Data.ClassicUO.Objects
{
    public static class Pathfinder
    {
        private const string TYPE = "ClassicUO.Game.Pathfinder";
        private static Type _type;
        private static MethodInfo _walkMethod;

        public static bool AutoWalking
        {
            get
            {
#if NET
                throw new PlatformNotSupportedException();
#endif

                if ( _type == null )
                {
                    _type = Engine.ClassicAssembly?.GetType( TYPE );
                }

                if ( _type == null )
                {
                    return false;
                }

                PropertyInfo property = _type.GetProperty( "AutoWalking" );

                if ( property == null )
                {
                    return false;
                }

                return (bool) property.GetValue( null );
            }
        }

        public static bool Cancel()
        {
#if NET
            throw new PlatformNotSupportedException();
#endif

            if ( _type == null )
            {
                _type = Engine.ClassicAssembly?.GetType( TYPE );
            }

            if ( _type == null )
            {
                return false;
            }

            PropertyInfo property = _type.GetProperty( "AutoWalking" );

            if ( property == null )
            {
                return false;
            }

            property.SetValue( null, false );

            return !AutoWalking;
        }

        public static bool WalkTo( int x, int y, int z, int distance )
        {
            try
            {
#if NET
                throw new PlatformNotSupportedException();
#endif

                if ( _type == null )
                {
                    _type = Engine.ClassicAssembly?.GetType( TYPE );
                }

                if ( _type == null )
                {
                    throw new Exception( "Cannot find type" );
                }

                if ( _walkMethod == null )
                {
                    _walkMethod = _type?.GetMethod( "WalkTo", BindingFlags.Public | BindingFlags.Static );
                }

                if ( _walkMethod == null )
                {
                    throw new Exception( "Cannot find method" );
                }

                AutoResetEvent are = new AutoResetEvent( false );

                bool retval = false;

                Engine.TickWorkQueue.Enqueue( () =>
                {
                    retval = (bool) _walkMethod.Invoke( null, new object[] { x, y, z, distance } );
                    are.Set();
                } );

                are.WaitOne( 5000 );

                return retval;
            }
            catch
            {
                // Fallback to old method
                Engine.SendPacketToClient( new Pathfind( x, y, z ) );
                return true;
            }
        }
    }
}