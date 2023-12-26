// Copyright (C) 2023 Reetus
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

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     Interaction logic for OptionCheckBox.xaml
    /// </summary>
    public partial class OptionedCheckBoxControl
    {
        public static readonly DependencyProperty ChildContentProperty =
            DependencyProperty.Register( nameof( ChildContent ), typeof( object ), typeof( OptionedCheckBoxControl ),
                new PropertyMetadata( null, OnChildContentChanged ) );

        public OptionedCheckBoxControl()
        {
            InitializeComponent();

            Checked += OnCheckedChanged;
            Unchecked += OnCheckedChanged;
        }

        public object ChildContent
        {
            get => GetValue( ChildContentProperty );
            set => SetValue( ChildContentProperty, value );
        }

        private static void OnChildContentChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            OptionedCheckBoxControl control = (OptionedCheckBoxControl) d;
            control.UpdateContent();
        }

        private void UpdateContent()
        {
            DockPanel dockPanel = new DockPanel { LastChildFill = true };
            ContentPresenter checkBoxPresenter =
                new ContentPresenter { Content = Content, VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock( checkBoxPresenter, Dock.Left );
            dockPanel.Children.Add( checkBoxPresenter );

            ContentPresenter childContentPresenter = new ContentPresenter
            {
                Content = ChildContent, HorizontalAlignment = HorizontalAlignment.Stretch
            };

            DockPanel.SetDock( childContentPresenter, Dock.Right );

            dockPanel.Children.Add( childContentPresenter );

            Content = dockPanel;

            OnCheckedChanged( this, new RoutedEventArgs() );
        }

        private void OnCheckedChanged( object sender, RoutedEventArgs routedEventArgs )
        {
            if ( ChildContent is UIElement childElement )
            {
                childElement.IsEnabled = IsChecked ?? false;
            }
        }
    }
}