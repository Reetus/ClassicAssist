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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

//https://stackoverflow.com/questions/4305565/wpf-context-menu-on-left-click
namespace ClassicAssist.UI.Misc
{
    public class ClickOpensContextMenuBehaviour
    {
        private static readonly DependencyProperty ClickOpensContextMenuProperty =
            DependencyProperty.RegisterAttached(
                "Enabled", typeof( bool ), typeof( ClickOpensContextMenuBehaviour ),
                new PropertyMetadata( HandlePropertyChanged )
            );

        public static bool GetEnabled( DependencyObject obj )
        {
            return (bool) obj.GetValue( ClickOpensContextMenuProperty );
        }

        public static void SetEnabled( DependencyObject obj, bool value )
        {
            obj.SetValue( ClickOpensContextMenuProperty, value );
        }

        private static void HandlePropertyChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            switch ( obj )
            {
                case Image image:
                    image.MouseLeftButtonDown -= ExecuteMouseDown;
                    image.MouseLeftButtonDown += ExecuteMouseDown;
                    break;
                case Hyperlink hyperlink:
                    hyperlink.Click -= ExecuteClick;
                    hyperlink.Click += ExecuteClick;
                    break;
            }
        }

        private static void ExecuteMouseDown( object sender, MouseButtonEventArgs args )
        {
            if ( !( sender is DependencyObject obj ) )
                return;

            bool enabled = (bool) obj.GetValue( ClickOpensContextMenuProperty );

            if ( !enabled )
            {
                return;
            }

            Image image = sender as Image;
            if ( image?.ContextMenu != null )
                image.ContextMenu.IsOpen = true;
        }

        private static void ExecuteClick( object sender, RoutedEventArgs args )
        {
            if ( !( sender is DependencyObject obj ) )
                return;

            bool enabled = (bool) obj.GetValue( ClickOpensContextMenuProperty );

            if ( !enabled )
            {
                return;
            }

            if ( !( sender is Hyperlink hyperlink ) )
            {
                return;
            }

            if ( hyperlink.ContextMenu != null )
                hyperlink.ContextMenu.IsOpen = true;
        }
    }
}