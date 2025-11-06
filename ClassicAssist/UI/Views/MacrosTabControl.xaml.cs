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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassicAssist.Data.Macros;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for MacrosTabControl.xaml
    /// </summary>
    public partial class MacrosTabControl
    {
        public MacrosTabControl()
        {
            InitializeComponent();
        }

        private void DraggableTreeView_OnPreviewMouseWheel( object sender, MouseWheelEventArgs e )
        {
            /*
             * Cheap hack for our broken template, no scrollbars, bubble event to parent scrollviewer
             */
            if ( !( sender is Control control ) || e.Handled )
            {
                return;
            }

            if ( control.Parent == null )
            {
                return;
            }

            e.Handled = true;
            MouseWheelEventArgs eventArg = new MouseWheelEventArgs( e.MouseDevice, e.Timestamp, e.Delta ) { RoutedEvent = MouseWheelEvent, Source = control };
            UIElement parent = control.Parent as UIElement;
            parent?.RaiseEvent( eventArg );
        }

        internal class SameNameComparer : IEqualityComparer<PythonCompletionData>
        {
            public bool Equals( PythonCompletionData x, PythonCompletionData y )
            {
                return y != null && x != null && x.Name.Equals( y.Name );
            }

            public int GetHashCode( PythonCompletionData obj )
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}