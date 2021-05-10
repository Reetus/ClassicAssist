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
using Assistant;
using ClassicAssist.Helpers;

namespace ClassicAssist.Data.ClassicUO.Objects
{
    public class MacroObjectString : LinkedObject<object, object>
    {
        private const string MACRO_TYPE_TYPE = "ClassicUO.Game.Managers.MacroType";
        private const string MACRO_SUB_TYPE_TYPE = "ClassicUO.Game.Managers.MacroSubType";
        private const string MACRO_OBJ_STRING_TYPE = "ClassicUO.Game.Managers.MacroObjectString";

        public MacroObjectString( string text ) : base( null )
        {
            Type macroTypeType = Engine.ClassicAssembly.GetType( MACRO_TYPE_TYPE );
            Type macroSubTypeType = Engine.ClassicAssembly.GetType( MACRO_SUB_TYPE_TYPE );

            object macroType = Enum.Parse( macroTypeType, "RazorMacro" );
            object macroSubType = Enum.Parse( macroSubTypeType, "MSC_NONE" );

            object macroObjStr = Reflection.CreateInstanceOfType( MACRO_OBJ_STRING_TYPE, null, macroType,
                macroSubType, text );

            AssociatedObject = macroObjStr;
            CreateMemberCache();
        }
    }
}