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

using System.Reflection;
using ClassicAssist.Plugin.Shared.Reflections.Helpers;

namespace ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects
{
    public class Game : ReflectionObject
    {
        public Game() : base(null)
        {
            AssociatedObject = GetGame();
            CreateMemberCache();
        }

        public UO UO => WrapProperty<UO>();

        private dynamic GetGame()
        {
            return ReflectionHelper.GetTypePropertyValue<object>( "ClassicUO.Client", "Game", null, BindingFlags.Static | BindingFlags.Public );
        }
    }
}