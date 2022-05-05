#region License

// Copyright (C) 2022 Reetus
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

namespace ClassicAssist.Data.Hotkeys
{
    public class HotkeyConfigurationAttribute : Attribute
    {
        public Type BaseType { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
    }
}