// Copyright (C) 2024 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Windows;
using ClassicAssist.Shared.UI;
using System.Collections.ObjectModel;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.Views.ECV
{
    /// <summary>
    ///     Interaction logic for EntityCollectionViewerOrganizerControl.xaml
    /// </summary>
    public partial class EntityCollectionViewerOrganizerControl
    {
        public static readonly DependencyProperty CollectionProperty = DependencyProperty.Register( nameof( Collection ), typeof( ItemCollection ),
            typeof( EntityCollectionViewerOrganizerControl ),
            new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback ) );

        public static readonly DependencyProperty QueueActionsProperty = DependencyProperty.Register( nameof( QueueActions ), typeof( ObservableCollection<QueueAction> ),
            typeof( EntityCollectionViewerOrganizerControl ),
            new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback ) );

        public EntityCollectionViewerOrganizerControl()
        {
            InitializeComponent();
        }

        public ItemCollection Collection
        {
            get => (ItemCollection) GetValue( CollectionProperty );
            set => SetValue( CollectionProperty, value );
        }

        public ObservableCollection<QueueAction> QueueActions
        {
            get => (ObservableCollection<QueueAction>) GetValue( QueueActionsProperty );
            set => SetValue( QueueActionsProperty, value );
        }

        private static void PropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is EntityCollectionViewerOrganizerControl control ) )
            {
                return;
            }

            if ( !( control.DataContext is EntityCollectionViewerOrganizerViewModel viewModel ) )
            {
                return;
            }

            if ( e.Property == CollectionProperty )
            {
                viewModel.Collection = (ItemCollection) e.NewValue;
            }
            else if ( e.Property == QueueActionsProperty )
            {
                viewModel.QueueActions = (ObservableCollection<QueueAction>) e.NewValue;
            }
        }
    }
}