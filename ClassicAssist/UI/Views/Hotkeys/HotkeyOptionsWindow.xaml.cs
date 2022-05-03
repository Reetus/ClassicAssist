// Copyright (C) $CURRENT_YEAR$ Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.Hotkeys
{
    /// <summary>
    ///     Interaction logic for HotkeyOptionsWindow.xaml
    /// </summary>
    public partial class HotkeyOptionsWindow
    {
        private ICommand _okCommand;

        public HotkeyOptionsWindow( HotkeyCommand hotkeyCommand )
        {
            InitializeComponent();

            HotkeyCommand = hotkeyCommand;

            Grid content = (Grid) FindName( "ContentGrid" );

            if ( content == null )
            {
                return;
            }

            IEnumerable<PropertyInfo> properties = hotkeyCommand.GetType().GetProperties().Where( prop =>
                prop.IsDefined( typeof( HotkeyConfigurationAttribute ), false ) ).ToList();

            foreach ( PropertyInfo property in properties )
            {
                object value = property.GetValue( hotkeyCommand );

                Values.Add( property, value );
            }

            foreach ( PropertyInfo property in properties )
            {
                content.RowDefinitions.Add( new RowDefinition { Height = GridLength.Auto } );

                HotkeyConfigurationAttribute attribute = property.GetCustomAttribute<HotkeyConfigurationAttribute>();

                TextBlock textBlock = new TextBlock { Text = attribute.Name, Margin = new Thickness( 0, 0, 10, 0 ) };

                Grid innerGrid = new Grid();
                content.Children.Add( innerGrid );
                Grid.SetRow( innerGrid, content.RowDefinitions.Count - 1 );

                innerGrid.ColumnDefinitions.Add( new ColumnDefinition { Width = GridLength.Auto } );
                innerGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength( 1, GridUnitType.Star ) } );
                innerGrid.Children.Add( textBlock );

                Control control = null;

                switch ( attribute.BaseType.Name )
                {
                    case "Enum":
                        control = CreateComboBox( attribute, property );
                        break;
                    default:
                        // ReSharper disable once LocalizableElement
                        throw new ArgumentOutOfRangeException( attribute.BaseType.Name,
                            $"Unsupported base type: {attribute.BaseType.Name}" );
                }

                if ( control != null )
                {
                    innerGrid.Children.Add( control );
                    Grid.SetColumn( control, 1 );
                }

                Grid.SetColumn( textBlock, 0 );
            }
        }

        public HotkeyCommand HotkeyCommand { get; set; }

        public ICommand OkCommand => _okCommand ?? ( _okCommand = new RelayCommand( Ok ) );

        public Dictionary<PropertyInfo, object> Values { get; set; } = new Dictionary<PropertyInfo, object>();

        private ComboBox CreateComboBox( HotkeyConfigurationAttribute attribute, PropertyInfo property )
        {
            ComboBox combobox = new ComboBox();

            Enum.GetNames( attribute.Type ).ToList().ForEach( item =>
            {
                object enumObject = Enum.Parse( attribute.Type, item );

                DescriptionAttribute descriptionAttribute = enumObject.GetType().GetMember( enumObject.ToString() )
                    .First().GetCustomAttribute<DescriptionAttribute>();

                ComboBoxItem comboBoxItem = new ComboBoxItem
                {
                    Content = descriptionAttribute != null
                        ? GetLocalizedString( descriptionAttribute.Description )
                        : GetLocalizedString( item ),
                    Tag = enumObject
                };
                combobox.Items.Add( comboBoxItem );

                if ( Values.ContainsKey( property ) && Values[property].ToString() == item )
                {
                    combobox.SelectedItem = comboBoxItem;
                }
            } );

            combobox.SelectionChanged += ( sender, args ) =>
            {
                if ( !( combobox.SelectedItem is ComboBoxItem comboBoxItem ) )
                {
                    return;
                }

                if ( Values.ContainsKey( property ) )
                {
                    Values[property] = comboBoxItem.Tag;
                }
            };

            return combobox;
        }

        private void Ok( object obj )
        {
            foreach ( KeyValuePair<PropertyInfo, object> keyValuePair in Values )
            {
                keyValuePair.Key.SetValue( HotkeyCommand, keyValuePair.Value );
            }
        }

        private static string GetLocalizedString( string name )
        {
            string localized = Strings.ResourceManager.GetString( name );

            if ( string.IsNullOrEmpty( localized ) )
            {
                throw new ArgumentNullException( $"No localizable string for {name}" );
            }

            return localized;
        }
    }
}