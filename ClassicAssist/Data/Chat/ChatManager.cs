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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Chat
{
    public class ChatManager
    {
        public delegate void dChannelCreated( string channel );

        public delegate void dChannelRemoved( string channel );

        public delegate void dChatMessage( string username, string channel, string message );

        public delegate void dClearUsers();

        public delegate void dJoinedChatChannel( string channel );

        public delegate void dLeftChatChannel( string channel );

        public delegate void dUserJoined( string username, string channel );

        public delegate void dUserLeft( string username, string channel );

        private static readonly object _instanceLock = new object();
        private static ChatManager _instance;
        public List<string> Channels { get; set; } = new List<string>();

        public string CurrentChannel { get; set; }

        public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();

        public List<string> Users { get; set; } = new List<string>();

        public event dJoinedChatChannel JoinedChatChannelEvent;
        public event dLeftChatChannel LeftChatChannelEvent;
        public event dChannelCreated ChannelCreatedEvent;
        public event dChannelRemoved ChannelRemovedEvent;
        public event dChatMessage ChatMessageEvent;
        public event dUserJoined UserJoinedEvent;
        public event dUserLeft UserLeftEvent;
        public event dClearUsers ClearUsersEvent;

        public static ChatManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new ChatManager();
                    }
                }
            }

            return _instance;
        }

        public void OnChatPacket( PacketReader reader )
        {
            int messageNum = reader.ReadInt16();

            reader.ReadString( 4 ); // Language

            byte[] packet = reader.GetData();

            switch ( messageNum )
            {
                case 0x25:
                {
                    int messageType = reader.ReadInt16();
                    string username = reader.ReadUnicodeString();
                    string message = reader.ReadUnicodeString();

                    Match matches = Regex.Match( message, "{(.*)}\\s+(.*)" );

                    if ( matches.Success )
                    {
                        string channel = matches.Groups[1].Value;
                        string text = matches.Groups[2].Value;

                        Messages.Add( new ChatMessage { Username = username, Channel = channel, Text = text } );
                        ChatMessageEvent?.Invoke( username, channel, text );
                    }

                    break;
                }

                case 0x3e8:
                {
                    string channel = reader.ReadUnicodeString();

                    if ( !Channels.Contains( channel ) )
                    {
                        Channels.Add( channel );
                        ChannelCreatedEvent?.Invoke( channel );
                    }

                    break;
                }

                case 0x3e9:
                {
                    string channel = reader.ReadUnicodeString();

                    if ( Channels.Contains( channel ) )
                    {
                        Channels.Remove( channel );
                        ChannelRemovedEvent?.Invoke( channel );
                    }

                    break;
                }

                case 0x3ee:
                {
                    reader.ReadInt16();
                    string userName = reader.ReadUnicodeString();

                    if ( !Users.Contains( userName ) )
                    {
                        Users.Add( userName );
                        UserJoinedEvent?.Invoke( userName, string.Empty );
                    }

                    break;
                }
                case 0x3ef:
                {
                    string userName = reader.ReadUnicodeString();

                    if ( Users.Contains( userName ) )
                    {
                        Users.Remove( userName );
                        UserLeftEvent?.Invoke( userName, string.Empty );
                    }

                    break;
                }

                case 0x3f1:
                {
                    CurrentChannel = reader.ReadUnicodeString();
                    JoinedChatChannelEvent?.Invoke( CurrentChannel );
                    break;
                }

                case 0x3f0:
                {
                    Users.Clear();
                    ClearUsersEvent?.Invoke();
                    break;
                }

                case 0x3f4:
                {
                    LeftChatChannelEvent?.Invoke( CurrentChannel );
                    CurrentChannel = null;
                    break;
                }

                // ReSharper disable once RedundantEmptySwitchSection
                default:
                    break;
            }
        }
    }
}