// Copyright (C) 2024 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels.Agents;

namespace ClassicAssist.UI.Views.Agents.Autoloot
{
    /// <summary>
    ///     Interaction logic for GroupControl.xaml
    /// </summary>
    public partial class AutolootPropertyGroupControl : INotifyPropertyChanged
    {
        private ICommand _addEntryCommand;
        private ICommand _removeEntryCommand;
        private bool _showOperator;
        private ICommand _addGroupCommand;

        public AutolootPropertyGroupControl()
        {
            InitializeComponent();

            DataContextChanged += ( sender, args ) =>
            {
                if ( !( DataContext is AutolootPropertyGroup group ) )
                {
                    return;
                }

                group.Children.CollectionChanged += CollectionChanged;
                CollectionChanged( group.Children, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
            };
        }

        private void CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( sender is ObservableCollection<AutolootBaseModel> collection )
            {
                ShowOperator = collection.Count > 1;
            }
        }

        public ICommand AddEntryCommand => _addEntryCommand ?? ( _addEntryCommand = new RelayCommand( AddEntry ) );
        public ICommand AddGroupCommand => _addGroupCommand ?? ( _addGroupCommand = new RelayCommand( AddGroup ) );

        private void AddGroup( object obj )
        {
            if ( !( obj is AutolootViewModel vm ) )
            {
                return;
            }
            if ( !( DataContext is AutolootPropertyGroup group ) )
            {
                return;
            }
            AutolootPropertyGroup newGroup = new AutolootPropertyGroup();
            group.Children.Add( newGroup );
        }

        public ICommand RemoveEntryCommand => _removeEntryCommand ?? ( _removeEntryCommand = new RelayCommand( RemoveEntry ) );

        public bool ShowOperator
        {
            get => _showOperator;
            set => SetField( ref _showOperator, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RemoveEntry( object obj )
        {
            AutolootPropertyGroupControl parentAutolootPropertyGroup = GetParentOfType<AutolootPropertyGroupControl>( this );

            if ( parentAutolootPropertyGroup != null )
            {
                if ( !( parentAutolootPropertyGroup.DataContext is AutolootPropertyGroup propertyGroup ) )
                {
                    return;
                }

                propertyGroup.Children.Remove( DataContext as AutolootBaseModel );
            }
            else
            {
                PropertyGroupControl parent = GetParentOfType<PropertyGroupControl>( this );

                if ( parent == null )
                {
                    return;
                }

                if ( parent.DataContext is AutolootViewModel vm )
                {
                    vm.SelectedItem.Children.Remove( DataContext as AutolootBaseModel );
                }
            }
        }

        private void AddEntry( object obj )
        {
            if ( !( obj is AutolootViewModel vm ) )
            {
                return;
            }

            if ( !( DataContext is AutolootPropertyGroup group ) )
            {
                return;
            }

            AutolootPropertyEntry entry = new AutolootPropertyEntry();

            entry.Constraints.Add( new AutolootConstraintEntry { Property = vm.Constraints.LastOrDefault() } );

            group.Children.Add( entry );
        }

        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected bool SetField<T>( ref T field, T value, [CallerMemberName] string propertyName = null )
        {
            if ( EqualityComparer<T>.Default.Equals( field, value ) )
            {
                return false;
            }

            field = value;
            OnPropertyChanged( propertyName );
            return true;
        }

        public static T GetParentOfType<T>( DependencyObject child ) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent( child );

            while ( parent != null )
            {
                if ( parent is T typedParent )
                {
                    return typedParent;
                }

                parent = VisualTreeHelper.GetParent( parent );
            }

            return null;
        }

        private void AddButtonOnClick( object sender, RoutedEventArgs e )
        {
            Button button = sender as Button;

            if ( !( DataContext is AutolootPropertyGroup group ) )
            {
                return;
            }

            if ( group.Children.Count > 0 )
            {
                AutolootBaseModel entry = group.Children.FirstOrDefault( child => child is AutolootPropertyEntry );

                if ( entry != null )
                {
                    AutolootPropertyGroup newGroup = new AutolootPropertyGroup();
                    newGroup.Children.Add( entry );
                    group.Children.Remove( entry );
                    group.Children.Add( newGroup );
                }

                group.Children.Add( new AutolootPropertyGroup() );
            }
            else
            {
                if ( button?.ContextMenu == null )
                {
                    return;
                }

                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}