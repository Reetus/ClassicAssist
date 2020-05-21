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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Assistant;
using ClassicAssist.Data.Chat;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.Resources;

namespace ClassicAssist.UI.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        private readonly SolidColorBrush _defaultBrush =
            new SolidColorBrush( Color.FromArgb( 0xff, 0xcc, 0xcc, 0xcc ) );

        private readonly SolidColorBrush _enemyBrush = new SolidColorBrush( Colors.Orange );

        private readonly SolidColorBrush _friendBrush = new SolidColorBrush( Colors.LimeGreen );

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ChatManager _manager;
        private ObservableCollection<ChatMessage> _messages = new ObservableCollection<ChatMessage>();
        private string _title;
        private bool _topmost = true;
        private ObservableCollection<ChatUser> _users = new ObservableCollection<ChatUser>();

        public ChatViewModel()
        {
            _manager = ChatManager.GetInstance();

            _manager.UserJoinedEvent += OnUserJoined;
            _manager.UserLeftEvent += OnUserLeft;
            _manager.ClearUsersEvent += OnClearUsers;
            _manager.JoinedChatChannelEvent += OnJoinedChatChannel;
            _manager.LeftChatChannelEvent += OnLeftChatChannel;
            Engine.DisconnectedEvent += () => OnLeftChatChannel( string.Empty );

            if ( !string.IsNullOrEmpty( _manager.CurrentChannel ) )
            {
                OnJoinedChatChannel( _manager.CurrentChannel );
            }
            else
            {
                UpdateTitle( Strings.Chat );
            }

            foreach ( string user in _manager.Users )
            {
                Users.AddSorted( new ChatUser { Username = user, Colour = GetUserColour( user ) } );
            }

            //foreach ( ChatMessage message in _manager.Messages )
            //{
            //    Messages.Add( message );
            //}

            _manager.ChatMessageEvent += OnChatMessage;
        }

        public ObservableCollection<ChatMessage> Messages
        {
            get => _messages;
            set => SetProperty( ref _messages, value );
        }

        public string Title
        {
            get => _title;
            set => SetProperty( ref _title, value );
        }

        public bool Topmost
        {
            get => _topmost;
            set => SetProperty( ref _topmost, value );
        }

        public ObservableCollection<ChatUser> Users
        {
            get => _users;
            set => SetProperty( ref _users, value );
        }

        private void OnChatMessage( string username, string channel, string message )
        {
            _dispatcher.Invoke( () => Messages.Add( new ChatMessage
            {
                Username = username, Channel = channel, Text = message, Colour = GetUserColour( username )
            } ) );
        }

        private void OnLeftChatChannel( string channel )
        {
            _dispatcher.Invoke( () =>
            {
                Users.Clear();
                UpdateTitle( string.Empty );
            } );
        }

        private void UpdateTitle( string channel )
        {
            if ( string.IsNullOrEmpty( channel ) )
            {
                Title = Strings.Chat;
                return;
            }

            Title = $"{Strings.Chat} - {channel}";
        }

        private void OnJoinedChatChannel( string channel )
        {
            _dispatcher.Invoke( () => UpdateTitle( channel ) );
        }

        private void OnClearUsers()
        {
            _dispatcher.Invoke( () => Users.Clear() );
        }

        private void OnUserLeft( string username, string channel )
        {
            _dispatcher.Invoke( () => { Users.Remove( Users.FirstOrDefault( u => u.Username == username ) ); } );
        }

        private void OnUserJoined( string username, string channel )
        {
            _dispatcher.Invoke( () =>
            {
                ChatUser user = new ChatUser { Username = username, Colour = GetUserColour( username ) };

                Users.AddSorted( user );
            } );
        }

        private SolidColorBrush GetUserColour( string username )
        {
            try
            {
                Match matches = Regex.Match( username, @"<(.*)>\s*(.*)" );

                if ( !matches.Success )
                {
                    return _defaultBrush;
                }

                int serial = int.Parse( matches.Groups[1].Value );

                if ( serial == Engine.Player?.Serial || MobileCommands.InFriendList( serial ) )
                {
                    return _friendBrush;
                }

                return serial == AliasCommands.GetAlias( "enemy" ) ? _enemyBrush : _defaultBrush;
            }
            catch ( Exception e )
            {
                return _defaultBrush;
            }
        }
    }
}