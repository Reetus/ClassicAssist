#region License

// Copyright (C) 2020 Reetus
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
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace ClassicAssist.Helpers
{
    /*
     * https://stackoverflow.com/questions/36557845/can-one-convert-a-dynamic-object-to-an-expandoobject-c
     * https://www.meziantou.net/easy-reflection-using-a-dynamicobject.htm
     */
    public class ReflectionObject : DynamicObject
    {
        private readonly object _currentObject;
        private readonly Dictionary<string, object> _customProperties = new Dictionary<string, object>();

        public ReflectionObject( dynamic sealedObject )
        {
            _currentObject = sealedObject;
        }

        private PropertyInfo GetPropertyInfo( string propertyName )
        {
            return _currentObject.GetType().GetProperty( propertyName );
        }

        public override bool TryInvokeMember( InvokeMemberBinder binder, object[] args, out object result )
        {
            Type[] types = args.Select( o => o.GetType() ).ToArray();

            MethodInfo method = _currentObject.GetType().GetMethod( binder.Name, types );

            result = null;

            if ( method != null )
            {
                result = method.Invoke( _currentObject, args );
            }

            return true;
        }

        public override bool TryGetMember( GetMemberBinder binder, out object result )
        {
            object val =
                Reflection.GetTypeFieldValueRecurse<object>( _currentObject.GetType(), binder.Name, _currentObject );

            if ( val != null )
            {
                result = val;
                return true;
            }

            PropertyInfo prop = GetPropertyInfo( binder.Name );

            if ( prop != null )
            {
                result = prop.GetValue( _currentObject );
                return true;
            }

            result = _customProperties[binder.Name];
            return true;
        }

        public override bool TrySetMember( SetMemberBinder binder, object value )
        {
            PropertyInfo prop = GetPropertyInfo( binder.Name );

            if ( prop != null )
            {
                prop.SetValue( _currentObject, value );
                return true;
            }

            if ( _customProperties.ContainsKey( binder.Name ) )
            {
                _customProperties[binder.Name] = value;
            }
            else
            {
                _customProperties.Add( binder.Name, value );
            }

            return true;
        }
    }
}