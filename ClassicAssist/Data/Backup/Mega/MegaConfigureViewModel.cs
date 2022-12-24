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
using System.Threading.Tasks;
using System.Windows.Input;
using CG.Web.MegaApiClient;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Backup.Mega
{
    public class MegaConfigureViewModel : SetPropertyNotifyChanged
    {
        private readonly MegaApiClient _client = new MegaApiClient();
        private string _errorMessage;
        private bool _isLoggedIn;
        private bool _isWorking;
        private ICommand _loginCommand;
        private ICommand _logoutCommand;
        private bool _overwriteExisting;
        private string _password;
        private string _username;

        public MegaConfigureViewModel()
        {
            Task.Run( async () =>
            {
                try
                {
                    IsWorking = true;

                    if ( AssistantOptions.BackupOptions.Provider is MegaBackupProvider provider &&
                         provider.Authentication != null )
                    {
                        OverwriteExisting = provider.OverwriteExisting;

                        _client.Login( new MegaApiClient.LogonSessionToken( provider.Authentication.SessionId,
                            provider.Authentication.MasterKey ) );

                        IAccountInformation info = await _client.GetAccountInformationAsync();

                        IsLoggedIn = true;
                    }
                }
                catch ( Exception )
                {
                    IsLoggedIn = false;
                }
                finally
                {
                    IsWorking = false;
                }
            } );
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty( ref _errorMessage, value );
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                SetProperty( ref _isLoggedIn, value );

                if ( AssistantOptions.BackupOptions?.Provider != null )
                {
                    AssistantOptions.BackupOptions.Provider.IsLoggedIn = value;
                }
            }
        }

        public bool IsWorking
        {
            get => _isWorking;
            set => SetProperty( ref _isWorking, value );
        }

        public ICommand LoginCommand =>
            _loginCommand ?? ( _loginCommand = new RelayCommandAsync( Login, o => !IsWorking ) );

        public ICommand LogoutCommand =>
            _logoutCommand ?? ( _logoutCommand = new RelayCommandAsync( Logout, o => !IsWorking ) );

        public bool OverwriteExisting
        {
            get => _overwriteExisting;
            set
            {
                SetProperty( ref _overwriteExisting, value );

                if ( AssistantOptions.BackupOptions?.Provider is MegaBackupProvider provider )
                {
                    provider.OverwriteExisting = value;
                }
            }
        }

        public string Password
        {
            get => _password;
            set => SetProperty( ref _password, value );
        }

        public string Username
        {
            get => _username;
            set => SetProperty( ref _username, value );
        }

        private async Task Logout( object arg )
        {
            try
            {
                IsWorking = true;

                await _client.LogoutAsync();

                if ( AssistantOptions.BackupOptions.Provider is MegaBackupProvider provider )
                {
                    provider.Authentication = null;
                }

                IsLoggedIn = false;
            }
            finally
            {
                IsWorking = false;
            }
        }

        private async Task Login( object arg )
        {
            try
            {
                IsWorking = true;

                MegaApiClient client = new MegaApiClient();
                MegaApiClient.LogonSessionToken token = await client.LoginAsync( _username, _password );

                if ( token != null )
                {
                    if ( AssistantOptions.BackupOptions.Provider is MegaBackupProvider megaBackupProvider )
                    {
                        megaBackupProvider.Authentication = token;
                    }
                }

                IsLoggedIn = true;
            }
            catch ( Exception e )
            {
                ErrorMessage = e.Message;
                IsLoggedIn = false;
            }
            finally
            {
                IsWorking = false;
            }
        }
    }
}