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
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassicAssist.Data.Backup.OneDrive;
using ClassicAssist.UI.ViewModels;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using File = System.IO.File;
using UserControl = System.Windows.Controls.UserControl;

namespace ClassicAssist.Data.Backup
{
    public class OneDriveBackupProvider : SetPropertyNotifyChanged /*, IBackupProvider */ //TODO
    {
        private byte[] _authentication;
        private string _backupPath;
        private bool _isLoggedIn;

        private UserControl _loginControl = new OneDriveConfigureControl();

        public byte[] Authentication
        {
            get => _authentication;
            set => SetProperty( ref _authentication, value );
        }

        public string BackupPath
        {
            get => _backupPath;
            set => SetProperty( ref _backupPath, value );
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty( ref _isLoggedIn, value );
        }

        public UserControl LoginControl
        {
            get => _loginControl;
            set => SetProperty( ref _loginControl, value );
        }

        public string Name { get; set; } = "Microsoft OneDrive";
        public bool RequiresLogin { get; set; } = true;

        public async Task<bool> Write( string fileName )
        {
            GraphServiceClient client = await OneDriveClient.GetGraphClient();

            using ( FileStream fileStream = File.OpenRead( fileName ) )
            {
                DriveItem result = await client.Drive.Items[BackupPath].ItemWithPath( Path.GetFileName( fileName ) )
                    .Content.Request().PutAsync<DriveItem>( fileStream );

                return result != null;
            }
        }

        public async Task<string> GetPath( string currentPath )
        {
            OneDrivePathPickerViewModel odppvm = new OneDrivePathPickerViewModel( GetChildren );

            OneDrivePathPickerWindow window = new OneDrivePathPickerWindow { DataContext = odppvm };

            window.ShowDialog();

            if ( odppvm.DialogResult == DialogResult.OK )
            {
                return await Task.FromResult( odppvm.SelectedItem.Id );
            }

            return await Task.FromResult( currentPath );
        }

        public void Serialize( JObject json )
        {
            json?.Add( "Authentication",
                Convert.ToBase64String( ProtectedData.Protect( Authentication, null,
                    DataProtectionScope.CurrentUser ) ) );
            json?.Add( "BackupPath", BackupPath );
        }

        public void Deserialize( JObject json, Options options )
        {
            if ( json == null )
            {
                return;
            }

            try
            {
                Authentication =
                    ProtectedData.Unprotect(
                        Convert.FromBase64String( json["Authentication"].ToObject<string>() ?? string.Empty ), null,
                        DataProtectionScope.CurrentUser );
                BackupPath = json["BackupPath"]?.ToObject<string>() ?? string.Empty;
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        private static async Task<IEnumerable<DriveItem>> GetChildren( string id )
        {
            GraphServiceClient client = await OneDriveClient.GetGraphClient();

            if ( string.IsNullOrEmpty( id ) )
            {
                return await client.Drive.Root.Children.Request().GetAsync();
            }

            return await client.Drive.Items[id].Children.Request().GetAsync();
        }
    }
}