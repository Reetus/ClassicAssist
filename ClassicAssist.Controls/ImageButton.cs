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
using System.Windows.Data;
using System.Windows.Media;

namespace ClassicAssist.Controls
{
    public partial class ImageButton : Button
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register( nameof( ImageSource ), typeof( ImageSource ), typeof( ImageButton ),
                new PropertyMetadata( null ) );

        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register( nameof( ImageHeight ), typeof( double ), typeof( ImageButton ),
                new PropertyMetadata( (double) 16 ) );

        public ImageButton()
        {
            Image imageSource = new Image
            {
                Stretch = Stretch.Uniform,
                Source = ImageSource,
            };

            Binding imageBinding = new Binding { Source = this, Path = new PropertyPath( ImageSourceProperty ) };

            imageSource.SetBinding( Image.SourceProperty, imageBinding );

            Binding imageHeightBinding = new Binding { Source = this, Path = new PropertyPath( ImageHeightProperty ) };

            imageSource.SetBinding( HeightProperty, imageHeightBinding );

            Content = imageSource;
        }

        public double ImageHeight
        {
            get => (double) GetValue( ImageHeightProperty );
            set => SetValue( ImageHeightProperty, value );
        }

        public ImageSource ImageSource
        {
            get => (ImageSource) GetValue( ImageSourceProperty );
            set => SetValue( ImageSourceProperty, value );
        }
    }
}