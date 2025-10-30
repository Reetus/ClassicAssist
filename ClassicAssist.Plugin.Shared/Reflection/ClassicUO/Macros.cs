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
using ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects;
using ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects.Gumps;

namespace ClassicAssist.Plugin.Shared.Reflection.ClassicUO
{
    public static class Macros
    {
        public static void PlayCUOMacro( string name )
        {
            GameScene gameScene = new GameScene();

            IEnumerable<Macro> allMacros = gameScene.Macros.GetAllMacros();

            Macro selectedMacro = allMacros.FirstOrDefault( m => m.Name == name );

            if ( selectedMacro == null )
            {
                return;
            }

            gameScene.Macros.PlayMacro( selectedMacro );
        }

        public static void CreateMacroButton( string name )
        {
            try
            {
                GameScene gameScene = new GameScene();

                IEnumerable<Macro> allMacros = gameScene.Macros.GetAllMacros();

                Macro macroObj = allMacros.FirstOrDefault( e => e.Name == name );

                if ( macroObj == null )
                {
                    macroObj = new Macro( name );

                    gameScene.Macros.PushToBack( macroObj );
                }

                macroObj.Items = new MacroObjectString( name );

                MacroButtonGump macroButton = new MacroButtonGump( macroObj, 200, 200 );

                UIManager.Add( macroButton );
            }
            catch ( Exception e )
            {
                // TODO
            }
        }

        public static void CreateMacroButton( string name, string contents )
        {
            try
            {
                GameScene gameScene = new GameScene();

                IEnumerable<Macro> allMacros = gameScene.Macros.GetAllMacros();

                Macro macroObj = allMacros.FirstOrDefault( e => e.Name == name );

                if ( macroObj == null )
                {
                    macroObj = new Macro( name );

                    gameScene.Macros.PushToBack( macroObj );
                }

                string macroText = contents;

                macroObj.Items = new MacroObjectString( macroText );

                MacroButtonGump macroButton = new MacroButtonGump( macroObj, 200, 200 );

                UIManager.Add( macroButton );
            }
            catch ( Exception e )
            {
                // TODO
            }
        }
    }
}