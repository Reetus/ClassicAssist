// Copyright (C) 2023 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.UI.ViewModels.Agents;

namespace ClassicAssist.UI.Views.Agents.Autoloot
{
    /// <summary>
    ///     Interaction logic for SkillBonusSelector.xaml
    /// </summary>
    public partial class AutolootSkillBonusSelector : UserControl
    {
        public AutolootSkillBonusSelector()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            // TODO: Convert to XAML
            if ( !( e.NewValue is AutolootConstraintEntry autolootConstraintEntry ) )
            {
                return;
            }

            Binding selectedItemBinding = new Binding( nameof( autolootConstraintEntry.Additional ) ) { Source = autolootConstraintEntry, Mode = BindingMode.TwoWay };

            List<string> source = ( from object value in typeof( SkillBonusSkills ).GetEnumValues()
                let fieldInfo = typeof( SkillBonusSkills ).GetField( value.ToString() )
                let attr = (DescriptionAttribute) fieldInfo.GetCustomAttributes( typeof( DescriptionAttribute ), false ).FirstOrDefault()
                select attr == null ? value.ToString() : attr.Description ).ToList();

            ComboBox comboBox = new ComboBox { ItemsSource = source, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness( 10, 0, 0, 0 ), SelectedIndex = 0 };

            BindingOperations.SetBinding( comboBox, Selector.SelectedItemProperty, selectedItemBinding );

            Content = comboBox;
        }
    }
}