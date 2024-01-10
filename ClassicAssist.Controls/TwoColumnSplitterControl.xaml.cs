// Copyright (C) 2023 Reetus
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

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     Interaction logic for TwoColumnSplitterControl.xaml
    /// </summary>
    public partial class TwoColumnSplitterControl : UserControl
    {
        public static readonly DependencyProperty LeftContentProperty =
            DependencyProperty.Register( nameof( LeftContent ), typeof( object ), typeof( TwoColumnSplitterControl ) );

        public static readonly DependencyProperty RightContentProperty =
            DependencyProperty.Register( nameof( RightContent ), typeof( object ), typeof( TwoColumnSplitterControl ) );

        public static readonly DependencyProperty LeftContentWidthProperty =
            DependencyProperty.Register( nameof( LeftContentWidth ), typeof( GridLength ), typeof( TwoColumnSplitterControl ),
                new PropertyMetadata( new GridLength( 1, GridUnitType.Star ) ) );

        public TwoColumnSplitterControl()
        {
            InitializeComponent();
        }

        public object LeftContent
        {
            get => GetValue( LeftContentProperty );
            set => SetValue( LeftContentProperty, value );
        }

        public GridLength LeftContentWidth
        {
            get => (GridLength) GetValue( LeftContentWidthProperty );
            set => SetValue( LeftContentWidthProperty, value );
        }

        public object RightContent
        {
            get => GetValue( RightContentProperty );
            set => SetValue( RightContentProperty, value );
        }
    }
}