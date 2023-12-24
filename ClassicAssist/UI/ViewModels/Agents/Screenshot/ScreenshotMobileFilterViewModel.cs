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
using System.Windows.Forms;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Screenshot;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.ViewModels.Agents.Screenshot
{
    public class ScreenshotMobileFilterViewModel : BaseViewModel
    {
        private ICommand _addCommand;
        private bool _enabled;

        private ObservableCollection<ScreenshotMobileFilterEntry> _items =
            new ObservableCollection<ScreenshotMobileFilterEntry>();

        private ICommand _okCommand;

        private ICommand _removeCommand;

        private ScreenshotMobileFilterEntry _selectedItem;
        private ICommand _targetCommand;

        public ICommand AddCommand => _addCommand ?? ( _addCommand = new RelayCommand( Add ) );
        public DialogResult DialogResult { get; set; } = DialogResult.Cancel;

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public ObservableCollection<ScreenshotMobileFilterEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand OkCommand => _okCommand ?? ( _okCommand = new RelayCommand( Ok, o => true ) );

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => SelectedItem != null ) );

        public ScreenshotMobileFilterEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ICommand TargetCommand =>
            _targetCommand ?? ( _targetCommand = new RelayCommandAsync( Target, o => Engine.Connected ) );

        private void Remove( object obj )
        {
            if ( SelectedItem != null )
            {
                _dispatcher.Invoke( () => { Items.Remove( SelectedItem ); } );
            }
        }

        private void Add( object obj )
        {
            _dispatcher.Invoke( () => { Items.Add( new ScreenshotMobileFilterEntry() ); } );
        }

        private void Ok( object obj )
        {
            DialogResult = DialogResult.OK;
        }

        private async Task Target( object arg )
        {
            ( TargetType _, TargetFlags _, int serial, int _, int _, int _, int itemID ) =
                await Commands.GetTargetInfoAsync();

            if ( UOMath.IsMobile( serial ) )
            {
                Mobile mobile = Engine.Mobiles.GetMobile( serial );

                string name = mobile?.Name ?? "Unknown";

                _dispatcher.Invoke( () =>
                {
                    Items.Add( new ScreenshotMobileFilterEntry { ID = itemID, Note = name } );
                } );
            }
        }
    }
}