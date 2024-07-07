// Copyright (C) 2024 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace ClassicAssist.UI.Views.Agents.Autoloot.Import
{
    /// <summary>
    ///     Interaction logic for CSVImportWindow.xaml
    /// </summary>
    public partial class CSVImportWindow : Window
    {
        public CSVImportWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnRequestNavigate( object sender, RequestNavigateEventArgs e )
        {
            Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
        }
    }
}