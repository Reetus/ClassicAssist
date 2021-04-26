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
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Data.Backup;
using ClassicAssist.Resources;
using Sentry;

namespace ClassicAssist.UI.ViewModels
{
    public class BackupWindowViewModel : SetPropertyNotifyChanged
    {
        private readonly Dispatcher _dispatcher;
        private readonly BackupOptions _options;
        private bool _isWorking;
        private ObservableCollection<string> _messages = new ObservableCollection<string>();

        public BackupWindowViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public BackupWindowViewModel( BackupOptions options )
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _options = options;

            Task.Run( async () =>
            {
                try
                {
                    IsWorking = true;

                    await Backup();
                }
                catch ( Exception e )
                {
                    SentrySdk.CaptureException( e );
                    IsErrored = true;
                }
                finally
                {
                    IsWorking = false;
                }

                if ( IsErrored )
                {
                    return;
                }

                _dispatcher.Invoke( () => { Messages.Add( Strings.Done___ ); } );
                _options.LastBackup = DateTime.Now;

                await Task.Delay( 2000 );

                _dispatcher.Invoke( CloseWindow );
            } ).ConfigureAwait( false );
        }

        public Action CloseWindow { get; set; }

        public bool IsErrored { get; set; }

        public bool IsWorking
        {
            get => _isWorking;
            set => SetProperty( ref _isWorking, value );
        }

        public ObservableCollection<string> Messages
        {
            get => _messages;
            set => SetProperty( ref _messages, value );
        }

        public async Task Backup()
        {
            if ( _options.Provider == null || string.IsNullOrEmpty( _options.Provider.BackupPath ) )
            {
                Messages.Add( Strings.No_backup_type_or_path_set___ );
                return;
            }

            List<string> files = new List<string> { "Assistant.json", "Macros.json" };
            files.AddRange( Directory.EnumerateFiles( Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory,
                "Profiles" ) ) );

            foreach ( string file in files )
            {
                string fullPath = file;

                if ( !Path.IsPathRooted( file ) )
                {
                    fullPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, file );
                }

                if ( !File.Exists( fullPath ) )
                {
                    continue;
                }

                _dispatcher.Invoke( () =>
                {
                    Messages.Add( string.Format( Strings.Backing_up__0____, Path.GetFileName( file ) ) );
                } );

                try
                {
                    bool result = await _options.Provider.Write( fullPath );

                    if ( !result )
                    {
                        throw new Exception( "Unknown error" );
                    }
                }
                catch ( Exception e )
                {
                    _dispatcher.Invoke( () =>
                    {
                        Messages.Add( string.Format( Strings.Failed_to_backup_file___0_, e.Message ) );
                    } );

                    throw;
                }
            }
        }
    }
}