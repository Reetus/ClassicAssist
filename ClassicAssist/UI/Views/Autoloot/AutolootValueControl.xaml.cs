// Copyright (C) 2023 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using ClassicAssist.Controls;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.UI.ValueConverters;
using ClassicAssist.UI.Misc;
using ClassicAssist.UI.ViewModels.Agents;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Views.Autoloot
{
    /// <summary>
    ///     Interaction logic for AutolootValueControl.xaml
    /// </summary>
    public partial class AutolootValueControl : UserControl
    {
        public AutolootValueControl()
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

            Grid.Children.Clear();

            if ( autolootConstraintEntry.Property.AllowedValuesEnum != null &&
                 autolootConstraintEntry.Property.AllowedValuesEnum == typeof( Layer ) )
            {
                EnumToIntegerValueConverter enumToIntegerValueConverter = new EnumToIntegerValueConverter();

                Binding selectedItemBinding = new Binding( nameof( autolootConstraintEntry.Value ) )
                {
                    Source = autolootConstraintEntry,
                    Converter = enumToIntegerValueConverter,
                    ConverterParameter = typeof( Layer ),
                    Mode = BindingMode.TwoWay
                };

                EnumBindingSourceExtension itemSource = new EnumBindingSourceExtension( typeof( Layer ) );

                ComboBox comboBox = new ComboBox
                {
                    Width = 90, ItemsSource = itemSource.ProvideValue( null ) as IEnumerable
                };

                BindingOperations.SetBinding( comboBox, Selector.SelectedItemProperty, selectedItemBinding );

                Grid.Children.Add( comboBox );

                return;
            }

            Binding binding = new Binding( "Value" ) { Source = autolootConstraintEntry };

            EditTextBlock editTextBlock = new EditTextBlock { Width = 100, MinWidth = 40, ShowIcon = true };

            BindingOperations.SetBinding( editTextBlock, EditTextBlock.TextProperty, binding );

            Grid.Children.Add( editTextBlock );
        }
    }
}