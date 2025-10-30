#region License

// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClassicAssist.Plugin.Shared;
using ClassicAssist.Plugin.Shared.Reflection;
using ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects;
using ClassicAssist.Shared;
using CUO_API;
using StreamJsonRpc;

// ReSharper disable once CheckNamespace
namespace Assistant
{
    public static class Engine
    {
        public static Queue<Action> TickWorkQueue { get; set; } = new Queue<Action>();
        
        private static IPluginMethods _plugin;
        private static HostMethods _hostMethods;
        private static OnConnected _onConnected;
        private static OnDisconnected _onDisconnected;
        private static OnPacketSendRecv _onReceive;
        private static OnPacketSendRecv _onSend;
        private static OnTick _onTick;
        private static OnGetUOFilePath _getUOFilePath;
        private static OnPacketSendRecv _sendToClient;
        private static OnPacketSendRecv _sendToServer;
        private static OnGetPacketLength _getPacketLength;
        private static OnUpdatePlayerPosition _onPlayerPositionChanged;
        private static OnSetTitle _setTitle;
        private static OnClientClose _onClientClosing;
        private static OnHotkey _onHotkeyPressed;
        private static RequestMove _requestMove;
        private static OnMouse _onMouse;
        private static OnFocusGained _onFocusGained;
        private static OnFocusLost _onFocusLost;

        public static Assembly ClassicAssembly { get; set; }

        public static string ClientPath { get; set; }

        public static Version ClientVersion { get; set; }

        public static string StartupPath { get; set; }

        public static IntPtr WindowHandle { get; set; }

        private static Assembly OnAssemblyResolve( object sender, ResolveEventArgs args )
        {
            string startup = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            string assemblyname = new AssemblyName( args.Name ).Name;

            string[] searchPaths = { startup, RuntimeEnvironment.GetRuntimeDirectory() };

            foreach ( string searchPath in searchPaths )
            {
                if ( !Directory.Exists( searchPath ) )
                {
                    throw new FileNotFoundException( nameof( searchPath ) );
                }
            }

            if ( assemblyname.Contains( "Colletions" ) )
            {
                assemblyname = "System.Collections";
            }

            foreach ( string searchPath in searchPaths )
            {
                if ( searchPath == null )
                {
                    continue;
                }

                string fullPath = Path.Combine( searchPath, assemblyname + ".dll" );

                string culture = new AssemblyName( args.Name ).CultureName;

                if ( !File.Exists( fullPath ) )
                {
                    string culturePath = Path.Combine( searchPath, culture, assemblyname + ".dll" );

                    if ( File.Exists( culturePath ) )
                    {
                        fullPath = culturePath;
                    }
                    else
                    {
                        continue;
                    }
                }

                Assembly assembly = Assembly.LoadFrom( fullPath );

                return assembly;
            }

            return null;
        }

        public static unsafe void Install( PluginHeader* plugin )
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            InitializePlugin( plugin );

            ClassicAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault( a => a.FullName.StartsWith( "ClassicUO," ) || a.FullName.StartsWith( "TazUO," ) );
            
            if ( ClassicAssembly != null )
            {
                CUOPath = Path.GetDirectoryName( ClassicAssembly.Location );
            }

            LaunchUI();
        }

        public static string CUOPath { get; set; }

        private static unsafe void InitializePlugin( PluginHeader* plugin )
        {
            _onConnected = OnConnected;
            _onDisconnected = OnDisconnected;
            _onReceive = OnPacketReceive;
            _onSend = OnPacketSend;
            _onPlayerPositionChanged = OnPlayerPositionChanged;
            _onClientClosing = OnClientClosing;
            _onHotkeyPressed = OnHotkeyPressed;
            _onMouse = OnMouse;
            _onTick = OnTick;
            _onFocusGained = () => OnFocusChanged( true );
            _onFocusLost = () => OnFocusChanged( false );
            WindowHandle = plugin->HWND;

            plugin->OnConnected = Marshal.GetFunctionPointerForDelegate( _onConnected );
            plugin->OnDisconnected = Marshal.GetFunctionPointerForDelegate( _onDisconnected );
            plugin->OnRecv = Marshal.GetFunctionPointerForDelegate( _onReceive );
            plugin->OnSend = Marshal.GetFunctionPointerForDelegate( _onSend );
            plugin->OnPlayerPositionChanged = Marshal.GetFunctionPointerForDelegate( _onPlayerPositionChanged );
            plugin->OnClientClosing = Marshal.GetFunctionPointerForDelegate( _onClientClosing );
            plugin->OnHotkeyPressed = Marshal.GetFunctionPointerForDelegate( _onHotkeyPressed );
            plugin->OnMouse = Marshal.GetFunctionPointerForDelegate( _onMouse );
            plugin->Tick = Marshal.GetFunctionPointerForDelegate( _onTick );
            plugin->OnFocusGained = Marshal.GetFunctionPointerForDelegate( _onFocusGained );
            plugin->OnFocusLost = Marshal.GetFunctionPointerForDelegate( _onFocusLost );

            _getPacketLength = Marshal.GetDelegateForFunctionPointer<OnGetPacketLength>( plugin->GetPacketLength );
            _getUOFilePath = Marshal.GetDelegateForFunctionPointer<OnGetUOFilePath>( plugin->GetUOFilePath );
            _sendToClient = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( plugin->Recv );
            _sendToServer = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( plugin->Send );
            _requestMove = Marshal.GetDelegateForFunctionPointer<RequestMove>( plugin->RequestMove );
            _setTitle = Marshal.GetDelegateForFunctionPointer<OnSetTitle>( plugin->SetTitle );

            ClientPath = _getUOFilePath();
            ClientVersion = new Version( (byte) ( plugin->ClientVersion >> 24 ), (byte) ( plugin->ClientVersion >> 16 ), (byte) ( plugin->ClientVersion >> 8 ),
                (byte) plugin->ClientVersion );

            if ( !Path.IsPathRooted( ClientPath ) )
            {
                ClientPath = Path.GetFullPath( ClientPath );
            }

            StartupPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            if ( StartupPath == null )
            {
                throw new InvalidOperationException();
            }
        }

        private static void LaunchUI()
        {
            string exePath = Path.Combine( StartupPath, "ClassicAssist.exe" );

            if ( !File.Exists( exePath ) )
            {
                exePath = Path.Combine( StartupPath, "..", "net9.0-windows", "ClassicAssist.exe" );
            }

            if ( !File.Exists( exePath ) )
            {
                return;
            }

#if NET
            string pipeName = $"CAPlugin_{Environment.ProcessId}";
#else
            string pipeName = $"CAPlugin_{Process.GetCurrentProcess().Id}";
#endif
            NativeMethods.SetCurrentProcessExplicitAppUserModelID( pipeName );

            ProcessStartInfo process = new ProcessStartInfo { FileName = exePath, WorkingDirectory = Path.GetDirectoryName( exePath ), Arguments = pipeName, UseShellExecute = false };

            Process.Start( process );

            NamedPipeServerStream pipe = new NamedPipeServerStream( pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous );

            _hostMethods = new HostMethods();
            pipe.WaitForConnection();
            JsonRpc rpc = JsonRpc.Attach( pipe, _hostMethods );
            _plugin = rpc.Attach<IPluginMethods>();
            
            ReflectionImpl.Initialize( ClassicAssembly, CUOPath, TickWorkQueue, Move );
        }

        private static void OnFocusChanged( bool focus )
        {
            _plugin?.OnFocusChanged( focus );
        }

        private static void OnTick()
        {
            while ( TickWorkQueue.Count > 0 )
            {
                Action action = TickWorkQueue.Dequeue();

                action?.Invoke();
            }
            
            _plugin?.OnTick();
        }

        private static void OnMouse( int button, int wheel )
        {
            _plugin?.OnMouse( button, wheel );
        }

        private static bool OnHotkeyPressed( int key, int mod, bool pressed )
        {
            return _plugin.OnHotkeyPressed( key, mod, pressed ).Result;
        }

        private static void OnClientClosing()
        {
            _plugin?.OnClientClosing();
        }

        private static void OnPlayerPositionChanged( int x, int y, int z )
        {
            _plugin?.OnPlayerPositionChanged( x, y, z );
        }

        private static bool OnPacketSend( ref byte[] data, ref int length )
        {
            if ( _plugin == null )
            {
                return true;
            }

            byte[] buffer = new byte[length];
            Buffer.BlockCopy( data, 0, buffer, 0, length );

            (bool result, byte[] newPacket, int newLength) = _plugin.OnPacketSend( buffer, buffer.Length ).Result;

            if ( newLength == 0 )
            {
                return result;
            }

            length = newLength;
            Buffer.BlockCopy( newPacket, 0, data, 0, length );

            return result;
        }

        private static bool OnPacketReceive( ref byte[] data, ref int length )
        {
            if ( _plugin == null )
            {
                return true;
            }

            byte[] buffer = new byte[length];
            Buffer.BlockCopy( data, 0, buffer, 0, length );

            (bool result, byte[] newPacket, int newLength) = _plugin.OnPacketReceive( buffer, buffer.Length ).Result;

            if ( newLength == 0 )
            {
                return result;
            }

            length = newLength;
            Buffer.BlockCopy( newPacket, 0, data, 0, length );

            return result;
        }

        private static void OnDisconnected()
        {
            _plugin?.OnDisconnected();
        }

        private static void OnConnected()
        {
            _plugin?.OnConnected();
        }

        public class HostMethods : IHostMethods
        {
            public Task<bool> SendPacketToServer( byte[] packet, int length )
            {
                return Task.FromResult( _sendToServer( ref packet, ref length ) );
            }

            public Task<bool> SendPacketToClient( byte[] packet, int length )
            {
                return Task.FromResult( _sendToClient( ref packet, ref length ) );
            }

            public Task<string> GetClientPath()
            {
                return Task.FromResult( ClientPath );
            }

            public Task<Version> GetClientVersion()
            {
                return Task.FromResult( ClientVersion );
            }

            public Task<short> GetPacketLength( int id )
            {
                return Task.FromResult( _getPacketLength( id ) );
            }

            public Task<string> GetUOFilePath()
            {
                return Task.FromResult( _getUOFilePath() );
            }

            public Task<bool> RequestMove( int dir, bool run )
            {
                return Task.FromResult( _requestMove( dir, run ) );
            }

            public void SetTitle( string title )
            {
                _setTitle( title );
            }

            public Task<(int x, int y)> GetGumpPosition( uint id )
            {
                return Task.FromResult( ReflectionImpl.GetGumpPosition( id ) );
            }

            public Task<bool> WalkTo( int x, int y, int z, int distance )
            {
                return Task.FromResult( Pathfinder.WalkTo( x, y, z, distance ) );
            }

            public Task<bool> Pathfinding()
            {
                return Task.FromResult( Pathfinder.AutoWalking );
            }

            public void CancelPathfinding()
            {
                Pathfinder.Cancel();
            }

            public Task<IntPtr> GetWindowHandle()
            {
                return Task.FromResult( WindowHandle );
            }

            public void CreateMacroButton( string name, string value )
            {
                ReflectionImpl.CreateMacroButton( name, value );
            }

            public Task<Point> GetGameWindowCenter()
            {
                return Task.FromResult( ReflectionImpl.GetGameWindowCenter() );
            }

            public Task<bool> UsePrimaryAbility()
            {
                return Task.FromResult( GameActions.UsePrimaryAbility() );
            }

            public Task<bool> UseSecondaryAbility()
            {
                return Task.FromResult( GameActions.UseSecondaryAbility() );
            }

            public Task<bool> Following()
            {
                return Task.FromResult( ReflectionImpl.Following() );
            }

            public void Logout()
            {
                ReflectionImpl.Logout();
            }

            public void Quit()
            {
                ReflectionImpl.Quit();
            }

            public void AddMapMarker( string name, int x, int y, int facet, int zoomLevel, string iconName )
            {
                WorldMapGump.AddMarker( name, x, y, facet, zoomLevel, iconName );
            }

            public Task<bool> Follow( int serial )
            {
                return Task.FromResult( ReflectionImpl.Follow( serial ) );
            }

            public void PlayCUOMacro( string name )
            {
                ReflectionImpl.PlayCUOMacro( name );
            }

            public Task<bool> HasDisconnectedGump()
            {
                return Task.FromResult( ReflectionImpl.HasDisconnectedGump() );
            }
        }

        public static void Move( int subCode, bool b )
        {
            _requestMove(subCode, b );
        }
    }
}