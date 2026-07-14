#region License

// Copyright (C) 2025 Reetus
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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc.Behaviours
{
    /// <summary>
    ///     Clamps a borderless ( WindowStyle=None / AllowsTransparency ) window's maximized bounds to
    ///     the work area of the monitor it is currently on. Without this, WPF maximizes a layered
    ///     borderless window to the full monitor bounds, covering the taskbar and mispositioning the
    ///     content when the taskbar is docked to the top, left or right. Handling WM_GETMINMAXINFO makes
    ///     maximize behave like a normal window title bar on any monitor / taskbar layout.
    /// </summary>
    public class WindowMaximizeBehaviour : Behavior<Window>
    {
        private const int WM_GETMINMAXINFO = 0x0024;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        private HwndSource _hwndSource;

        protected override void OnAttached()
        {
            base.OnAttached();

            if ( AssociatedObject.IsInitialized )
            {
                Hook();
            }
            else
            {
                AssociatedObject.SourceInitialized += OnSourceInitialized;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SourceInitialized -= OnSourceInitialized;
            _hwndSource?.RemoveHook( WndProc );
            _hwndSource = null;
        }

        private void OnSourceInitialized( object sender, EventArgs e )
        {
            Hook();
        }

        private void Hook()
        {
            _hwndSource = (HwndSource) PresentationSource.FromVisual( AssociatedObject );
            _hwndSource?.AddHook( WndProc );
        }

        private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            if ( msg != WM_GETMINMAXINFO )
            {
                return IntPtr.Zero;
            }

            WmGetMinMaxInfo( hwnd, lParam );
            handled = true;

            return IntPtr.Zero;
        }

        private static void WmGetMinMaxInfo( IntPtr hwnd, IntPtr lParam )
        {
            IntPtr monitor = MonitorFromWindow( hwnd, MONITOR_DEFAULTTONEAREST );

            if ( monitor == IntPtr.Zero )
            {
                return;
            }

            MONITORINFO monitorInfo = new MONITORINFO { cbSize = Marshal.SizeOf( typeof( MONITORINFO ) ) };

            if ( !GetMonitorInfo( monitor, ref monitorInfo ) )
            {
                return;
            }

            MINMAXINFO mmi = (MINMAXINFO) Marshal.PtrToStructure( lParam, typeof( MINMAXINFO ) );

            RECT work = monitorInfo.rcWork;
            RECT bounds = monitorInfo.rcMonitor;

            // ptMaxPosition / ptMaxSize are expressed relative to the monitor's top-left corner.
            mmi.ptMaxPosition.X = work.Left - bounds.Left;
            mmi.ptMaxPosition.Y = work.Top - bounds.Top;
            mmi.ptMaxSize.X = work.Right - work.Left;
            mmi.ptMaxSize.Y = work.Bottom - work.Top;

            Marshal.StructureToPtr( mmi, lParam, true );
        }

        [DllImport( "user32.dll" )]
        private static extern IntPtr MonitorFromWindow( IntPtr handle, int flags );

        [DllImport( "user32.dll" )]
        private static extern bool GetMonitorInfo( IntPtr monitor, ref MONITORINFO monitorInfo );

        [StructLayout( LayoutKind.Sequential )]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }
    }
}
