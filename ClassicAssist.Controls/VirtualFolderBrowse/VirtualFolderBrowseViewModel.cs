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
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Controls.VirtualFolderBrowse
{
    public class VirtualFolderBrowseViewModel : SetPropertyNotifyChanged
    {
        private readonly Dispatcher _dispatcher;
        private ICommand _expandCommand;
        private ObservableCollection<VirtualFolder> _folders = new ObservableCollection<VirtualFolder>();
        private bool _isWorking;
        private ICommand _newFolderCommand;
        private ICommand _saveCommand;
        private VirtualFolder _selectedItem;

        public VirtualFolderBrowseViewModel()
        {
        }

        public VirtualFolderBrowseViewModel( Func<string, Task<IEnumerable<VirtualFolder>>> getChildren,
            Func<VirtualFolder, string, Task<VirtualFolder>> createFolder )
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            GetChildren = getChildren;
            CreateFolder = createFolder;

            GetFolders( string.Empty, Folders ).ConfigureAwait( false );
        }

        public Func<VirtualFolder, string, Task<VirtualFolder>> CreateFolder { get; set; }

        public DialogResult DialogResult { get; set; } = DialogResult.Cancel;

        public ICommand ExpandCommand =>
            _expandCommand ?? ( _expandCommand = new RelayCommandAsync( ExpandAsync, o => true ) );

        public ObservableCollection<VirtualFolder> Folders
        {
            get => _folders;
            set => SetProperty( ref _folders, value );
        }

        public Func<string, Task<IEnumerable<VirtualFolder>>> GetChildren { get; set; }

        public bool IsWorking
        {
            get => _isWorking;
            set => SetProperty( ref _isWorking, value );
        }

        public ICommand NewFolderCommand =>
            _newFolderCommand ?? ( _newFolderCommand = new RelayCommandAsync( NewFolder, o => CreateFolder != null && !IsWorking) );

        public ICommand SaveCommand =>
            _saveCommand ?? ( _saveCommand = new RelayCommand( Save,
                o => SelectedItem != null && !string.IsNullOrEmpty( SelectedItem.Id ) ) );

        public VirtualFolder SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        private async Task GetFolders( string id, ICollection<VirtualFolder> collection )
        {
            try
            {
                IsWorking = true;

                IEnumerable<VirtualFolder> driveItems = ( await GetChildren( id ) ).ToList();

                _dispatcher.Invoke( collection.Clear );

                if ( !driveItems.Any() )
                {
                    return;
                }

                foreach ( VirtualFolder driveItem in driveItems )
                {
                    VirtualFolder item = new VirtualFolder { Id = driveItem.Id, Name = driveItem.Name };

                    item.Children.Add( new VirtualFolder { Name = Strings.Loading___ } );
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
            if ( !( arg is VirtualFolder driveItem ) )
            {
                return;
            }

            await GetFolders( driveItem.Id, driveItem.Children );
        }

        private async Task NewFolder( object arg )
        {
            try
            {
                IsWorking = true;

                FolderPromptWindow window = new FolderPromptWindow();

                window.ShowDialog();

                if ( window.DialogResult == true )
                {
                    VirtualFolder result = await CreateFolder( SelectedItem, window.Text );

                    if ( result != null )
                    {
                        await GetFolders( SelectedItem?.Id, SelectedItem?.Children ?? Folders );
                    }
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        private void Save( object obj )
        {
            DialogResult = DialogResult.OK;
        }
    }
}