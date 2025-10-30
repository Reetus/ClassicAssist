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
using System.Drawing;
using System.Linq;
using System.Reflection;
using ClassicAssist.Plugin.Shared.Reflection.ClassicUO;
using ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects;
using ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects.Gumps;
using ClassicAssist.Plugin.Shared.Reflections.Helpers;

namespace ClassicAssist.Plugin.Shared.Reflection
{
    public static class ReflectionImpl
    {
        public static string ClassicUOPath { get; set; }
        public static Assembly DefaultAssembly { get; set; }

        public static Action<int, bool> Move { get; set; }

        public static Queue<Action> TickWorkQueue { get; set; }

        public static void Initialize( Assembly assembly, string classicUOPath, Queue<Action> tickWorkQueue, Action<int, bool> move )
        {
            DefaultAssembly = assembly;
            ClassicUOPath = classicUOPath;
            TickWorkQueue = tickWorkQueue;
            Move = move;
        }

        public static (int x, int y) GetGumpPosition( uint id )
        {
            IEnumerable<dynamic> gumps = Gumps.GetGumps();

            dynamic gump = gumps.FirstOrDefault( e => e.ServerSerial == id );

            if ( gump != null )
            {
                dynamic location = gump.Location;
                int x = location.X;
                int y = location.Y;
                return ( x, y );
            }

            return ( -1, -1 );
        }

        public static Point GetGameWindowCenter()
        {
            dynamic settings = ReflectionHelper.GetTypeFieldValue<dynamic>( "ClassicUO.Configuration.Settings", "GlobalSettings", null );

            string[] possibleProperties = { "Current", "CurrentProfile" };

            dynamic currentProfile = null;

            foreach ( string possibleProperty in possibleProperties )
            {
                currentProfile = ReflectionHelper.GetTypePropertyValue<dynamic>( "ClassicUO.Configuration.ProfileManager", possibleProperty, null );

                if ( currentProfile != null )
                {
                    break;
                }
            }

            if ( currentProfile == null )
            {
                return Point.Empty;
            }

            dynamic gameWindowSize = ReflectionHelper.GetTypePropertyValue<dynamic>( currentProfile.GetType(), "GameWindowSize", currentProfile );
            dynamic gameWindowPosition = ReflectionHelper.GetTypePropertyValue<dynamic>( currentProfile.GetType(), "GameWindowPosition", currentProfile );

            if ( gameWindowSize == null || gameWindowPosition == null )
            {
                return Point.Empty;
            }

            return new Point( gameWindowPosition.X + ( gameWindowSize.X >> 1 ), gameWindowPosition.Y + ( gameWindowSize.Y >> 1 ) );
        }

        public static bool Following()
        {
            dynamic gameScene = new GameScene();

            return gameScene._followingMode;
        }

        public static void Logout()
        {
            TickWorkQueue.Enqueue( () =>
            {
                dynamic socket = new ReflectionObject( ReflectionHelper.GetTypePropertyValue<dynamic>( "ClassicUO.Network.NetClient", "Socket", null ) );

                if ( socket.AssociatedObject != null )
                {
                    if ( socket.IsConnected )
                    {
                        socket.Disconnect();
                    }

                    dynamic game = new ReflectionObject( ReflectionHelper.GetTypePropertyValue<dynamic>( "ClassicUO.Client", "Game", null ) );

                    object instance = ReflectionHelper.CreateInstanceOfType( "ClassicUO.Game.Scenes.LoginScene" );

                    game.SetScene( instance );
                }
                else
                {
                    World world = new World();

                    MethodInfo logout = DefaultAssembly.GetType( "ClassicUO.Game.GameActions" ).GetMethod( "Logout" );

                    logout?.Invoke( null, new[] { world.AssociatedObject } );
                }
            } );
        }

        public static void Quit()
        {
            TickWorkQueue.Enqueue( () =>
            {
                dynamic game = new ReflectionObject( ReflectionHelper.GetTypePropertyValue<dynamic>( "ClassicUO.Client", "Game", null ) );

                game.Exit();
            } );
        }

        public static void AddMapMarker( string name, int x, int y, int facet, int zoomLevel, string iconName )
        {
            WorldMapGump.AddMarker( name, x, y, facet, zoomLevel, iconName );
        }

        public static bool Pathfinding()
        {
            return Pathfinder.AutoWalking;
        }

        public static void CreateMacroButton( string name, string value )
        {
            GameScene gameScene = new GameScene();

            IEnumerable<Macro> allMacros = gameScene.Macros.GetAllMacros();

            Macro macroObj = allMacros.FirstOrDefault( e => e.Name == name );

            if ( macroObj == null )
            {
                macroObj = new Macro( name );

                gameScene.Macros.PushToBack( macroObj );
            }

            macroObj.Items = new MacroObjectString( value );

            MacroButtonGump macroButton = new MacroButtonGump( macroObj, 200, 200 );

            UIManager.Add( macroButton );
        }

        public static bool Follow( int serial )
        {
            dynamic gameScene = new GameScene();

            if ( serial <= 0 )
            {
                gameScene._followingMode = false;
            }
            else
            {
                gameScene._followingMode = true;
                gameScene._followingTarget = (uint) serial;
            }
            
            return gameScene._followingMode;
        }

        public static bool WalkTo( int x, int y, int z, int desiredDistance )
        {
            return Pathfinder.WalkTo( x, y, z, desiredDistance );
        }

        public static void CancelPathfinding()
        {
            Pathfinder.Cancel();
        }

        public static void PlayCUOMacro( string name )
        {
            Macros.PlayCUOMacro( name );
        }

        public static bool HasDisconnectedGump()
        {
            IEnumerable<dynamic> gumps = Gumps.GetGumps();
            
            if (gumps == null)
            {
                return false;
            }
            
            dynamic obj = gumps.FirstOrDefault( g => g.GetType().Name == "MessageBoxGump" );
            
            MessageBoxGump messageBox = new MessageBoxGump( obj );

            dynamic label = messageBox.Children.FirstOrDefault(
                ele => ele.GetType().ToString().Contains( "Label" ) );

            dynamic button = messageBox.Children.FirstOrDefault(
                ele => ele.GetType().ToString().Contains( "Button" ) );

            if ( label == null || button == null )
            {
                return false;
            }

            dynamic textProperty = label.GetType()
                .GetProperty( "Text", BindingFlags.Instance | BindingFlags.Public );

            dynamic textValue = textProperty.GetValue( label, null );

            // TODO: resources
            return textValue.Contains( "Connection lost:" );
        }
    }
}