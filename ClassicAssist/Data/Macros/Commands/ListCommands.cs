using System.Collections.Generic;
using System.Linq;
using ClassicAssist.Resources;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class ListCommands
    {
        private static readonly Dictionary<string, List<int>> _lists = new Dictionary<string, List<int>>();

        [CommandsDisplay( Category = nameof( Strings.Lists ) )]
        public static void CreateList( string listName )
        {
            if ( ListExists( listName ) )
            {
                RemoveList( listName );
            }

            _lists.Add( listName, new List<int>() );
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ) )]
        public static bool ListExists( string listName )
        {
            return _lists.ContainsKey( listName );
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ) )]
        public static int List( string listName )
        {
            return ListExists( listName ) ? _lists[listName].Count : 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ) )]
        public static void PushList( string listName, int value )
        {
            if ( !ListExists( listName ) )
            {
                CreateList( listName );
            }

            _lists[listName].Add( value );
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ) )]
        public static int[] GetList( string listName )
        {
            return _lists[listName].ToArray();
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ) )]
        public static void RemoveList( string listName )
        {
            _lists.Remove( listName );
        }

        internal static Dictionary<string, List<int>> GetAllLists()
        {
            return _lists;
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ) )]
        public static void ClearList( string listName )
        {
            if ( !_lists.ContainsKey( listName ) )
            {
                return;
            }

            _lists[listName].Clear();
        }

        [CommandsDisplay( Category = nameof( Strings.Lists ) )]
        public static bool InList( string listName, int value )
        {
            int[] list;

            return ( list = GetList( listName ) ) != null && list.Contains( value );
        }
    }
}