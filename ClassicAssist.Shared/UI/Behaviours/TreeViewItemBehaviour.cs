#region License

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

namespace ClassicAssist.Shared.UI.Behaviours
{
    //https://www.codeproject.com/Articles/28959/Introduction-to-Attached-Behaviors-in-WPF
    /// <summary>
    ///     Exposes attached behaviors that can be
    ///     applied to TreeViewItem objects.
    /// </summary>
    public static class TreeViewItemBehavior
    {
        #region IsBroughtIntoViewWhenSelected

        public static bool GetIsBroughtIntoViewWhenSelected( TreeViewItem treeViewItem )
        {
            return (bool) treeViewItem.GetValue( IsBroughtIntoViewWhenSelectedProperty );
        }

        public static void SetIsBroughtIntoViewWhenSelected( TreeViewItem treeViewItem, bool value )
        {
            treeViewItem.SetValue( IsBroughtIntoViewWhenSelectedProperty, value );
        }

        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached( "IsBroughtIntoViewWhenSelected", typeof( bool ),
                typeof( TreeViewItemBehavior ),
                new UIPropertyMetadata( false, OnIsBroughtIntoViewWhenSelectedChanged ) );

        private static void OnIsBroughtIntoViewWhenSelectedChanged( DependencyObject depObj,
            DependencyPropertyChangedEventArgs e )
        {
            TreeViewItem item = depObj as TreeViewItem;

            if ( item == null )
            {
                return;
            }

            if ( e.NewValue is bool == false )
            {
                return;
            }

            if ( (bool) e.NewValue )
            {
                item.Selected += OnTreeViewItemSelected;
            }
            else
            {
                item.Selected -= OnTreeViewItemSelected;
            }
        }

        private static void OnTreeViewItemSelected( object sender, RoutedEventArgs e )
        {
            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified. Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            if ( !ReferenceEquals( sender, e.OriginalSource ) )
            {
                return;
            }

            if ( e.OriginalSource is TreeViewItem item )
            {
                item.BringIntoView();
            }
        }

        #endregion // IsBroughtIntoViewWhenSelected
    }
}