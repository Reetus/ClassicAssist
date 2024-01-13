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
using ClassicAssist.Data.Misc;

namespace ClassicAssist.UI.Views.ECV
{
    /// <summary>
    ///     Interaction logic for EntityCollectionViewerSettingsWindow.xaml
    /// </summary>
    public partial class EntityCollectionViewerSettingsWindow : Window
    {
        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register( nameof( Options ), typeof( EntityCollectionViewerOptions ),
            typeof( EntityCollectionViewerSettingsWindow ), new PropertyMetadata( default( EntityCollectionViewerOptions ), PropertyChangedCallback ) );

        public EntityCollectionViewerSettingsWindow()
        {
            InitializeComponent();
        }

        public EntityCollectionViewerOptions Options
        {
            get => (EntityCollectionViewerOptions) GetValue( OptionsProperty );
            set => SetValue( OptionsProperty, value );
        }

        private static void PropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is EntityCollectionViewerSettingsWindow window ) )
            {
                return;
            }

            if ( e.NewValue is EntityCollectionViewerOptions options )
            {
                window.DataContext = new EntityCollectionViewerSettingsViewModel { Options = options };
            }
        }
    }
}