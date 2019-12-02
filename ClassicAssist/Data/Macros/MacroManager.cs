using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assistant;
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

        public Action<string> InsertDocument { get; set; }
        public Func<bool> IsRecording { get; set; }

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
            foreach ( string result in _parsers
                .Select( parser => parser.Parse( data, length, direction ) )
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
    }
}