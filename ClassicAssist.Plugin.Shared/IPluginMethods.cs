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
using System.Threading.Tasks;

namespace ClassicAssist.Shared
{
    public interface IPluginMethods
    {
        void OnConnected();
        void OnDisconnected();
        Task<bool> OnPacketReceive( byte[] data, int length );
        Task<bool> OnPacketSend( byte[] data, int length );
        void OnClientClosing();
        Task<bool> OnHotkeyPressed( int key, int mod, bool pressed );
        void OnMouse( int button, int wheel );
        void OnTick();
        void OnFocusChanged( bool focus );
        void OnPlayerPositionChanged( int x, int y, int z );
    }
}