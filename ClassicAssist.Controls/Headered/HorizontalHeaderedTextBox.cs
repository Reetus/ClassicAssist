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

namespace ClassicAssist.Controls.Headered
{
    public class HorizontalHeaderedTextBox : HorizontalHeaderedContentControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register( nameof( Text ),
            typeof( string ), typeof( HorizontalHeaderedTextBox ),
            new FrameworkPropertyMetadata( default( string ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        public HorizontalHeaderedTextBox()
        {
            TextBox textBox = new TextBox();
            Binding binding = new Binding( "Text" )
            {
                Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            textBox.SetBinding( TextBox.TextProperty, binding );
            Content = textBox;
        }

        public string Text
        {
            get => (string) GetValue( TextProperty );
            set => SetValue( TextProperty, value );
        }
    }
}