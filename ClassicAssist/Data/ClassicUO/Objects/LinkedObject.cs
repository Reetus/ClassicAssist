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
using System.Runtime.CompilerServices;
using ClassicAssist.Helpers;

namespace ClassicAssist.Data.ClassicUO.Objects
{
    public class LinkedObject<T, T2> : ReflectionObject
    {
        public LinkedObject( object obj ) : base(obj)
        {
        }

        public virtual T2 Items
        {
            get => WrapField<T2>();
            set
            {
                if ( value is ReflectionObject reflectionObject )
                {
                    _fields["Items"].SetValue( AssociatedObject, reflectionObject.AssociatedObject );
                    return;
                }

                _fields["Items"].SetValue( AssociatedObject, value );
            }
        }

        public virtual T Next => WrapField<T>();

        public virtual T Previous => WrapField<T>();
    }
}