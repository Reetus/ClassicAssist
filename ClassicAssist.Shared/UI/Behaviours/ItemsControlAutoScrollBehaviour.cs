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

using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ClassicAssist.Shared.UI.Behaviours
{
    public class ItemsControlAutoScrollBehaviour : Behavior<ItemsControl>
    {
        public static readonly DependencyProperty ScrollViewerProperty = DependencyProperty.Register( "ScrollViewer",
            typeof( ScrollViewer ), typeof( ItemsControlAutoScrollBehaviour ),
            new PropertyMetadata( default( ScrollViewer ) ) );

        private INotifyCollectionChanged _incc;

        public ScrollViewer ScrollViewer
        {
            get => (ScrollViewer) GetValue( ScrollViewerProperty );
            set => SetValue( ScrollViewerProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if ( AssociatedObject.Items is INotifyCollectionChanged incc )
            {
                _incc = incc;
                _incc.CollectionChanged += OnCollectionChanged;
            }
        }

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            ScrollViewer scrollViewer = ScrollViewer ?? AssociatedObject.GetChildOfType<ScrollViewer>();

            scrollViewer?.ScrollToEnd();
        }

        protected override void OnDetaching()
        {
            if ( _incc != null )
            {
                _incc.CollectionChanged -= OnCollectionChanged;
            }

            base.OnDetaching();
        }
    }
}