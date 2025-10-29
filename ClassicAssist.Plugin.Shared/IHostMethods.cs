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
    }
}