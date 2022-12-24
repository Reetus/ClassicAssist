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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using Microsoft.Win32;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugAssembliesViewModel : BaseViewModel
    {
        private ObservableCollection<Assembly> _items = new ObservableCollection<Assembly>();
        private ICommand _loadCommand;
        private ICommand _removeCommand;
        private ICommand _saveCommand;

        public DebugAssembliesViewModel()
        {
            if ( AssistantOptions.Assemblies == null )
            {
                return;
            }

            foreach ( string assemblyName in AssistantOptions.Assemblies )
            {
                Assembly assembly = LoadAssembly( assemblyName );

                if ( assembly != null )
                {
                    Items.Add( assembly );
                }
            }
        }

        public ObservableCollection<Assembly> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand LoadCommand => _loadCommand ?? ( _loadCommand = new RelayCommandAsync( Load, o => true ) );

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommandAsync( Remove, o => true ) );

        public ICommand SaveCommand => _saveCommand ?? ( _saveCommand = new RelayCommandAsync( Save, o => true ) );

        public Assembly SelectedItem { get; set; }

        private async Task Load( object arg )
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = Engine.StartupPath, Filter = "DLL Files|*.dll", CheckFileExists = true
            };

            bool? result = ofd.ShowDialog();

            if ( result.HasValue && result.Value )
            {
                try
                {
                    Assembly assembly = LoadAssembly( ofd.FileName );

                    if ( assembly != null )
                    {
                        await _dispatcher.InvokeAsync( () => Items.Add( assembly ) );
                    }
                }
                catch ( Exception e )
                {
                    MessageBox.Show( string.Format( Strings.Error_loading_assembly___0_, e.Message ) );
                }
            }
        }

        private static Assembly LoadAssembly( string fileName )
        {
            if ( !File.Exists( fileName ) )
            {
                return null;
            }

            return Assembly.LoadFile( fileName );
        }

        private async Task Remove( object arg )
        {
            if ( !( arg is Assembly assembly ) )
            {
                return;
            }

            await _dispatcher.InvokeAsync( () => Items.Remove( assembly ) );
        }

        private async Task Save( object arg )
        {
            AssistantOptions.Assemblies = Items.Select( a => a.Location ).ToArray();

            MacroInvoker.ResetImportCache();

            await Task.CompletedTask;
        }
    }
}