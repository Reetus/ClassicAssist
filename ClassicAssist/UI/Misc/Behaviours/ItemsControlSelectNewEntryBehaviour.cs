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
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class ItemsControlSelectNewEntryBehaviour : Behavior<ItemsControl>
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register( nameof( SelectedItem ), typeof( object ),
            typeof( ItemsControlSelectNewEntryBehaviour ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register( nameof( Type ), typeof( Type ), typeof( ItemsControlSelectNewEntryBehaviour ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public object SelectedItem
        {
            get => GetValue( SelectedItemProperty );
            set => SetValue( SelectedItemProperty, value );
        }

        public Type Type
        {
            get => (Type) GetValue( TypeProperty );
            set => SetValue( TypeProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            ( (INotifyCollectionChanged) AssociatedObject.Items ).CollectionChanged += OnCollectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            ( (INotifyCollectionChanged) AssociatedObject.Items ).CollectionChanged -= OnCollectionChanged;
        }

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( !( sender is ItemCollection ) )
            {
                return;
            }

            if ( e.NewItems == null )
            {
                return;
            }

            object firstItem = e.NewItems[0];

            if ( Type != null )
            {
                if ( firstItem.GetType() != Type )
                {
                    return;
                }
            }

            SelectedItem = firstItem;
        }
    }
}