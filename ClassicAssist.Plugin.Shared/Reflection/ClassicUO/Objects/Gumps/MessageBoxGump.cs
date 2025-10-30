// Copyright (C) $CURRENT_YEAR$ Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Generic;
using ClassicAssist.Plugin.Shared.Reflections.Helpers;

namespace ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects.Gumps
{
    internal class MessageBoxGump : ReflectionObject
    {
        public MessageBoxGump( object sealedObject ) : base( sealedObject )
        {
        }

        public List<dynamic> Children => WrapProperty<List<dynamic>>();
    }
}