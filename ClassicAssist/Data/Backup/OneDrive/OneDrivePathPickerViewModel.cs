#region License

// Copyright (C) 2021 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using ClassicAssist.Resources;
using ClassicAssist.UI.ViewModels;
using Microsoft.Graph;

namespace ClassicAssist.Data.Backup.OneDrive
{
    public class OneDrivePathPickerViewModel : SetPropertyNotifyChanged
    {
        private readonly Dispatcher _dispatcher;
        private ICommand _expandCommand;
        private ObservableCollection<OneDriveFolder> _folders = new ObservableCollection<OneDriveFolder>();
        private bool _isWorking;
        private ICommand _saveCommand;
        private OneDriveFolder _selectedItem;

        public OneDrivePathPickerViewModel()
        {
        }

        public OneDrivePathPickerViewModel( Func<string, Task<IEnumerable<DriveItem>>> getChildren )
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            GetChildren = getChildren;

            GetFolders( string.Empty, Folders ).ConfigureAwait( false );
        }

        public DialogResult DialogResult { get; set; } = DialogResult.Cancel;

        public ICommand ExpandCommand =>
            _expandCommand ?? ( _expandCommand = new RelayCommandAsync( ExpandAsync, o => true ) );

        public ObservableCollection<OneDriveFolder> Folders
        {
            get => _folders;
            set => SetProperty( ref _folders, value );
        }

        public Func<string, Task<IEnumerable<DriveItem>>> GetChildren { get; set; }

        public bool IsWorking
        {
            get => _isWorking;
            set => SetProperty( ref _isWorking, value );
        }

        public ICommand SaveCommand =>
            _saveCommand ?? ( _saveCommand = new RelayCommand( Save,
                o => SelectedItem != null && !string.IsNullOrEmpty( SelectedItem.Id ) ) );

        public OneDriveFolder SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        private async Task GetFolders( string id, ICollection<OneDriveFolder> collection )
        {
            try
            {
                IsWorking = true;

                IEnumerable<DriveItem> driveItems = ( await GetChildren( id ) ).Where( f => f.Folder != null ).ToList();

                _dispatcher.Invoke( collection.Clear );

                if ( !driveItems.Any() )
                {
                    return;
                }

                foreach ( DriveItem driveItem in driveItems )
                {
                    OneDriveFolder item = new OneDriveFolder { Id = driveItem.Id, Name = driveItem.Name };

                    if ( !( driveItem.Folder?.ChildCount > 0 ) )
                    {
                        continue;
                    }

                    item.Children.Add( new OneDriveFolder { Name = Strings.Loading___ } );
                    await _dispatcher.InvokeAsync( () => { collection.Add( item ); } );
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        private async Task ExpandAsync( object arg )
        {
            if ( !( arg is OneDriveFolder driveItem ) )
            {
                return;
            }

            await GetFolders( driveItem.Id, driveItem.Children );
        }

        private void Save( object obj )
        {
            DialogResult = DialogResult.OK;
        }
    }
}