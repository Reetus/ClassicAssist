using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Assistant;
using ClassicAssist.UI.Misc;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;

namespace ClassicAssist.Data.Macros
{
    public class MacroManager
    {
        private static readonly object _lock = new object();
        private static MacroManager _instance;
        private readonly List<IMacroCommandParser> _parsers = new List<IMacroCommandParser>();

        private MacroManager()
        {
            Engine.PacketReceivedEvent += PacketReceivedEvent;
            Engine.PacketSentEvent += PacketSentEvent;

            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes()
                .Where( t => t.IsClass && typeof( IMacroCommandParser ).IsAssignableFrom( t ) );

            foreach ( Type type in types )
            {
                IMacroCommandParser t = (IMacroCommandParser) Activator.CreateInstance( type );

                _parsers.Add( t );
            }
        }

        public MacroEntry CurrentMacro { get; set; }
        public Action<string> InsertDocument { get; set; }
        public Func<bool> IsRecording { get; set; }
        public ObservableCollectionEx<MacroEntry> Items { get; set; }
        public Action<string, string> NewMacro { get; set; }
        public static bool QuietMode { get; set; }
        public bool Replay { get; set; }

        private void PacketSentEvent( byte[] data, int length )
        {
            if ( !IsRecording() )
            {
                return;
            }

            PacketSentReceived( data, length, PacketDirection.Outgoing );
        }

        private void PacketReceivedEvent( byte[] data, int length )
        {
            if ( !IsRecording() )
            {
                return;
            }

            PacketSentReceived( data, length, PacketDirection.Incoming );
        }

        private void PacketSentReceived( byte[] data, int length, PacketDirection direction )
        {
            foreach ( string result in _parsers.Select( parser => parser.Parse( data, length, direction ) )
                .Where( result => !string.IsNullOrEmpty( result ) ) )
            {
                InsertDocument?.Invoke( result );
                return;
            }
        }

        public static MacroManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new MacroManager();
                    return _instance;
                }
            }

            return _instance;
        }

        public void Execute( MacroEntry macro )
        {
            if ( macro.IsBackground )
            {
                if ( macro.IsRunning )
                {
                    macro.Stop();
                }
                else
                {
                    macro.Execute();
                }
            }
            else
            {
                if ( CurrentMacro != null && CurrentMacro.IsRunning )
                {
                    if ( macro == CurrentMacro && macro.DoNotAutoInterrupt && !Replay )
                    {
                        return;
                    }

                    CurrentMacro.Stop();
                }

                CurrentMacro = macro;
                CurrentMacro.Execute();
            }
        }

        public void StopAll()
        {
            foreach ( MacroEntry entry in Items )
            {
                if ( entry.IsRunning )
                {
                    entry.Stop();
                }
            }
        }

        public void Stop( string name = null )
        {
            if ( string.IsNullOrEmpty( name ) )
            {
                CurrentMacro?.Stop();
            }
            else
            {
                MacroEntry macro = Items.FirstOrDefault( m => m.Name.ToLower().Equals( name.ToLower() ) );
                macro?.Stop();
            }
        }

        public void Autostart()
        {
            foreach ( MacroEntry entry in Items )
            {
                if ( entry.IsAutostart )
                {
                    Execute( entry );
                }
            }
        }

        public MacroEntry GetCurrentMacro()
        {
            Thread currentThread = Thread.CurrentThread;

            return Items?.FirstOrDefault( m => m.MacroInvoker.Thread?.Equals( currentThread ) ?? false );
        }
    }
}