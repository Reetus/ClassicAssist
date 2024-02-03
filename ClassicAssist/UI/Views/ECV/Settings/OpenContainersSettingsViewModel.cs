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
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.Views.ECV.Settings
{
    public class OpenContainersSettingsViewModel : SetPropertyNotifyChanged
    {
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private ICommand _addEntryCommand;
        private ObservableCollection<OpenContainersIgnoreEntry> _items = new ObservableCollection<OpenContainersIgnoreEntry>();
        private ICommand _removeEntryCommand;
        private OpenContainersIgnoreEntry _selectedItem;
        private ICommand _targetCommand;

        public ICommand AddEntryCommand => _addEntryCommand ?? ( _addEntryCommand = new RelayCommand( AddEntry ) );

        public ObservableCollection<OpenContainersIgnoreEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand RemoveEntryCommand => _removeEntryCommand ?? ( _removeEntryCommand = new RelayCommand( RemoveEntry, o => o != null ) );

        public OpenContainersIgnoreEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ICommand TargetCommand => _targetCommand ?? ( _targetCommand = new RelayCommandAsync( Target, o => Engine.Connected ) );

        private async Task Target( object arg )
        {
            if ( !( arg is OpenContainersIgnoreEntry entry ) )
            {
                return;
            }

            ( TargetType _, TargetFlags _, int serial, int _, int _, int _, int itemId ) = await Commands.GetTargetInfoAsync();

            if ( serial <= 0 )
            {
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            entry.ID = item?.ID ?? itemId;
        }

        private void RemoveEntry( object obj )
        {
            if ( !( obj is OpenContainersIgnoreEntry entry ) )
            {
                return;
            }

            _dispatcher.Invoke( () => { Items.Remove( entry ); } );
        }

        private void AddEntry( object obj )
        {
            _dispatcher.Invoke( () => Items.Add( new OpenContainersIgnoreEntry() ) );
        }
    }
}