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

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Organizer;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.Views.ECV
{
    public class EntityCollectionViewerOrganizerViewModel : SetPropertyNotifyChanged
    {
        private readonly Dispatcher _dispatcher;
        private readonly OrganizerManager _manager;
        private ItemCollection _collection;
        private ObservableCollectionEx<OrganizerEntry> _items;
        private ICommand _playCommand;
        private ObservableCollectionEx<QueueAction> _queueActions;
        private string _selectedDestination;
        private OrganizerEntry _selectedEntry;
        private ICommand _targetCommand;

        public EntityCollectionViewerOrganizerViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _manager = OrganizerManager.GetInstance();
            Items = _manager.Items;
        }

        public ItemCollection Collection
        {
            get => _collection;
            set => SetProperty( ref _collection, value );
        }

        public ObservableCollection<string> Destinations { get; set; } = new ObservableCollection<string> { "backpack", "bank" };

        public ObservableCollectionEx<OrganizerEntry> Items
        {
            get => _items;
            set { _dispatcher.Invoke( () => { SetProperty( ref _items, value ); } ); }
        }

        public ICommand PlayCommand => _playCommand ?? ( _playCommand = new RelayCommand( Play, o => Engine.Connected ) );

        public ObservableCollectionEx<QueueAction> QueueActions
        {
            get => _queueActions;
            set => SetProperty( ref _queueActions, value );
        }

        public string SelectedDestination
        {
            get => _selectedDestination;
            set => SetProperty( ref _selectedDestination, value );
        }

        public OrganizerEntry SelectedEntry
        {
            get => _selectedEntry;
            set => SetProperty( ref _selectedEntry, value );
        }

        public ICommand TargetCommand => _targetCommand ?? ( _targetCommand = new RelayCommandAsync( Target, o => Engine.Connected && !_manager.IsOrganizing ) );

        private void Play( object arg )
        {
            if ( SelectedEntry == null )
            {
                return;
            }

            int serial = AliasCommands.GetAlias( SelectedDestination );

            if ( serial == 0 )
            {
                serial = Convert.ToInt32( SelectedDestination );
            }

            OrganizerEntry entry = SelectedEntry;

            QueueActions.Add( new QueueAction
            {
                Action = async action =>
                {
                    await _manager.Organize( entry, Collection, destinationContainer: serial, token: action.CancellationTokenSource.Token );
                    return true;
                },
                CancellationTokenSource = new CancellationTokenSource(),
                Status = $"{Strings.Organizer} - {SelectedEntry.Name}"
            } );
        }

        private async Task Target( object obj )
        {
            ( TargetType _, TargetFlags _, int serial, int _, int _, int _, int _ ) = await Commands.GetTargetInfoAsync();

            if ( serial <= 0 )
            {
                return;
            }

            Destinations.Add( serial.ToString() );
            SelectedDestination = serial.ToString();
        }
    }
}