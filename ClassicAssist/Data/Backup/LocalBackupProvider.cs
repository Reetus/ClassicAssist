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
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Assistant;
using ClassicAssist.Shared.Resources;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Backup
{
    public sealed class LocalBackupProvider : BaseBackupProvider
    {
        public LocalBackupProvider()
        {
            BackupPath = DefaultBackupPath;
        }

        public static string DefaultBackupPath => "Backup";

        public override bool IsLoggedIn => true;

        public override string Name => "Local Backup";

        public override async Task<bool> Write( string fileName )
        {
            string fullPath = BackupPath;

            if ( !Path.IsPathRooted( BackupPath ) )
            {
                fullPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, BackupPath );
            }

            if ( !Directory.Exists( fullPath ) )
            {
                Directory.CreateDirectory( fullPath );
            }

            fullPath = Path.Combine( fullPath, Path.GetFileName( fileName ) );

            using ( FileStream originalStream = File.OpenRead( fileName ) )
            {
                using ( FileStream destinationStream = File.OpenWrite( fullPath ) )
                {
                    await originalStream.CopyToAsync( destinationStream );
                }
            }

            FirstRun = true;

            return File.Exists( fullPath );
        }

        public override async Task<string> GetPath( string currentPath )
        {
            string fullPath = currentPath;

            if ( !Path.IsPathRooted( fullPath ) )
            {
                fullPath = Path.Combine( Engine.StartupPath, currentPath );
            }

            FolderBrowserDialog folderBrowseDialog = new FolderBrowserDialog
            {
                Description = Strings.Choose_backup_folder, SelectedPath = fullPath, ShowNewFolderButton = true
            };

            DialogResult result = folderBrowseDialog.ShowDialog();

            return await Task.FromResult( result == DialogResult.OK ? folderBrowseDialog.SelectedPath : currentPath );
        }

        public override void Deserialize( JObject json, Options options, bool _ = false )
        {
            base.Deserialize( json, options );

            if ( string.IsNullOrEmpty( BackupPath ) )
            {
                BackupPath = DefaultBackupPath;
            }
        }
    }
}