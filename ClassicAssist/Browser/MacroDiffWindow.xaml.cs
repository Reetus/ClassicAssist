// Copyright (C) $CURRENT_YEAR$ Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Windows;
using System.Windows.Input;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Browser
{
    /// <summary>
    ///     Interaction logic for MacroDiffWindow.xaml
    /// </summary>
    public partial class MacroDiffWindow : Window
    {
        public static readonly DependencyProperty OldTextProperty =
            DependencyProperty.Register( nameof( OldText ), typeof( string ), typeof( MacroDiffWindow ) );

        public static readonly DependencyProperty NewTextProperty =
            DependencyProperty.Register( nameof( NewText ), typeof( string ), typeof( MacroDiffWindow ) );

        private ICommand _okCommand;

        public MacroDiffWindow()
        {
            InitializeComponent();
        }

        public string NewText
        {
            get => (string) GetValue( NewTextProperty );
            set => SetValue( NewTextProperty, value );
        }

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK ) );

        public string OldText
        {
            get => (string) GetValue( OldTextProperty );
            set => SetValue( OldTextProperty, value );
        }

        private void OK( object obj )
        {
            DialogResult = true;
        }
    }
}