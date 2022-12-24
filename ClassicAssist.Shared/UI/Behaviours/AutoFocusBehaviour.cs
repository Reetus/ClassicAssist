﻿#region License

// Copyright (C) 2021 Reetus
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ClassicAssist.Shared.UI.Behaviours
{
    public class AutoFocusBehaviour : Behavior<Control>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
            AssociatedObject.IsVisibleChanged += AssociatedObjectOnIsVisibleChanged;
        }

        private void AssociatedObjectOnIsVisibleChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            if ( e.NewValue is bool boolVal && boolVal )
            {
                AssociatedObject.Focus();
            }
        }

        private void AssociatedObjectOnLoaded( object sender, RoutedEventArgs e )
        {
            AssociatedObject.Focus();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
            AssociatedObject.IsVisibleChanged -= AssociatedObjectOnIsVisibleChanged;

            base.OnDetaching();
        }
    }
}