#region License

// Copyright (C) 2022 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion
#if !NET
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Helpers;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;

namespace ClassicAssist.Extensions
{
    public class AutologinExtension : IExtension
    {
        private const int STEP_TIMEOUT = 5000;
        private int _characterIndex;
        private MethodInfo _connectMethod;
        private string _password;
        private dynamic _scene;
        private MethodInfo _selectCharacterMethod;
        private MethodInfo _selectServerMethod;
        private int _serverIndex;
        private string _username;
        private AutologinStatusWindow _window;

        public bool IsLoggingIn { get; set; }

        public void Initialize()
        {
            AssistantOptions.ProfileChangedEvent += OnProfileChangedEvent;
            Engine.DisconnectedEvent += OnDisconnectedEvent;

            Type gameSceneType = Engine.ClassicAssembly.GetType( "ClassicUO.Game.Scenes.LoginScene" );

            _connectMethod = gameSceneType.GetMethod( "Connect", BindingFlags.Public | BindingFlags.Instance );

            _selectServerMethod =
                gameSceneType.GetMethod( "SelectServer", BindingFlags.Public | BindingFlags.Instance );

            _selectCharacterMethod =
                gameSceneType.GetMethod( "SelectCharacter", BindingFlags.Public | BindingFlags.Instance );
        }

        private void OnDisconnectedEvent()
        {
            Task.Delay( 2000 ).ContinueWith( t =>
            {
                // Wait for the scene to change
                CheckAutologin( Options.CurrentOptions.AutologinReconnectDelay );
            } );
        }

        private void OnProfileChangedEvent( string profile )
        {
            Task.Delay( 2000 ).ContinueWith( t => { CheckAutologin( Options.CurrentOptions.AutologinConnectDelay ); } );
        }

        private void CheckAutologin( TimeSpan connectInterval )
        {
            if ( IsLoggingIn )
            {
                return;
            }

            if ( !Options.CurrentOptions.Autologin )
            {
                return;
            }

            _scene = GetLoginScene();

            if ( _scene == null )
            {
                return;
            }

            string step = GetCurrentStep( _scene );

            if ( step != "Main" )
            {
                return;
            }

            _username = Options.CurrentOptions.AutologinUsername;
            _password = Options.CurrentOptions.AutologinPassword;
            _serverIndex = Options.CurrentOptions.AutologinServerIndex;
            _characterIndex = Options.CurrentOptions.AutologinCharacterIndex;

            PerformLogin( connectInterval ).ConfigureAwait( false );
        }

        private static string GetCurrentStep( object scene )
        {
            PropertyInfo stepProperty = scene.GetType()
                .GetProperty( "CurrentLoginStep", BindingFlags.Public | BindingFlags.Instance );

            if ( stepProperty == null )
            {
                return "Unknown";
            }

            return stepProperty.GetValue( scene )?.ToString() ?? "Unknown";
        }

        private async Task<bool> PerformLogin( TimeSpan connectInterval )
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            try
            {
                ShowWindow( cancellationTokenSource );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
            }

            AddMessage( string.Format( Strings.Autologin_in__0__seconds___, connectInterval.TotalSeconds ) );

            await Task.Delay( connectInterval, cancellationTokenSource.Token );

            if ( cancellationTokenSource.IsCancellationRequested )
            {
                return false;
            }

            try
            {
                IsLoggingIn = true;

                AddMessage( Strings.Connecting___ );

                Engine.TickWorkQueue.Enqueue( () =>
                    _connectMethod?.Invoke( _scene, new object[] { _username, _password } ) );

                bool step_result = await WaitForStep( "ServerSelection", STEP_TIMEOUT, cancellationTokenSource.Token );

                if ( cancellationTokenSource.IsCancellationRequested )
                {
                    return false;
                }

                if ( !step_result )
                {
                    AddMessage( Strings.Timeout_waiting_for_shard_selection_screen );
                    return false;
                }

                List<ServerEntry> servers = GetServerList();

                if ( servers == null )
                {
                    AddMessage( Strings.Failed_to_get_server_list );
                    return false;
                }

                ServerEntry selectedServer = servers.FirstOrDefault( e => e.Index == _serverIndex );

                if ( selectedServer == null )
                {
                    AddMessage( Strings.Failed_to_get_selected_server_index );
                    return false;
                }

                AddMessage( Strings.Servers_ );

                foreach ( ServerEntry server in servers )
                {
                    try
                    {
                        AddMessage( $"  {server.Index} - {server.Name}" );
                    }
                    catch ( Exception e )
                    {
                        AddMessage( e.Message );
                    }
                }

                AddMessage( string.Format( Strings.Selecting_server__0____, _serverIndex ) );

                Engine.TickWorkQueue.Enqueue( () =>
                {
                    _selectServerMethod?.Invoke( _scene, new object[] { (byte) _serverIndex } );
                } );

                step_result = await WaitForStep( "CharacterSelection", STEP_TIMEOUT, cancellationTokenSource.Token );

                if ( cancellationTokenSource.IsCancellationRequested )
                {
                    return false;
                }

                if ( !step_result )
                {
                    AddMessage( Strings.Timeout_waiting_for_character_selection_screen );
                    return false;
                }

                string[] characters = GetCharacterList();

                if ( characters == null )
                {
                    AddMessage( Strings.Failed_to_get_character_list );
                    return false;
                }

                if ( characters.Length < _characterIndex + 1 || characters[_characterIndex] == null )
                {
                    AddMessage( Strings.Character_index_not_found );
                    return false;
                }

                AddMessage( Strings.Characters_ );

                foreach ( ( string character, int index ) in characters.Select( ( character, index ) =>
                             ( character, index ) ) )
                {
                    AddMessage( $"  {index} - {character}" );
                }

                AddMessage( string.Format( Strings.Selecting_character__0____, _characterIndex ) );

                Engine.TickWorkQueue.Enqueue( () =>
                {
                    _selectCharacterMethod?.Invoke( _scene, new object[] { (uint) _characterIndex } );
                } );
            }
            catch ( Exception e )
            {
                AddMessage( e.Message );
            }
            finally
            {
                IsLoggingIn = false;
            }

            await Task.Delay( 500, cancellationTokenSource.Token );
            HideWindow();

            return true;
        }

        private List<ServerEntry> GetServerList()
        {
            List<ServerEntry> results = new List<ServerEntry>();

            PropertyInfo serverProperty = _scene.GetType()
                .GetProperty( "Servers", BindingFlags.Public | BindingFlags.Instance );

            if ( serverProperty == null )
            {
                return null;
            }

            dynamic value = serverProperty.GetValue( _scene );

            if ( value == null )
            {
                return null;
            }

            foreach ( dynamic item in value )
            {
                dynamic index = Convert.ToInt32( item.GetType().GetField( "Index" )?.GetValue( item ) );
                string name = item.GetType().GetField( "Name" )?.GetValue( item ).ToString();

                if ( !string.IsNullOrEmpty( name ) )
                {
                    results.Add( new ServerEntry { Index = index, Name = name } );
                }
            }

            return results.Count > 0 ? results : null;
        }

        private string[] GetCharacterList()
        {
            PropertyInfo charactersProperty = _scene.GetType()
                .GetProperty( "Characters", BindingFlags.Public | BindingFlags.Instance );

            if ( charactersProperty == null )
            {
                return null;
            }

            dynamic value = charactersProperty.GetValue( _scene );

            return value is string[] strings ? strings : null;
        }

        private static dynamic GetLoginScene()
        {
            dynamic game = Reflection.GetTypePropertyValue<dynamic>( "ClassicUO.Client", "Game", null );

            Type gameType = game.GetType();

            MethodInfo getSceneMethod = gameType.GetMethod( "GetScene", BindingFlags.Public | BindingFlags.Instance );

            Type gameSceneType = Engine.ClassicAssembly.GetType( "ClassicUO.Game.Scenes.LoginScene" );

            MethodInfo method = getSceneMethod?.MakeGenericMethod( gameSceneType );

            return method?.Invoke( game, null );
        }

        private async Task<bool> WaitForStep( string step, int timeout, CancellationToken cancellationToken )
        {
            Stopwatch elapsed = new Stopwatch();

            elapsed.Start();

            do
            {
                object stepProperty = _scene.GetType()
                    .GetProperty( "CurrentLoginStep", BindingFlags.Public | BindingFlags.Instance )
                    ?.GetValue( _scene, null );

                if ( stepProperty?.ToString() == step )
                {
                    return true;
                }

                await Task.Delay( 100, cancellationToken );

                if ( cancellationToken.IsCancellationRequested )
                {
                    return false;
                }
            }
            while ( elapsed.ElapsedMilliseconds < timeout );

            return false;
        }

        private void ShowWindow( CancellationTokenSource cancellationTokenSource )
        {
            Engine.Dispatcher.Invoke( () =>
            {
                _window = new AutologinStatusWindow
                {
                    DataContext = new AutologinStatusViewModel( cancellationTokenSource )
                };

                _window.Show();
            } );
        }

        private void HideWindow()
        {
            Engine.Dispatcher.Invoke( () => _window.Close() );
        }

        private void AddMessage( string message )
        {
            Engine.Dispatcher.Invoke( () =>
            {
                if ( _window?.DataContext is AutologinStatusViewModel vm )
                {
                    vm.Messages.Add( message );
                }
            } );
        }
    }

    public class ServerEntry
    {
        public int Index { get; set; }
        public string Name { get; set; }
    }
}
#endif