﻿// Copyright (C) 2023 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Windows;

namespace ClassicAssist.Controls.Headered
{
    /// <summary>
    ///     Interaction logic for HorizontalHeaderedContentControl.xaml
    /// </summary>
    public partial class HorizontalHeaderedContentControl
    {
        public static readonly DependencyProperty ChildMarginProperty = DependencyProperty.Register(
            nameof( ChildMargin ), typeof( Thickness ), typeof( HorizontalHeaderedContentControl ),
            new PropertyMetadata( new Thickness( 0, 0, 5, 0 ) ) );

        public HorizontalHeaderedContentControl()
        {
            InitializeComponent();
        }

        public Thickness ChildMargin
        {
            get => (Thickness) GetValue( ChildMarginProperty );
            set => SetValue( ChildMarginProperty, value );
        }
    }
}