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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using ClassicAssist.Controls;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.UI.ValueConverters;
using ClassicAssist.UI.Misc;
using ClassicAssist.UI.Misc.ValueConverters;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Views.Agents.Autoloot
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
            if ( e.OldValue is AutolootConstraintEntry oldEntry )
            {
                oldEntry.PropertyChanged -= PropertyOnPropertyChanged;
            }

            if ( !( e.NewValue is AutolootConstraintEntry autolootConstraintEntry ) )
            {
                return;
            }

            autolootConstraintEntry.PropertyChanged += PropertyOnPropertyChanged;
            PropertyOnPropertyChanged( autolootConstraintEntry, new PropertyChangedEventArgs( nameof( autolootConstraintEntry.Property ) ) );
        }

        private void PropertyOnPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if ( !( sender is AutolootConstraintEntry autolootConstraintEntry ) )
            {
                return;
            }

            if ( e.PropertyName != nameof( autolootConstraintEntry.Property ) )
            {
                return;
            }

            Content = null;

            if ( autolootConstraintEntry.Property.AllowedValuesEnum != null && autolootConstraintEntry.Property.AllowedValuesEnum == typeof( Layer ) )
            {
                EnumToIntegerValueConverter enumToIntegerValueConverter = new EnumToIntegerValueConverter();

                Binding selectedItemBinding = new Binding( nameof( autolootConstraintEntry.Value ) )
                {
                    Source = autolootConstraintEntry, Converter = enumToIntegerValueConverter, ConverterParameter = typeof( Layer ), Mode = BindingMode.TwoWay
                };

                EnumBindingSourceExtension itemSource = new EnumBindingSourceExtension( typeof( Layer ) );

                ComboBox comboBox = new ComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    ItemsSource = itemSource.ProvideValue( null ) as IEnumerable
                };

                BindingOperations.SetBinding( comboBox, Selector.SelectedItemProperty, selectedItemBinding );

                Content = comboBox;

                return;
            }

            Binding binding = new Binding( "Value" ) { Source = autolootConstraintEntry, Converter = new HexToIntValueConverter() };

            EditTextBlock editTextBlock = new EditTextBlock { Width = 100, MinWidth = 40, ShowIcon = true, Foreground = (Brush) FindResource( "ThemeForegroundBrush" ) };

            BindingOperations.SetBinding( editTextBlock, EditTextBlock.TextProperty, binding );

            Content = editTextBlock;
        }
    }
}