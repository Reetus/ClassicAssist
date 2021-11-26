using System;
using System.Collections.Generic;
using System.IO;
using Assistant;

namespace ClassicAssist.Data.Macros
{
    internal delegate void dMacroFolderEvent( MacroFolderEventType eventType, string fileName, string newFileName );

    internal enum MacroFolderEventType
    {
        Created,
        Deleted,
        Changed,
        Renamed
    }

    internal class MacroFolderWatcher
    {
        private static FileSystemWatcher _fileSystemWatcher;
        private static readonly Dictionary<string, DateTime> _lastReadTimes = new Dictionary<string, DateTime>();

        public static readonly int DuplicateEventThreshold = 100;
        public static event dMacroFolderEvent MacroFolderEvent;

        public static void Enable()
        {
            string macrosPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Macros" );

            if ( _fileSystemWatcher == null )
            {
                Directory.CreateDirectory( macrosPath );

                _fileSystemWatcher = new FileSystemWatcher( macrosPath, "*.py" );

                _fileSystemWatcher.Changed += OnChanged;
                _fileSystemWatcher.Created += OnCreated;
                _fileSystemWatcher.Deleted += OnDeleted;
                _fileSystemWatcher.Renamed += OnRenamed;
                _fileSystemWatcher.Error += OnError;
            }

            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        public static void Disable()
        {
            if ( _fileSystemWatcher != null )
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
            }
        }

        public static string ReadAllLines( string fileName )
        {
            using ( Stream fileStream =
                new FileStream( fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                using ( StreamReader streamReader = new StreamReader( fileStream ) )
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
        public static void WriteAllText( string fileName, string contents )
        {
            File.WriteAllText( fileName, contents );
        }

        private static bool IsPythonExtension( string fileName )
        {
            return Path.GetExtension( fileName ).ToLower() == ".py";
        }

        private static bool DuplicateEventFilter( string fileName )
        {
            DateTime lastWriteTime = File.GetLastWriteTime( fileName );

            if ( _lastReadTimes.TryGetValue( fileName, out DateTime lastReadTime ) )
            {
                if ( lastWriteTime <= lastReadTime.AddMilliseconds( DuplicateEventThreshold ) )
                {
                    return false;
                }

                _lastReadTimes.Remove( fileName );
            }

            _lastReadTimes.Add( fileName, lastWriteTime );

            return true;
        }

        private static void SendEvent( MacroFolderEventType eventType, string fileName, string newFileName = null )
        {
            Engine.Dispatcher.Invoke( () =>
            {
                MacroFolderEvent?.Invoke( eventType, fileName, newFileName ?? fileName );
            } );
        }

        private static void OnChanged( object sender, FileSystemEventArgs e )
        {
            if ( !DuplicateEventFilter( e.FullPath ) )
            {
                return;
            }

            string macroName = Path.GetFileNameWithoutExtension( e.Name );
            Console.WriteLine(
                $"Changed: {e.FullPath} on {new DateTimeOffset( File.GetLastWriteTime( e.FullPath ) ).ToUnixTimeMilliseconds()} macro {macroName}" );

            SendEvent( MacroFolderEventType.Changed, e.FullPath );
        }

        private static void OnCreated( object sender, FileSystemEventArgs e )
        {
            if ( !DuplicateEventFilter( e.FullPath ) )
            {
                return;
            }

            string macroName = Path.GetFileNameWithoutExtension( e.Name );
            Console.WriteLine( $"Created: {e.FullPath}, Macro: {macroName}" );
            SendEvent( MacroFolderEventType.Created, e.FullPath );
        }

        private static void OnDeleted( object sender, FileSystemEventArgs e )
        {
            string macroName = Path.GetFileNameWithoutExtension( e.Name );
            Console.WriteLine( $"Deleted: {e.FullPath}, Macro: {macroName}" );
            SendEvent( MacroFolderEventType.Deleted, e.FullPath );
        }

        private static void OnRenamed( object sender, RenamedEventArgs e )
        {
            string existingMacroName = Path.GetFileNameWithoutExtension( e.OldName );
            string newMacroName = Path.GetFileNameWithoutExtension( e.Name );

            Console.WriteLine( "Renamed:" );
            Console.WriteLine( $"    Old: {e.OldFullPath}" );
            Console.WriteLine( $"    New: {e.FullPath}" );

            if ( IsPythonExtension( e.Name ) )
            {
                Console.WriteLine( $"    Rename existing macro: {existingMacroName} to {newMacroName}" );
                SendEvent( MacroFolderEventType.Renamed, e.OldFullPath, e.FullPath );
            }
            else
            {
                Console.WriteLine( $"    Delete macro due to rename: {existingMacroName}" );
                SendEvent( MacroFolderEventType.Deleted, e.OldFullPath );
            }
        }

        private static void OnError( object sender, ErrorEventArgs e )
        {
            PrintException( e.GetException() );
        }

        private static void PrintException( Exception ex )
        {
            if ( ex != null )
            {
                Console.WriteLine( $"Message: {ex.Message}" );
                Console.WriteLine( "Stacktrace:" );
                Console.WriteLine( ex.StackTrace );
                Console.WriteLine();
                PrintException( ex.InnerException );
            }
        }
    }
}