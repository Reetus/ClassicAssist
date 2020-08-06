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

using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace ClassicAssist.Avalonia.Misc
{
    public class ListBoxAutoScrollBehaviour : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if ( AssociatedObject != null )
            {
                ( (INotifyCollectionChanged) AssociatedObject.Items ).CollectionChanged += OnCollectionChanged;
            }
        }

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            ScrollViewer scrollViewer = AssociatedObject.FindDescendantOfType<ScrollViewer>();

            scrollViewer?.ScrollToEnd();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if ( AssociatedObject != null )
            {
                ( (INotifyCollectionChanged) AssociatedObject.Items ).CollectionChanged -= OnCollectionChanged;
            }
        }
    }
}