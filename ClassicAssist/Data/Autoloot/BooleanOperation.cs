#region License
// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY
#endregion

using ClassicAssist.UI.Misc;
using System.ComponentModel;

namespace ClassicAssist.Data.Autoloot
{
    [TypeConverter( typeof( EnumDescriptionTypeConverter ) )]
    public enum BooleanOperation
    {
        [Description( "And (&&)" )]
        And,

        [Description( "Or (||)" )]
        Or,

        [Description( "Not (!)" )]
        Not
    }
}