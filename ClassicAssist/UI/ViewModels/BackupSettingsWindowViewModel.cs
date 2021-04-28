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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Data.Backup;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.ViewModels
{
    public class BackupSettingsWindowViewModel : SetPropertyNotifyChanged
    {
        private BackupOptions _backupOptions;
        private ICommand _browsePathCommand;
        private ObservableCollection<IBackupProvider> _providers = new ObservableCollection<IBackupProvider>();

        public BackupSettingsWindowViewModel()
        {
            BackupOptions = AssistantOptions.BackupOptions;

            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where( t =>
                typeof( IBackupProvider ).IsAssignableFrom( t ) && t.IsClass && !t.IsAbstract );

            Providers.Add( BackupOptions.Provider );

            foreach ( Type type in types.Where( e => e != BackupOptions.Provider.GetType() ) )
            {
                IBackupProvider provider = (IBackupProvider) Activator.CreateInstance( type );
                Providers.Add( provider );
            }
        }

        public BackupOptions BackupOptions
        {
            get => _backupOptions;
            set => SetProperty( ref _backupOptions, value );
        }

        public ICommand BrowsePathCommand =>
            _browsePathCommand ?? ( _browsePathCommand =
                new RelayCommandAsync( BrowsePath, o => BackupOptions.Provider.IsLoggedIn ) );

        public ObservableCollection<IBackupProvider> Providers
        {
            get => _providers;
            set => SetProperty( ref _providers, value );
        }

        private async Task BrowsePath( object obj )
        {
            BackupOptions.Provider.BackupPath =
                await BackupOptions.Provider.GetPath( BackupOptions.Provider.BackupPath );
        }
    }
}