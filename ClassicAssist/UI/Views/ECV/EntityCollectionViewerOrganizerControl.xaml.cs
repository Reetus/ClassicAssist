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
using System.Windows.Controls;
using ItemCollection = ClassicAssist.UO.Objects.ItemCollection;

namespace ClassicAssist.UI.Views.ECV
{
    /// <summary>
    ///     Interaction logic for EntityCollectionViewerOrganizerControl.xaml
    /// </summary>
    public partial class EntityCollectionViewerOrganizerControl : UserControl
    {
        public static readonly DependencyProperty CollectionProperty = DependencyProperty.Register( nameof( Collection ), typeof( ItemCollection ),
            typeof( EntityCollectionViewerOrganizerControl ), new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback) );

        public EntityCollectionViewerOrganizerControl()
        {
            InitializeComponent();
        }

        public ItemCollection Collection
        {
            get => (ItemCollection) GetValue( CollectionProperty );
            set => SetValue( CollectionProperty, value );
        }

        private static void PropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is EntityCollectionViewerOrganizerControl control ) )
            {
                return;
            }

            if ( control.DataContext is EntityCollectionViewerOrganizerViewModel viewModel )
            {
                viewModel.Collection = (ItemCollection) e.NewValue;
            }
        }
    }
}