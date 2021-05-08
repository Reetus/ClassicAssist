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

using ClassicAssist.Helpers;

namespace ClassicAssist.Data.ClassicUO.Objects
{
    public class Macro : LinkedObject<Macro, MacroObjectString>
    {
        private const string MACRO_TYPE = "ClassicUO.Game.Managers.Macro";

        public Macro( string name ) : base( null )
        {
            AssociatedObject = Reflection.CreateInstanceOfType( MACRO_TYPE, null, name );
            CreateMemberCache();
        }

        // Don't remove
        public Macro( object obj ) : base( obj )
        {
        }

        public string Name => (string) AssociatedObject.GetType().GetProperty( "Name" )?.GetValue( AssociatedObject );
    }
}