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
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;

namespace ClassicAssist.Plugin.Shared
{
    public interface IHostMethods
    {
        Task<bool> SendPacketToServer( byte[] packet, int length );
        Task<bool> SendPacketToClient( byte[] packet, int length );
        Task<string> GetClientPath();
        Task<Version> GetClientVersion();
        Task<short> GetPacketLength( int id );
        Task<string> GetUOFilePath();
        Task<bool> RequestMove( int dir, bool run );
        void SetTitle( string title );
        Task<(int x, int y)> GetGumpPosition( uint id );
        Task<bool> WalkTo( int x, int y, int z, int distance );
        Task<bool> Pathfinding();
        void CancelPathfinding();
        Task<IntPtr> GetWindowHandle();
        void CreateMacroButton( string name, string value );
        Task<Point> GetGameWindowCenter();
        Task<bool> UsePrimaryAbility();
        Task<bool> UseSecondaryAbility();
        Task<bool> Following();
        void Logout();
        void Quit();
        void AddMapMarker( string name, int x, int y, int facet, int zoomLevel, string iconName );
        Task<bool> Follow( int serial );
        void PlayCUOMacro( string name );
        Task<bool> HasDisconnectedGump();
    }
}