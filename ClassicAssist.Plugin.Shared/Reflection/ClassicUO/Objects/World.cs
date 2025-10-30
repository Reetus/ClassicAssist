#region License
// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY
#endregion

using System;
using System.Reflection;
using ClassicAssist.Plugin.Shared.Reflections.Helpers;

namespace ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects
{
    public class World : ReflectionObject
    {
        public World(object sealedObject) : base(sealedObject)
        {
        }
        
        public World() : base( null )
        {
            AssociatedObject = GetWorld();
            CreateMemberCache();
        }

        public Player Player => WrapProperty<Player>();

        private static dynamic GetWorld()
        {
            Type gameType = ReflectionImpl.DefaultAssembly?.GetType( "ClassicUO.Client" );

            object gameInstance = ReflectionHelper.GetTypePropertyValue<object>( gameType, "Game", null, BindingFlags.Static | BindingFlags.Public );

            object gameController = gameInstance.GetType().GetProperty( "UO" )?.GetValue( gameInstance );

            if ( gameController == null )
            {
                throw new InvalidOperationException( "Failed to get GameController.UO" );
            }

            object world = gameController.GetType().GetProperty( "World" )?.GetValue( gameController );

            return world;
        }
    }
}