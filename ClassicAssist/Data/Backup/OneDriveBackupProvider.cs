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
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassicAssist.Controls.VirtualFolderBrowse;
using ClassicAssist.Data.Backup.OneDrive;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using File = System.IO.File;
using UserControl = System.Windows.Controls.UserControl;

namespace ClassicAssist.Data.Backup
{
    public sealed class OneDriveBackupProvider : BaseBackupProvider
    {
        private byte[] _authentication;

        public byte[] Authentication
        {
            get => _authentication;
            set => SetProperty( ref _authentication, value );
        }

        public override UserControl LoginControl => new OneDriveConfigureControl();

        public override string Name => "Microsoft OneDrive";

        public override async Task<bool> Write( string fileName )
        {
            GraphServiceClient client = await OneDriveClient.GetGraphClient();

            using ( FileStream fileStream = File.OpenRead( fileName ) )
            {
                DriveItem result = await client.Drive.Items[BackupPath].ItemWithPath( Path.GetFileName( fileName ) )
                    .Content.Request().PutAsync<DriveItem>( fileStream );

                return result != null;
            }
        }

        public override async Task<string> GetPath( string currentPath )
        {
            VirtualFolderBrowseViewModel odppvm = new VirtualFolderBrowseViewModel( GetChildren, CreateFolder );

            VirtualFolderBrowseWindow window = new VirtualFolderBrowseWindow { DataContext = odppvm };

            window.ShowDialog();

            if ( odppvm.DialogResult != DialogResult.OK )
            {
                return await Task.FromResult( currentPath );
            }

            /* Set FirstRun if changing the folder */
            FirstRun = true;
            return await Task.FromResult( odppvm.SelectedItem.Id );
        }

        public override void Serialize( JObject json )
        {
            base.Serialize( json );

            if ( Authentication != null )
            {
                json?.Add( "Authentication",
                    Convert.ToBase64String( ProtectedData.Protect( Authentication, null,
                        DataProtectionScope.CurrentUser ) ) );
            }
        }

        public override void Deserialize( JObject json, Options options )
        {
            base.Deserialize( json, options );

            if ( json == null )
            {
                return;
            }

            try
            {
                if ( json["Authentication"] != null )
                {
                    Authentication = ProtectedData.Unprotect(
                        Convert.FromBase64String( json["Authentication"].ToObject<string>() ?? string.Empty ), null,
                        DataProtectionScope.CurrentUser );
                }
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        private static async Task<VirtualFolder> CreateFolder( VirtualFolder parent, string folder )
        {
            GraphServiceClient graphClient = await OneDriveClient.GetGraphClient();

            IDriveItemChildrenCollectionRequestBuilder parentDriveItem = parent == null
                ? graphClient.Drive.Root.Children
                : graphClient.Drive.Items[parent.Id].Children;

            DriveItem result = await parentDriveItem.Request()
                .AddAsync( new DriveItem { Name = folder, Folder = new Folder() } );

            return result != null
                ? new VirtualFolder { Id = result.Id, Name = result.Name, ContainsChildren = false }
                : null;
        }

        private static async Task<IEnumerable<VirtualFolder>> GetChildren( string id )
        {
            GraphServiceClient client = await OneDriveClient.GetGraphClient();

            IDriveItemChildrenCollectionPage files;

            if ( string.IsNullOrEmpty( id ) )
            {
                files = await client.Drive.Root.Children.Request().GetAsync();
            }
            else
            {
                files = await client.Drive.Items[id].Children.Request().GetAsync();
            }

            return files.Where( driveItem => driveItem.Folder != null ).Select( driveItem => new VirtualFolder
            {
                Id = driveItem.Id, Name = driveItem.Name, ContainsChildren = driveItem.Folder?.ChildCount > 0
            } );
        }
    }
}