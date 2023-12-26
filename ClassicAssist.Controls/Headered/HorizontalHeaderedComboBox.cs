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

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace ClassicAssist.Controls.Headered
{
    public class HorizontalHeaderedComboBox : HorizontalHeaderedContentControl
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof( SelectedItem ), typeof( object ), typeof( HorizontalHeaderedComboBox ),
            new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof( ItemsSource ), typeof( IEnumerable ), typeof( HorizontalHeaderedComboBox ),
            new FrameworkPropertyMetadata( default( IEnumerable ) ) );

        public HorizontalHeaderedComboBox()
        {
            ComboBox comboBox = new ComboBox();
            Binding binding = new Binding( "SelectedItem" ) { Source = this, Mode = BindingMode.TwoWay };
            Binding itemsSourceBinding = new Binding( "ItemsSource" ) { Source = this };
            comboBox.SetBinding( ItemsControl.ItemsSourceProperty, itemsSourceBinding );
            comboBox.SetBinding( Selector.SelectedItemProperty, binding );
            Content = comboBox;
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable) GetValue( ItemsSourceProperty );
            set => SetValue( ItemsSourceProperty, value );
        }

        public object SelectedItem
        {
            get => GetValue( SelectedItemProperty );
            set => SetValue( SelectedItemProperty, value );
        }
    }
}