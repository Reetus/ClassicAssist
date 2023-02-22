#region License

// Copyright (C) 2023 Reetus
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
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Filters;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views;
using ClassicAssist.UI.Views.Filters.ItemIDFilter;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.ViewModels.Filters
{
    public class ItemIDFilterConfigureWindowViewModel : BaseViewModel
    {
        private ICommand _addCommand;
        private ObservableCollection<ItemIDFilterEntry> _items = new ObservableCollection<ItemIDFilterEntry>();
        private ICommand _removeCommand;
        private ICommand _selectDestinationIDCommand;
        private ItemIDFilterEntry _selectedItem;
        private ICommand _selectSourceIDCommand;
        private ICommand _targetDestinationIDCommand;
        private ICommand _targetSourceIDCommand;
        private ICommand _selectHueCommand;

        public ItemIDFilterConfigureWindowViewModel( ObservableCollection<ItemIDFilterEntry> items )
        {
            Items = items;
        }

        public ItemIDFilterConfigureWindowViewModel()
        {
            Items.Add( new ItemIDFilterEntry() );
        }

        public ICommand AddCommand => _addCommand ?? ( _addCommand = new RelayCommand( Add, o => true ) );

        public ObservableCollection<ItemIDFilterEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => o != null ) );

        public ICommand SelectDestinationIDCommand =>
            _selectDestinationIDCommand ??
            ( _selectDestinationIDCommand = new RelayCommand( SelectDestinationID, o => o != null ) );

        public ItemIDFilterEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ICommand SelectSourceIDCommand =>
            _selectSourceIDCommand ?? ( _selectSourceIDCommand = new RelayCommand( SelectSourceID, o => o != null ) );

        public ICommand TargetDestinationIDCommand =>
            _targetDestinationIDCommand ?? ( _targetDestinationIDCommand =
                new RelayCommandAsync( TargetDestinationID, o => o != null && Engine.Connected ) );

        public ICommand TargetSourceIDCommand =>
            _targetSourceIDCommand ?? ( _targetSourceIDCommand =
                new RelayCommandAsync( TargetSourceID, o => o != null && Engine.Connected ) );

        private static async Task TargetDestinationID( object arg )
        {
            if ( !( arg is ItemIDFilterEntry entry ) )
            {
                return;
            }

            ( TargetType _, TargetFlags _, int _, int _, int _, int _, int itemID ) =
                await Commands.GetTargetInfoAsync();

            entry.DestinationID = itemID;
        }

        private static void SelectDestinationID( object obj )
        {
            if ( !( obj is ItemIDFilterEntry entry ) )
            {
                return;
            }

            ItemIDSelectionWindow window = new ItemIDSelectionWindow();

            window.ShowDialog();

            if ( window.Result )
            {
                entry.DestinationID = window.SelectedItem.ID;
            }
        }

        private static async Task TargetSourceID( object arg )
        {
            if ( !( arg is ItemIDFilterEntry entry ) )
            {
                return;
            }

            ( TargetType _, TargetFlags _, int _, int _, int _, int _, int itemID ) =
                await Commands.GetTargetInfoAsync();

            entry.SourceID = itemID;
        }

        public ICommand SelectHueCommand =>
            _selectHueCommand ?? ( _selectHueCommand = new RelayCommand( SelectHue, o => o != null ) );

        private static void SelectHue( object obj )
        {
            if ( !( obj is ItemIDFilterEntry entry ) )
            {
                return;
            }

            if ( HuePickerWindow.GetHue( out int hue ) )
            {
                entry.Hue = hue;
            }
        }

        private static void SelectSourceID( object obj )
        {
            if ( !( obj is ItemIDFilterEntry entry ) )
            {
                return;
            }

            ItemIDSelectionWindow window = new ItemIDSelectionWindow();

            window.ShowDialog();

            if ( window.Result )
            {
                entry.SourceID = window.SelectedItem.ID;
            }
        }

        private void Add( object obj )
        {
            Items.Add( new ItemIDFilterEntry() );
        }

        private void Remove( object obj )
        {
            if ( !( obj is ItemIDFilterEntry entry ) )
            {
                return;
            }

            Items.Remove( entry );
        }
    }
}