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
using ClassicAssist.Helpers;

namespace ClassicAssist.Data.ClassicUO.Objects
{
    public class GameScene : ReflectionObject
    {
        public GameScene() : base( null )
        {
            AssociatedObject = GetGameScene();
            CreateMemberCache();
        }

        public MacroManager Macros => WrapProperty<MacroManager>();

        private static dynamic GetGameScene()
        {
            dynamic game = Reflection.GetTypePropertyValue<dynamic>( "ClassicUO.Client", "Game", null );

            Type gameType = game.GetType();

            MethodInfo getSceneMethod = gameType.GetMethod( "GetScene", BindingFlags.Public | BindingFlags.Instance );

            Type gameSceneType = Engine.ClassicAssembly.GetType( "ClassicUO.Game.Scenes.GameScene" );

            MethodInfo method = getSceneMethod?.MakeGenericMethod( gameSceneType );

            return method?.Invoke( game, null );
        }
    }
}