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
using System.Windows.Input;
using System.Windows.Media;
using ClassicAssist.Shared;
using ClassicAssist.Data.Chat;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UO;

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
        private ICommand _changeChannelCommand;
        private ObservableCollection<string> _channels = new ObservableCollection<string>();
        private string _currentChannel;
        private ObservableCollection<ChatMessage> _messages = new ObservableCollection<ChatMessage>();
        private string _selectedChannel;
        private string _title;
        private ICommand _toggleOptionCommand;
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
            _manager.ChannelCreatedEvent += OnChannelCreated;
            _manager.ChannelRemovedEvent += OnChannelRemoved;

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
                //TODO2
                Users.AddSorted( new ChatUser { Username = user/*, Colour = GetUserColour( user )*/ } );
            }

            foreach ( string channel in _manager.Channels )
            {
                OnChannelCreated( channel );
            }

            //foreach ( ChatMessage message in _manager.Messages )
            //{
            //    Messages.Add( message );
            //}

            _manager.ChatMessageEvent += OnChatMessage;
        }

        public ICommand ChangeChannelCommand =>
            _changeChannelCommand ??
            ( _changeChannelCommand = new RelayCommand( ChangeChannel, o => Channels.Count != 0 ) );

        public ObservableCollection<string> Channels
        {
            get => _channels;
            set => SetProperty( ref _channels, value );
        }

        public string CurrentChannel
        {
            get => _currentChannel;
            set => SetProperty( ref _currentChannel, value );
        }

        public ObservableCollection<ChatMessage> Messages
        {
            get => _messages;
            set => SetProperty( ref _messages, value );
        }

        public string SelectedChannel
        {
            get => _selectedChannel;
            set => SetProperty( ref _selectedChannel, value );
        }

        public string Title
        {
            get => _title;
            set => SetProperty( ref _title, value );
        }

        public ICommand ToggleOptionCommand =>
            _toggleOptionCommand ?? ( _toggleOptionCommand = new RelayCommand( ToggleOption, o => true ) );

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

        private static void ToggleOption( object obj )
        {
        }

        private void ChangeChannel( object obj )
        {
            if ( !( obj is string channel ) )
            {
                return;
            }

            _dispatcher.Invoke( () => { Commands.JoinChatChannel( channel ); } );
        }

        private void OnChannelRemoved( string channel )
        {
            _dispatcher.Invoke( () => { Channels.Remove( channel ); } );
        }

        private void OnChannelCreated( string channel )
        {
            _dispatcher.Invoke( () => { Channels.Add( channel ); } );
        }

        private void OnChatMessage( string username, string channel, string message )
        {
            _dispatcher.Invoke( () => Messages.Add( new ChatMessage
            {
                //TODO2
                Username = username,
                Channel = channel,
                Text = message/*, Colour = GetUserColour( username )*/
            } ) );
        }

        private void OnLeftChatChannel( string channel )
        {
            _dispatcher.Invoke( () =>
            {
                Users.Clear();
                CurrentChannel = null;
                SelectedChannel = null;
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
            _dispatcher.Invoke( () =>
            {
                CurrentChannel = channel;
                SelectedChannel = channel;
                UpdateTitle( channel );
            } );
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
                //TODO2
                ChatUser user = new ChatUser { Username = username/*, Colour = GetUserColour( username )*/ };

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
            catch ( Exception )
            {
                return _defaultBrush;
            }
        }
    }
}