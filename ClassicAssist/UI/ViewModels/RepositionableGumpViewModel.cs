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

using ClassicAssist.Shared;
using ClassicAssist.Misc;
using ClassicAssist.UO.Gumps;

namespace ClassicAssist.UI.ViewModels
{
    public class RepositionableGumpViewModel : BaseViewModel
    {
        private readonly RepositionableGump _gump;
        private int _horizontalMax;
        private int _verticalMax;
        private int _x;
        private int _y;

        public RepositionableGumpViewModel()
        {
        }

        public RepositionableGumpViewModel( RepositionableGump gump, int initialX, int initialY )
        {
            _gump = gump;
            X = initialX;
            Y = initialY;
            HorizontalMax = 3840;
            VerticalMax = 2160;

            if ( !NativeMethods.GetWindowRect( Engine.WindowHandle, out NativeMethods.RECT rect ) )
            {
                return;
            }

            HorizontalMax = rect.Right - rect.Left;
            VerticalMax = rect.Bottom - rect.Top;
        }

        public int HorizontalMax
        {
            get => _horizontalMax;
            set => SetProperty( ref _horizontalMax, value );
        }

        public int VerticalMax
        {
            get => _verticalMax;
            set => SetProperty( ref _verticalMax, value );
        }

        public int X
        {
            get => _x;
            set
            {
                SetProperty( ref _x, value );
                _gump?.SetPosition( _x, _y );
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                SetProperty( ref _y, value );
                _gump?.SetPosition( _x, _y );
            }
        }
    }
}