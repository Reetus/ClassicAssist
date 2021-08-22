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
using Assistant;

namespace ClassicAssist.Data.ClassicUO.Objects
{
    public static class Pathfinder
    {
        private const string TYPE = "ClassicUO.Game.Pathfinder";
        private static Type _type;

        public static bool AutoWalking
        {
            get
            {
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
    }
}