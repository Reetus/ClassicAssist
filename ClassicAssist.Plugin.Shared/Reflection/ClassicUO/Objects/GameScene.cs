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
using ClassicAssist.Plugin.Shared.Reflections.Helpers;

namespace ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects
{
    public class GameScene : ReflectionObject
    {
        public GameScene() : base( null )
        {
            AssociatedObject = GetGameScene();
            CreateMemberCache();
        }

        public MacroManager Macros => GetMacroManager();

        private MacroManager GetMacroManager()
        {
            if ( _properties.ContainsKey( nameof( Macros ) ) )
            {
                return WrapProperty<MacroManager>( null, nameof( Macros ) );
            }
            
            Type gameType = ReflectionImpl.DefaultAssembly?.GetType( "ClassicUO.Client" );

            object gameInstance = ReflectionHelper.GetTypePropertyValue<object>( gameType, "Game", null, BindingFlags.Static | BindingFlags.Public );

            object gameController = gameInstance.GetType().GetProperty( "UO" )?.GetValue( gameInstance );

            if ( gameController == null )
            {
                throw new InvalidOperationException( "Failed to get GameController.UO" );
            }

            object world = gameController.GetType().GetProperty( "World" )?.GetValue( gameController );

            if ( world != null )
            {
                var macros = world.GetType().GetProperty( "Macros" ).GetValue( world );
                
                return (MacroManager) Activator.CreateInstance( typeof( MacroManager ), macros );
            }

            return null;
        }

        private static dynamic GetGameScene()
        {
            dynamic game = ReflectionHelper.GetTypePropertyValue<dynamic>( "ClassicUO.Client", "Game", null );

            Type gameType = game.GetType();

            MethodInfo getSceneMethod = gameType.GetMethod( "GetScene", BindingFlags.Public | BindingFlags.Instance );

            Type gameSceneType = ReflectionImpl.DefaultAssembly.GetType( "ClassicUO.Game.Scenes.GameScene" );

            MethodInfo method = getSceneMethod?.MakeGenericMethod( gameSceneType );

            return method?.Invoke( game, null );
        }
    }
}