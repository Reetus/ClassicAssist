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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CG.Web.MegaApiClient;
using ClassicAssist.Controls.VirtualFolderBrowse;
using ClassicAssist.Data.Backup.Mega;
using Newtonsoft.Json.Linq;
using UserControl = System.Windows.Controls.UserControl;

namespace ClassicAssist.Data.Backup
{
    public class MegaBackupProvider : BaseBackupProvider
    {
        private bool _overwriteExisting;
        public MegaApiClient.LogonSessionToken Authentication { get; set; }
        public override UserControl LoginControl => new MegaConfigureControl();
        public override string Name => "Mega";

        public bool OverwriteExisting
        {
            get => _overwriteExisting;
            set => SetProperty( ref _overwriteExisting, value );
        }

        public override void Serialize( JObject json )
        {
            base.Serialize( json );

            if ( Authentication == null )
            {
                return;
            }

            JObject authentication = new JObject
            {
                {
                    "SessionId",
                    Convert.ToBase64String( ProtectedData.Protect( Encoding.UTF8.GetBytes( Authentication.SessionId ),
                        null, DataProtectionScope.CurrentUser ) )
                },
                {
                    "MasterKey",
                    Convert.ToBase64String( ProtectedData.Protect( Authentication.MasterKey, null,
                        DataProtectionScope.CurrentUser ) )
                }
            };

            json.Add( "Authentication", authentication );
            json.Add( "OverwriteExisting", OverwriteExisting );
        }

        public override void Deserialize( JObject json, Options options )
        {
            base.Deserialize( json, options );

            OverwriteExisting = json?["OverwriteExisting"]?.ToObject<bool>() ?? false;

            if ( json?["Authentication"] == null )
            {
                return;
            }

            JToken auth = json["Authentication"];

            byte[] sessionId = ProtectedData.Unprotect(
                Convert.FromBase64String(
                    auth["SessionId"]?.ToObject<string>() ?? throw new InvalidOperationException() ), null,
                DataProtectionScope.CurrentUser );
            byte[] masterKey = ProtectedData.Unprotect(
                Convert.FromBase64String(
                    auth["MasterKey"]?.ToObject<string>() ?? throw new InvalidOperationException() ), null,
                DataProtectionScope.CurrentUser );

            Authentication = new MegaApiClient.LogonSessionToken( Encoding.UTF8.GetString( sessionId ), masterKey );
        }

        public override async Task<bool> Write( string fileName )
        {
            MegaApiClient client = await MegaClient.GetClient( Authentication );

            INode parent = await MegaClient.GetDirectoryById( client, BackupPath );

            using ( FileStream file = File.OpenRead( fileName ) )
            {
                string name = Path.GetFileName( fileName );

                if ( OverwriteExisting )
                {
                    INode existing = await MegaClient.GetNode( client, name, parent );

                    if ( existing != null )
                    {
                        await client.DeleteAsync( existing );
                    }
                }

                INode node = await client.UploadAsync( file, name, parent );

                return node != null;
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

        private async Task<VirtualFolder> CreateFolder( VirtualFolder virtualParent, string name )
        {
            MegaApiClient client = await MegaClient.GetClient( Authentication );

            List<INode> nodes = await MegaClient.GetAllNodes( client );

            INode parent;

            if ( virtualParent == null )
            {
                parent = await MegaClient.GetRootNode( client );
            }
            else
            {
                parent = nodes.Single( e => e.Id == virtualParent.Id );
            }

            INode node = await client.CreateFolderAsync( name, parent );

            await MegaClient.GetAllNodes( client, true );

            return new VirtualFolder { Id = node.Id, Name = node.Name, ContainsChildren = false };
        }

        private async Task<IEnumerable<VirtualFolder>> GetChildren( string id )
        {
            List<VirtualFolder> result = new List<VirtualFolder>();
            MegaApiClient client = await MegaClient.GetClient( Authentication );

            try
            {
                List<INode> nodes = await MegaClient.GetAllNodes( client );

                IEnumerable<INode> folders;

                if ( string.IsNullOrEmpty( id ) )
                {
                    INode root = nodes.First( e => e.Type == NodeType.Root );

                    folders = nodes.Where( e => e.ParentId == root.Id && e.Type == NodeType.Directory );
                }
                else
                {
                    folders = nodes.Where( e => e.ParentId == id && e.Type == NodeType.Directory );
                }

                result.AddRange( folders.Select( folder => new VirtualFolder
                {
                    Id = folder.Id,
                    Name = folder.Name,
                    ContainsChildren = nodes.Any( e => e.ParentId == folder.Id && e.Type == NodeType.Directory )
                } ) );

                return result;
            }
            catch ( Exception )
            {
                return result;
            }
        }
    }
}