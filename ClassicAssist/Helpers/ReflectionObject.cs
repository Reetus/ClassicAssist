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
using System.Runtime.CompilerServices;

namespace ClassicAssist.Helpers
{
    /*
     * https://stackoverflow.com/questions/36557845/can-one-convert-a-dynamic-object-to-an-expandoobject-c
     * https://www.meziantou.net/easy-reflection-using-a-dynamicobject.htm
     */
    public class ReflectionObject : DynamicObject
    {
        private const BindingFlags InstanceDefaultBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        protected readonly IDictionary<string, FieldInfo> _fields = new Dictionary<string, FieldInfo>();
        protected readonly IDictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();

        public ReflectionObject( object sealedObject )
        {
            if ( sealedObject is ReflectionObject reflectionObject )
            {
                AssociatedObject = reflectionObject.AssociatedObject;
            }
            else
            {
                AssociatedObject = sealedObject;
            }

            if ( AssociatedObject != null )
            {
                CreateMemberCache();
            }
        }

        public object AssociatedObject { get; set; }

        protected void CreateMemberCache()
        {
            foreach ( PropertyInfo propertyInfo in AssociatedObject.GetType()
                .GetProperties( InstanceDefaultBindingFlags ) )
            {
                _properties.Add( propertyInfo.Name, propertyInfo );
            }

            foreach ( FieldInfo fieldInfo in AssociatedObject.GetType().GetFields( InstanceDefaultBindingFlags ) )
            {
                _fields.Add( fieldInfo.Name, fieldInfo );
            }
        }

        protected T WrapField<T>( Type type = null, [CallerMemberName] string fieldName = null )
        {
            if ( fieldName == null )
            {
                return default;
            }

            object value = _fields[fieldName].GetValue( AssociatedObject );

            if ( value == null )
            {
                return default;
            }

            return (T) Activator.CreateInstance( typeof( T ), value );
        }

        protected T WrapProperty<T>( Type type = null, [CallerMemberName] string fieldName = null )
        {
            if ( fieldName == null )
            {
                return default;
            }

            object value = _properties[fieldName].GetValue( AssociatedObject );

            if ( value == null )
            {
                return default;
            }

            return (T) Activator.CreateInstance( typeof( T ), value );
        }

        private PropertyInfo GetPropertyInfo( string propertyName )
        {
            return AssociatedObject.GetType().GetProperty( propertyName );
        }

        public override bool TryInvokeMember( InvokeMemberBinder binder, object[] args, out object result )
        {
            Type[] types = args.Select( o => o.GetType() ).ToArray();

            MethodInfo method = AssociatedObject.GetType().GetMethod( binder.Name, types );

            result = null;

            if ( method != null )
            {
                result = method.Invoke( AssociatedObject, args );
            }

            return true;
        }

        public object GetProperty( [CallerMemberName] string name = null )
        {
            PropertyInfo prop = GetPropertyInfo( name );

            return prop != null ? prop.GetValue( AssociatedObject ) : null;
        }

        public override bool TryGetMember( GetMemberBinder binder, out object result )
        {
            if ( _fields.ContainsKey( binder.Name ) )
            {
                result = _fields[binder.Name].GetValue( AssociatedObject );
                return true;
            }

            if ( _properties.ContainsKey( binder.Name ) )
            {
                result = _properties[binder.Name].GetValue( AssociatedObject );
                return true;
            }

            result = null;
            return false;
        }

        public override bool TrySetMember( SetMemberBinder binder, object value )
        {
            string name = binder.Name;

            if ( _properties.TryGetValue( name, out PropertyInfo property ) )
            {
                property.SetValue( AssociatedObject, value );
                return true;
            }

            if ( _fields.TryGetValue( name, out FieldInfo field ) )
            {
                field.SetValue( AssociatedObject, value );
                return true;
            }

            return false;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            List<string> list = new List<string>();

            list.AddRange( _properties.Keys.ToList() );
            list.AddRange( _fields.Keys.ToList() );

            return list;
        }
    }
}