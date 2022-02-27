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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ClassicAssist.Shared.UI;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;

namespace ClassicAssist.Data.Backup.GoogleDrive
{
    public class GoogleDriveConfigureViewModel : SetPropertyNotifyChanged
    {
        private UserCredential _authenticationResult;
        private string _errorMessage;
        private bool _isLoggedIn;
        private bool _isWorking;
        private ICommand _loginCommand;
        private ICommand _logoutCommand;

        public UserCredential AuthenticationResult
        {
            get => _authenticationResult;
            set => SetProperty( ref _authenticationResult, value );
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

        private async Task Logout( object arg )
        {
            try
            {
                IsWorking = true;

                await GoogleDriveClient.LogoutAsync();

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

                AuthenticationResult = await GoogleDriveClient.GetAccessTokenAsync( CancellationToken.None );

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