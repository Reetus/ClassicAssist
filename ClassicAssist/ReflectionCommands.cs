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

using System.Drawing;
using Assistant;
using ClassicAssist.Plugin.Shared.Reflection;
using ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects;

namespace ClassicAssist
{
    public static class ReflectionCommands
    {
        public static void PlayCUOMacro( string name )
        {
            if ( Engine.Host != null )
            {
                Engine.Host.PlayCUOMacro( name );
            }
            else
            {
                ReflectionImpl.PlayCUOMacro( name );
            }
        }

        public static void CreateMacroButton( string name, string value )
        {
            if ( Engine.Host != null )
            {
                Engine.Host.CreateMacroButton( name, value );
            }
            else
            {
                ReflectionImpl.CreateMacroButton( name, value );
            }
        }

        public static void Logout()
        {
            if ( Engine.Host != null )
            {
                Engine.Host.Logout();
            }
            else
            {
                ReflectionImpl.Logout();
            }
        }

        public static void Quit()
        {
            if ( Engine.Host != null )
            {
                Engine.Host.Quit();
            }
            else
            {
                ReflectionImpl.Quit();
            }
        }

        public static void AddMapMarker( string name, int x, int y, int facet, int zoomLevel, string iconName )
        {
            if ( Engine.Host != null )
            {
                Engine.Host.AddMapMarker( name, x, y, facet, zoomLevel, iconName );
            }
            else
            {
                ReflectionImpl.AddMapMarker( name, x, y, facet, zoomLevel, iconName );
            }
        }

        public static bool Following()
        {
            return Engine.Host != null ? Engine.Host.Following().Result : ReflectionImpl.Following();
        }

        public static bool Follow( int serial )
        {
            return Engine.Host != null ? Engine.Host.Follow( serial ).Result : ReflectionImpl.Follow( serial );
        }

        public static bool WalkTo( int x, int y, int z, int desiredDistance )
        {
            return Engine.Host != null ? Engine.Host.WalkTo( x, y, z, desiredDistance ).Result : ReflectionImpl.WalkTo( x, y, z, desiredDistance );
        }

        public static bool CancelPathfinding()
        {
            if ( Engine.Host != null )
            {
                Engine.Host.CancelPathfinding();
            }
            else
            {
                ReflectionImpl.CancelPathfinding();
            }

            return true;
        }

        public static bool Pathfinding()
        {
            return Engine.Host != null ? Engine.Host.Pathfinding().Result : ReflectionImpl.Pathfinding();
        }

        public static (int x, int y) GetGumpPosition( uint id )
        {
            return Engine.Host != null ? Engine.Host.GetGumpPosition( id ).Result : ReflectionImpl.GetGumpPosition( id );
        }

        public static Point GetGameWindowCenter()
        {
            return Engine.Host != null ? Engine.Host.GetGameWindowCenter().Result : ReflectionImpl.GetGameWindowCenter();            
        }

        public static bool UsePrimaryAbility()
        {       
            return Engine.Host != null ? Engine.Host.UsePrimaryAbility().Result : GameActions.UsePrimaryAbility();
        }

        public static bool UseSecondaryAbility()
        {
            return Engine.Host != null ? Engine.Host.UseSecondaryAbility().Result : GameActions.UseSecondaryAbility();
        }

        public static bool HasConnectionLostGump()
        {
            return Engine.Host != null ? Engine.Host.HasDisconnectedGump().Result : ReflectionImpl.HasDisconnectedGump();
        }
    }
}