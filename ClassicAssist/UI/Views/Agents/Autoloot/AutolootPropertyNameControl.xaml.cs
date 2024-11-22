// Copyright (C) 2023 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.ObjectModel;
using System.Windows;
using ClassicAssist.Data.Autoloot;

namespace ClassicAssist.UI.Views.Agents.Autoloot
{
    /// <summary>
    ///     Interaction logic for AutolootPropertyControl.xaml
    /// </summary>
    public partial class AutolootPropertyNameControl
    {
        public static readonly DependencyProperty ConstraintsProperty = DependencyProperty.Register( nameof( Constraints ), typeof( ObservableCollection<PropertyEntry> ),
            typeof( AutolootPropertyNameControl ), new PropertyMetadata( default ) );

        public AutolootPropertyNameControl()
        {
            InitializeComponent();
        }

        public ObservableCollection<PropertyEntry> Constraints
        {
            get => (ObservableCollection<PropertyEntry>) GetValue( ConstraintsProperty );
            set => SetValue( ConstraintsProperty, value );
        }
    }
}