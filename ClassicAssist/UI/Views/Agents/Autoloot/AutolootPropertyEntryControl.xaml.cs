// Copyright (C) 2024 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels.Agents;

namespace ClassicAssist.UI.Views.Agents.Autoloot
{
    /// <summary>
    ///     Interaction logic for EntryControl.xaml
    /// </summary>
    public partial class AutolootPropertyEntryControl : INotifyPropertyChanged
    {
        private ICommand _addEntryCommand;
        private ICommand _removeEntryCommand;

        public AutolootPropertyEntryControl()
        {
            InitializeComponent();
        }

        public ICommand AddEntryCommand => _addEntryCommand ?? ( _addEntryCommand = new RelayCommand( AddEntry ) );
        public ICommand RemoveEntryCommand => _removeEntryCommand ?? ( _removeEntryCommand = new RelayCommand( RemoveEntry, CanRemoveEntry ) );

        public event PropertyChangedEventHandler PropertyChanged;

        private bool CanRemoveEntry( object arg )
        {
            if ( !( arg is AutolootViewModel vm ) )
            {
                return false;
            }

            if ( !( DataContext is AutolootPropertyEntry alpe ) )
            {
                return false;
            }

            return vm.SelectedProperty != null && alpe?.Constraints != null && alpe.Constraints.Contains( vm.SelectedProperty );
        }

        private void RemoveEntry( object obj )
        {
            if ( !( DataContext is AutolootPropertyEntry alpe ) )
            {
                return;
            }

            if ( !( obj is AutolootViewModel vm ) )
            {
                return;
            }

            alpe.Constraints.Remove( vm.SelectedProperty );
        }

        private void AddEntry( object obj )
        {
            if ( !( DataContext is AutolootPropertyEntry alpe ) )
            {
                return;
            }

            if ( !( obj is AutolootViewModel vm ) )
            {
                return;
            }

            var lastConstraint = vm.Constraints.ToList().LastOrDefault();
            alpe.Constraints.Add( new AutolootConstraintEntry { Property = lastConstraint } );
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
    }
}