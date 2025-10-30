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

namespace ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects
{
    public static class Pathfinder
    {
        private static object _pathfinderInstance;
        private const string TYPE = "ClassicUO.Game.Pathfinder";
        private static Type _type;
        private static MethodInfo _walkMethod;

        public static bool AutoWalking
        {
            get
            {
                if ( _type == null )
                {
                    _type = ReflectionImpl.DefaultAssembly?.GetType( TYPE );
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

                return (bool) property.GetValue( _pathfinderInstance );
            }
        }

        public static bool Cancel()
        {
            if ( _type == null )
            {
                _type = ReflectionImpl.DefaultAssembly?.GetType( TYPE );
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

            property.SetValue( _pathfinderInstance, false );

            return !AutoWalking;
        }

        public static bool WalkTo( int x, int y, int z, int distance )
        {
            try
            {
                if ( _type == null )
                {
                    _type = ReflectionImpl.DefaultAssembly?.GetType( TYPE );
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
                    _walkMethod = _type?.GetMethod( "WalkTo", BindingFlags.Instance | BindingFlags.Public );

                    if ( _walkMethod != null )
                    {
                        World world = new World();
                        PropertyInfo property;

                        if ( (property = world.Player.AssociatedObject.GetType().GetProperty( "Pathfinder" )) != null )
                        {
                            object pathfinder = property.GetValue( world.Player.AssociatedObject );

                            _pathfinderInstance = pathfinder ?? throw new InvalidOperationException( "Failed to get Pathfinder" );
                        }
                    }
                }

                if ( _walkMethod == null )
                {
                    throw new Exception( "Cannot find method" );
                }

                AutoResetEvent are = new AutoResetEvent( false );

                bool retval = false;

                ReflectionImpl.TickWorkQueue.Enqueue( () =>
                {
                    retval = (bool) _walkMethod.Invoke( _pathfinderInstance, new object[] { x, y, z, distance } );
                    are.Set();
                } );

                are.WaitOne( 5000 );

                return retval;
            }
            catch
            {
                // TODO
                return true;
                // // Fallback to old method
                // Engine.SendPacketToClient( new Pathfind( x, y, z ) );
                // return true;
            }
        }
    }
}