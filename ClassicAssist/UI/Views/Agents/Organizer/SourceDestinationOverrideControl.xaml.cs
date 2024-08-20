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
using System.Windows.Input;

namespace ClassicAssist.UI.Views.Agents.Organizer
{
    /// <summary>
    ///     Interaction logic for SourceDestinationOverrideControl.xaml
    /// </summary>
    public partial class SourceDestinationOverrideControl : UserControl
    {
        public static readonly DependencyProperty SerialProperty = DependencyProperty.Register( nameof( Serial ), typeof( int? ), typeof( SourceDestinationOverrideControl ),
            new PropertyMetadata( defaultValue: null ) );

        public static readonly DependencyProperty SetCommandProperty = DependencyProperty.Register( nameof( SetCommand ), typeof( ICommand ),
            typeof( SourceDestinationOverrideControl ), new PropertyMetadata( defaultValue: null ) );

        public static readonly DependencyProperty SetCommandParameterProperty = DependencyProperty.Register( nameof( SetCommandParameter ), typeof( object ),
            typeof( SourceDestinationOverrideControl ), new PropertyMetadata( defaultValue: null ) );

        public static readonly DependencyProperty ClearCommandProperty = DependencyProperty.Register( nameof( ClearCommand ), typeof( ICommand ),
            typeof( SourceDestinationOverrideControl ), new PropertyMetadata( defaultValue: null ) );

        public SourceDestinationOverrideControl()
        {
            InitializeComponent();
        }

        public ICommand ClearCommand
        {
            get => (ICommand) GetValue( ClearCommandProperty );
            set => SetValue( ClearCommandProperty, value );
        }

        public int? Serial
        {
            get => (int?) GetValue( SerialProperty );
            set => SetValue( SerialProperty, value );
        }

        public ICommand SetCommand
        {
            get => (ICommand) GetValue( SetCommandProperty );
            set => SetValue( SetCommandProperty, value );
        }

        public object SetCommandParameter
        {
            get => GetValue( SetCommandParameterProperty );
            set => SetValue( SetCommandParameterProperty, value );
        }
    }
}