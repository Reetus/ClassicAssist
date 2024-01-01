#region License

// Copyright (C) 2023 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClassicAssist.Controls
{
    public partial class ImageButton
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register( nameof( ImageSource ), typeof( ImageSource ), typeof( ImageButton ),
                new PropertyMetadata( null, OnImageSourceChanged ) );

        public ImageButton()
        {
            InitializeComponent();

            Background = Brushes.Transparent;
            BorderThickness = new Thickness( 0 );
            Padding = new Thickness( 0 );
        }

        public ImageSource ImageSource
        {
            get => (ImageSource) GetValue( ImageSourceProperty );
            set => SetValue( ImageSourceProperty, value );
        }

        private static void OnImageSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( d is ImageButton button )
            {
                button.Content = new Image
                {
                    Source = (ImageSource) e.NewValue, Height = button.Height, Width = button.Width
                };
            }
        }
    }
}