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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assistant;
using ClassicAssist.Data.ClassicUO.Objects;
using ClassicAssist.Data.ClassicUO.Objects.Gumps;
using ClassicAssist.Data.Macros;
using ClassicAssist.Shared.Resources;
using Sentry;

namespace ClassicAssist.Data.ClassicUO
{
    public static class Macros
    {
        public static void CreateMacroButton( MacroEntry macroEntry )
        {
            try
            {
                GameScene gameScene = new GameScene();

                IEnumerable<Macro> allMacros = gameScene.Macros.GetAllMacros();

                Macro macroObj = allMacros.FirstOrDefault( e => e.Name == macroEntry.Name );

                if ( macroObj == null )
                {
                    macroObj = new Macro( macroEntry.Name );

                    gameScene.Macros.PushToBack( macroObj );
                }

                macroObj.Items = new MacroObjectString( macroEntry.Name );

                MacroButtonGump macroButton = new MacroButtonGump( macroObj, 200, 200 );

                UIManager.Add( macroButton );
            }
            catch ( Exception e )
            {
                SentrySdk.CaptureException( e );
                UO.Commands.SystemMessage( string.Format( Strings.Reflection_Error___0_, e.Message ) );
            }
        }
    }
}