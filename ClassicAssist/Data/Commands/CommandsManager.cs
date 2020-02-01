using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

// ReSharper disable UnusedVariable

namespace ClassicAssist.Data.Commands
{
    public class CommandsManager
    {
        private static Dictionary<string, Func<string[], bool>> _commands;

        private static readonly byte[] _speechPacketIDs = { 0xAD, 0x03 };

        public static void Initialize()
        {
            _commands = new Dictionary<string, Func<string[], bool>>
            {
                { "help", OnHelp },
                { "where", OnSendLocation },
                { "addfriend", OnAddFriend },
                { "removefriend", OnRemoveFriend }
            };
        }

        private static bool OnRemoveFriend( string[] args )
        {
            if ( args.Length != 0 )
            {
                return false;
            }

            Task.Run( () => MobileCommands.RemoveFriend() );

            return true;
        }

        private static bool OnAddFriend( string[] args )
        {
            if ( args.Length != 0 )
            {
                return false;
            }

            Task.Run( () => MobileCommands.AddFriend() );

            return true;
        }

        private static bool OnHelp( string[] args )
        {
            if ( args.Length != 0 )
            {
                return false;
            }

            string[] commands = _commands.Select( kvp => kvp.Key ).ToArray();

            UOC.SystemMessage( $"{Strings.Commands_} {string.Join( " ", commands )}" );

            return true;
        }

        public static bool IsSpeechPacket( byte packetId )
        {
            return _speechPacketIDs?.Contains( packetId ) ?? false;
        }

        private static bool OnSendLocation( string[] args )
        {
            if ( Engine.Player == null )
            {
                return false;
            }

            PlayerMobile player = Engine.Player;

            if ( args?.Length != 0 )
            {
                return false;
            }

            UOC.SystemMessage( $"{Strings.Current_Location_} {player.X}, {player.Y}, {player.Map}" );
            UOC.SystemMessage( $"Region: {Regions.Regions.GetRegion( Engine.Player )}" );

            return true;
        }

        public static bool CheckCommand( byte[] data, int length )
        {
            string text = null;

            if ( data[0] == 0xAD )
            {
                text = ParseUnicodeSpeech( data, data.Length );
            }
            else if ( data[0] == 0x03 )
            {
                text = ParseAsciiSpeech( data, data.Length );
            }

            if ( string.IsNullOrEmpty(text) || text[0] != Options.CurrentOptions.CommandPrefix )
            {
                return false;
            }

            string[] args = text.Remove( 0, 1 ).Split( ' ' );

            return _commands.ContainsKey( args[0] ) && _commands[args[0]].Invoke( args.Skip( 1 ).ToArray() );
        }

        private static string ParseAsciiSpeech( byte[] data, int length )
        {
            PacketReader reader = new PacketReader( data, length, false );

            int messageType = reader.ReadByte();
            int hue = reader.ReadInt16();
            int font = reader.ReadInt16();

            string text = reader.ReadString();

            return text.Trim();
        }

        internal static string ParseUnicodeSpeech( byte[] data, int length )
        {
            PacketReader reader = new PacketReader( data, length, false );

            int messageType = reader.ReadByte();
            int hue = reader.ReadInt16();
            int font = reader.ReadInt16();
            string lang = reader.ReadString( 4 );

            string text;

            // skip keywords
            if ( ( messageType & 0xC0 ) != 0 )
            {
                //https://github.com/runuo/runuo/blob/master/Server/Network/PacketHandlers.cs
                int count = ( reader.ReadInt16() & 0xFFF0 ) >> 4;

                if ( count < 0 || count > 50 )
                {
                    return null;
                }

                for ( int i = 0; i < count; ++i )
                {
                    if ( ( i & 1 ) == 0 )
                    {
                        reader.ReadByte();
                    }
                    else
                    {
                        reader.ReadInt16();
                    }
                }

                text = reader.ReadString();
            }
            else
            {
                text = reader.ReadUnicodeString();
            }

            return text.Trim();
        }
    }
}