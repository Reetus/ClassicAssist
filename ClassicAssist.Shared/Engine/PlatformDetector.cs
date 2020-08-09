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

// ReSharper disable once CheckNamespace
using System;
using System.Runtime.InteropServices;

namespace ClassicAssist.Shared
{
    public enum PlatformType
    {
        Windows,
        Unix,
        MacOSX
    }

    public static partial class Engine
    {
        private static readonly Lazy<PlatformType> m_Platform = new Lazy<PlatformType>( DetectPlatformType );

        public static PlatformType GetPlatformType()
        {
#if DEBUG
            Console.WriteLine( $"Using Platform: {m_Platform.Value}" );
#endif
            return m_Platform.Value;
        }

        private static PlatformType DetectPlatformType()
        {
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
            {
                return PlatformType.Unix;
            }

            if ( RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
            {
                return PlatformType.MacOSX;
            }

            return PlatformType.Windows;
        }
    }
}