using System.Collections.Generic;
using System.Linq;
using ClassicAssist.Shared.Resources;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class ListCommands
    {
        private static readonly Dictionary<string, List<object>> _lists = new Dictionary<string, List<object>>();

        [CommandsDisplay( Category = nameof( Strings.Lists ), Parameters = new[] { nameof( ParameterType.ListName ) } )]
        public static void CreateList( string listName )
        {
            if ( ListExists( listName ) )
            {
                RemoveList( listName );
            }

            _lists.Add( listName, new List<object>() );
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ), Parameters = new[] { nameof( ParameterType.ListName ) } )]
        public static bool ListExists( string listName )
        {
            return _lists.ContainsKey( listName );
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ), Parameters = new[] { nameof( ParameterType.ListName ) } )]
        public static int List( string listName )
        {
            return ListExists( listName ) ? _lists[listName].Count : 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ),
            Parameters = new[] { nameof( ParameterType.ListName ), nameof( ParameterType.IntegerValue ) } )]
        public static void PushList( string listName, object value )
        {
            if ( !ListExists( listName ) )
            {
                CreateList( listName );
            }

            _lists[listName].Add( value );
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ),
            Parameters = new[] { nameof( ParameterType.ListName ), nameof( ParameterType.ElementValueFrontBack ) } )]
        public static int PopList( string listName, object elementValue = null)
        {
            elementValue = elementValue ?? "back";

            if ( !ListExists( listName ) )
            {
                CreateList( listName );
            }

            List<object> list = _lists[listName];

            if ( list.Count == 0 )
            {
                return 0;
            }

            switch ( elementValue.ToString().ToLower() )
            {
                case "front":
                {
                    list.RemoveAt( 0 );
                    return 1;
                }
                case "back":
                {
                    list.RemoveAt( _lists[listName].Count - 1 );

                    return 1;
                }
                default:
                {
                    return list.RemoveAll( listItem => elementValue.Equals(listItem) );
                }
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ), Parameters = new[] { nameof( ParameterType.ListName ) } )]
        public static object[] GetList( string listName )
        {
            return _lists[listName].ToArray();
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ), Parameters = new[] { nameof( ParameterType.ListName ) } )]
        public static void RemoveList( string listName )
        {
            _lists.Remove( listName );
        }

        internal static Dictionary<string, List<object>> GetAllLists()
        {
            return _lists;
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ), Parameters = new[] { nameof( ParameterType.ListName ) } )]
        public static void ClearList( string listName )
        {
            if ( !_lists.ContainsKey( listName ) )
            {
                return;
            }

            _lists[listName].Clear();
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ),
            Parameters = new[] { nameof( ParameterType.ListName ), nameof( ParameterType.IntegerValue ) } )]
        public static bool InList( string listName, object value )
        {
            object[] list;

            return ( list = GetList( listName ) ) != null && list.Contains( value );
        }
    }
}