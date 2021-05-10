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
using ClassicAssist.Data.ClassicUO.Objects.Gumps;

namespace ClassicAssist.Data.ClassicUO.Objects
{
    public static class UIManager
    {
        private const string UI_MANAGER_TYPE = "ClassicUO.Game.Managers.UIManager";
        private static Type _uiManagerType;
        private static MethodInfo _addMethod;

        public static void Add( MacroButtonGump button )
        {
            if ( _uiManagerType == null )
            {
                _uiManagerType = Engine.ClassicAssembly?.GetType( UI_MANAGER_TYPE );
            }

            if ( _addMethod == null )
            {
                _addMethod = _uiManagerType?.GetMethod( "Add", BindingFlags.Public | BindingFlags.Static );
            }

            Engine.TickWorkQueue.Enqueue( () => { _addMethod?.Invoke( null, new[] { button.AssociatedObject } ); } );
        }
    }
}