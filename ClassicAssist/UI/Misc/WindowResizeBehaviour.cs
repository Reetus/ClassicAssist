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

using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using ClassicAssist.Data;

namespace ClassicAssist.UI.Misc
{
    public class WindowResizeBehaviour : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

#if !DEVELOP
            AssociatedObject.Width = AssistantOptions.WindowWidth;
            AssociatedObject.Height = AssistantOptions.WindowHeight;
#endif

            AssociatedObject.SizeChanged += OnSizeChanged;
            AssistantOptions.OptionsLoaded += OnOptionsLoaded;
        }

        private void OnOptionsLoaded( object sender, EventArgs e )
        {
            AssociatedObject.Width = AssistantOptions.WindowWidth;
            AssociatedObject.Height = AssistantOptions.WindowHeight;
        }

        private static void OnSizeChanged( object sender, SizeChangedEventArgs e )
        {
#if !DEVELOP
            AssistantOptions.WindowHeight = e.HeightChanged ? e.NewSize.Height : AssistantOptions.WindowHeight;
            AssistantOptions.WindowWidth = e.WidthChanged ? e.NewSize.Width : AssistantOptions.WindowWidth;
#endif
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SizeChanged += OnSizeChanged;
        }
    }
}