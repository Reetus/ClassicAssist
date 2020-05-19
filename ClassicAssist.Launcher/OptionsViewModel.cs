#region License

// Copyright (C) 2020 Reetus
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
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ClassicAssist.Launcher
{
    public class OptionsViewModel : BaseViewModel
    {
        private ICommand _addPluginCommand;
        private ClassicOptions _classicOptions = new ClassicOptions();
        private ICommand _okCommand;
        private ObservableCollection<PluginEntry> _plugins = new ObservableCollection<PluginEntry>();
        private ICommand _removePluginCommand;
        private PluginEntry _selectedPlugin;

        public OptionsViewModel()
        {
        }

        public OptionsViewModel( IEnumerable<PluginEntry> plugins, ClassicOptions classicOptions )
        {
            Plugins = new ObservableCollection<PluginEntry>( plugins );
            ClassicOptions = classicOptions;
        }

        public ICommand AddPluginCommand =>
            _addPluginCommand ?? ( _addPluginCommand = new RelayCommand( AddPlugin, o => true ) );

        public ClassicOptions ClassicOptions
        {
            get => _classicOptions;
            set => SetProperty( ref _classicOptions, value );
        }

        public DialogResult DialogResult { get; set; } = DialogResult.Cancel;

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK, o => true ) );

        public ObservableCollection<PluginEntry> Plugins
        {
            get => _plugins;
            set => SetProperty( ref _plugins, value );
        }

        public ICommand RemovePluginCommand =>
            _removePluginCommand ??
            ( _removePluginCommand = new RelayCommand( RemovePlugin, o => SelectedPlugin != null ) );

        public PluginEntry SelectedPlugin
        {
            get => _selectedPlugin;
            set => SetProperty( ref _selectedPlugin, value );
        }

        private void OK( object obj )
        {
            DialogResult = DialogResult.OK;
        }

        private void AddPlugin( object obj )
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = false,
                InitialDirectory = Environment.CurrentDirectory,
                CheckFileExists = true,
                Filter = "Executable / DLL Files (*.exe, *.dll)|*.dll;*.exe"
            };

            bool? result = ofd.ShowDialog();

            if ( result.HasValue && result.Value )
            {
                Plugins.Add( new PluginEntry { Name = Path.GetFileName( ofd.FileName ), FullPath = ofd.FileName } );
            }
        }

        private void RemovePlugin( object obj )
        {
            if ( !( obj is PluginEntry entry ) )
            {
                return;
            }

            Plugins.Remove( entry );
        }
    }
}