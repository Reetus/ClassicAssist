#region License

// Copyright (C) 2022 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassicAssist.Controls.VirtualFolderBrowse;
using ClassicAssist.Data.Backup.GoogleDrive;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using Newtonsoft.Json.Linq;
using File = System.IO.File;
using GFile = Google.Apis.Drive.v3.Data.File;
using UserControl = System.Windows.Controls.UserControl;

namespace ClassicAssist.Data.Backup
{
    public class GoogleDriveBackupProvider : BaseBackupProvider
    {
        private readonly GoogleDriveDataStore _dataStore = GoogleDriveDataStore.GetInstance();
        private IList<GFile> _folderFiles;

        public override UserControl LoginControl => new GoogleDriveConfigureControl();
        public override string Name => "Google Drive";

        public override bool RequiresLogin => true;

        public override void Serialize( JObject json, bool _ = false )
        {
            base.Serialize( json, _ );

            _dataStore.Serialize( json );
        }

        public override void Deserialize( JObject json, Options options, bool _ = false )
        {
            base.Deserialize( json, options, _ );

            if ( json == null )
            {
                return;
            }

            _dataStore.Deserialize( json );
        }

        public override async Task<bool> Write( string fileName )
        {
            DriveService client = await GoogleDriveClient.GetServiceClient();

            if ( _folderFiles == null )
            {
                FilesResource.ListRequest listRequest = client.Files.List();
                listRequest.Q = $"'{BackupPath}' in parents and trashed = false";

                FileList response = await listRequest.ExecuteAsync();
                _folderFiles = response.Files;
            }

            using ( FileStream fileStream = File.OpenRead( fileName ) )
            {
                GFile file = new GFile { Name = Path.GetFileName( fileName ) };

                GFile existing = _folderFiles.FirstOrDefault( e => e.Name == file.Name );

                ResumableUpload request;

                if ( existing == null )
                {
                    file.Parents = new List<string> { BackupPath };

                    request = client.Files.Create( file, fileStream, "application/json" );
                }
                else
                {
                    request = client.Files.Update( file, existing.Id, fileStream, "application/json" );
                }

                IUploadProgress response = await request.UploadAsync();

                return response.Status == UploadStatus.Completed;
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

        private static async Task<VirtualFolder> CreateFolder( VirtualFolder parent, string name )
        {
            DriveService client = await GoogleDriveClient.GetServiceClient();

            GFile body = new GFile { Name = name, MimeType = "application/vnd.google-apps.folder" };

            if ( parent != null )
            {
                body.Parents = new List<string> { parent.Id };
            }

            FilesResource.CreateRequest request = client.Files.Create( body );

            GFile response = await request.ExecuteAsync();

            return response == null ? null : new VirtualFolder { Name = response.Name };
        }

        private static async Task<IEnumerable<VirtualFolder>> GetChildren( string arg )
        {
            DriveService client = await GoogleDriveClient.GetServiceClient();

            FilesResource.ListRequest listRequest = client.Files.List();
            listRequest.Q = "'root' in parents and trashed = false and mimeType = 'application/vnd.google-apps.folder'";
            listRequest.Fields = "*";

            if ( !string.IsNullOrEmpty( arg ) )
            {
                listRequest.Q = $"'{arg}' in parents and mimeType = 'application/vnd.google-apps.folder'";
            }

            FileList response = await listRequest.ExecuteAsync();

            return ( from folder in response.Files select new VirtualFolder { Name = folder.Name, Id = folder.Id } )
                .ToList();
        }
    }
}