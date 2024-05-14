#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views.ECV.Settings.Models;

namespace ClassicAssist.UI.Views.ECV.Settings
{
    public class OpenContainersSettingsViewModel : SetPropertyNotifyChanged
    {
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private ICommand _addEntryCommand;
        private ObservableCollection<CombineStacksOpenContainersIgnoreEntry> _items = new ObservableCollection<CombineStacksOpenContainersIgnoreEntry>();
        private ICommand _removeEntryCommand;
        private CombineStacksOpenContainersIgnoreEntry _selectedItem;

        public ICommand AddEntryCommand => _addEntryCommand ?? ( _addEntryCommand = new RelayCommand( AddEntry ) );

        public ObservableCollection<CombineStacksOpenContainersIgnoreEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand RemoveEntryCommand => _removeEntryCommand ?? ( _removeEntryCommand = new RelayCommand( RemoveEntry, o => o != null ) );

        public CombineStacksOpenContainersIgnoreEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        private void RemoveEntry( object obj )
        {
            if ( !( obj is CombineStacksOpenContainersIgnoreEntry entry ) )
            {
                return;
            }

            _dispatcher.Invoke( () => { Items.Remove( entry ); } );
        }

        private void AddEntry( object obj )
        {
            _dispatcher.Invoke( () => Items.Add( new CombineStacksOpenContainersIgnoreEntry() ) );
        }
    }
}