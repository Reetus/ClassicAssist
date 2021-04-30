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
using System.Reflection;
using Assistant;

namespace ClassicAssist.Helpers
{
    public static class Reflection
    {
        public static T GetTypePropertyValue<T>( Type type, string property, object obj = null,
            BindingFlags bindingFlags = BindingFlags.Default )
        {
            PropertyInfo propertyInfo = type.GetProperty( property );

            T val = (T) propertyInfo?.GetValue( obj, null );

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if ( val == null )
            {
                return default;
            }

            return val;
        }

        public static T GetTypePropertyValue<T>( string type, string property, object obj,
            BindingFlags bindingFlags = BindingFlags.Default, Assembly assembly = null )
        {
            if ( assembly == null )
            {
                assembly = Engine.ClassicAssembly;
            }

            Type t = assembly?.GetType( type );

            return t == null ? default : GetTypePropertyValue<T>( t, property, obj, bindingFlags );
        }

        public static T GetTypeFieldValue<T>( Type type, string property, object obj = null,
            BindingFlags bindingFlags = BindingFlags.Default )
        {
            FieldInfo fieldInfo = type.GetField( property );

            T val = (T) fieldInfo?.GetValue( obj );

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if ( val == null )
            {
                return default;
            }

            return val;
        }

        public static T GetTypeFieldValue<T>( string type, string property, object obj,
            BindingFlags bindingFlags = BindingFlags.Default, Assembly assembly = null )
        {
            if ( assembly == null )
            {
                assembly = Engine.ClassicAssembly;
            }

            Type t = assembly?.GetType( type );

            return t == null ? default : GetTypeFieldValue<T>( t, property, obj, bindingFlags );
        }

        public static MethodInfo GetTypeMethod( Type type, string methodName, object obj = null )
        {
            return type.GetMethod( methodName );
        }

        public static object CreateInstanceOfType( string type, Assembly assembly = null )
        {
            if ( assembly == null )
            {
                assembly = Engine.ClassicAssembly;
            }

            Type t = assembly?.GetType( type );

            return Activator.CreateInstance( t );
        }

        /*
         * https://stackoverflow.com/questions/6961781/reflecting-a-private-field-from-a-base-class
         */
        public static T GetTypeFieldValueRecurse<T>( Type t, string name, object obj )
        {
            FieldInfo fi;

            while ( ( fi = t.GetField( name,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly ) ) == null &&
                    ( t = t.BaseType ) != null )
            {
            }

            if ( fi != null )
            {
                return (T) fi.GetValue( obj );
            }

            return default;
        }

        public static void SetTypeFieldValueRecurse( Type t, string name, object obj, object val )
        {
            FieldInfo fi;

            while ( ( fi = t.GetField( name,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly ) ) == null &&
                    ( t = t.BaseType ) != null )
            {
            }

            if ( fi != null )
            {
                fi.SetValue( obj, val );
            }
        }
    }
}