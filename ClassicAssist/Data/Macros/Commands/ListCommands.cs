using System.Collections.Generic;
using System.Linq;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class ListCommands
    {
        private static readonly Dictionary<string, List<int>> _lists = new Dictionary<string, List<int>>();

        [CommandsDisplay( Category = "Lists",
            Description = "Create list with given name, if list already exists, it is overwritten.",
            InsertText = "CreateList(\"list\")" )]
        public static void CreateList( string listName )
        {
            if ( ListExists( listName ) )
            {
                RemoveList( listName );
            }

            _lists.Add( listName, new List<int>() );
        }

        [CommandsDisplay( Category = "Lists", Description = "Returns true if list exist, or false if not.",
            InsertText = "if ListExists(\"list\"):" )]
        public static bool ListExists( string listName )
        {
            return _lists.ContainsKey( listName );
        }

        [CommandsDisplay( Category = "Lists", Description = "Returns the number of entries in the list.",
            InsertText = "if List(\"list\") < 5:" )]
        public static int List( string listName )
        {
            return ListExists( listName ) ? _lists[listName].Count : 0;
        }

        [CommandsDisplay( Category = "Lists",
            Description = "Pushes a value to the end of the list, will create list if it doesn't exist.",
            InsertText = "PushList(\"list\", 1)" )]
        public static void PushList( string listName, int value )
        {
            if ( !ListExists( listName ) )
            {
                CreateList( listName );
            }

            _lists[listName].Add( value );
        }

        [CommandsDisplay( Category = "Lists",
            Description = "Returns array of all entries in the list, for use with for loop etc.",
            InsertText = "GetList(\"list\")" )]
        public static int[] GetList( string listName )
        {
            return _lists[listName].ToArray();
        }

        [CommandsDisplay( Category = "Lists", Description = "Removes the list with the given name.",
            InsertText = "RemoveList(\"list\")" )]
        public static void RemoveList( string listName )
        {
            _lists.Remove( listName );
        }

        internal static Dictionary<string, List<int>> GetAllLists()
        {
            return _lists;
        }

        [CommandsDisplay( Category = "Lists", Description = "Clear a list by name.",
            InsertText = "ClearList(\"list\")" )]
        public static void ClearList( string listName )
        {
            if ( !_lists.ContainsKey( listName ) )
            {
                return;
            }

            _lists[listName].Clear();
        }

        [CommandsDisplay( Category = "Lists", Description = "Checks whether a list contains a given element.",
            InsertText = "if InList(\"shmoo\", 1):" )]
        public static bool InList( string listName, int value )
        {
            int[] list;

            return ( list = GetList( listName ) ) != null && list.Contains( value );
        }
    }
}