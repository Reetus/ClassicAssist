#region License

// Copyright (C) 2023 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassicAssist.Controls.VirtualFolderBrowse;
using ClassicAssist.Data.Backup.WebDAV;
using Newtonsoft.Json.Linq;
using WebDav;
using UserControl = System.Windows.Controls.UserControl;

namespace ClassicAssist.Data.Backup
{
    public sealed class WebDAVBackupProvider : BaseBackupProvider
    {
        private WebDavClient _client;

        public WebDAVBackupProvider()
        {
            IsLoggedIn = true;
        }

        public WebDAVAuthentication Authentication { get; set; } = new WebDAVAuthentication();
        public override UserControl LoginControl => new WebDAVConfigureControl();
        public override string Name => "WebDAV";

        public override void OnBackupStart()
        {
            _client = null;
        }

        public override async Task<bool> Write( string fileName )
        {
            WebDavClient client = GetClient();

            using ( FileStream fileStream = File.OpenRead( fileName ) )
            {
                Uri basePath = new Uri( Authentication.Address );

                if ( !basePath.AbsolutePath.EndsWith( "/" ) )
                {
                    basePath = new Uri( basePath.AbsoluteUri + "/" );
                }

                WebDavResponse result =
                    await client.PutFile( new Uri( basePath + Path.GetFileName( fileName ) ), fileStream );

                if ( !result.IsSuccessful )
                {
                    throw new Exception( result.Description );
                }

                return result.IsSuccessful;
            }
        }

        private WebDavClient GetClient()
        {
            if ( _client != null )
            {
                return _client;
            }

            WebDavClientParams clientParams = new WebDavClientParams
            {
                BaseAddress = new Uri( Authentication.Address ),
                Credentials = new NetworkCredential( Authentication.Username, Authentication.Password )
            };

            _client = new WebDavClient( clientParams );

            return _client;
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

            Uri uri = new Uri( Authentication.Address );

            UriBuilder urlBuilder = new UriBuilder( uri ) { Path = odppvm.SelectedItem.Id };

            Authentication.Address = urlBuilder.ToString();

            return odppvm.SelectedItem.Id;
        }

        private async Task<VirtualFolder> CreateFolder( VirtualFolder arg1, string arg2 )
        {
            WebDavClient client = GetClient();

            string path = arg1?.Id;

            if ( string.IsNullOrEmpty( path ) )
            {
                path = new Uri( Authentication.Address ).PathAndQuery;
            }

            if ( !path.EndsWith( "/" ) )
            {
                path += "/";
            }

            path += arg2;

            if ( !path.EndsWith( "/" ) )
            {
                path += "/";
            }

            WebDavResponse result = await client.Mkcol( path );

            return !result.IsSuccessful ? null : new VirtualFolder { Id = path, Name = arg2, ContainsChildren = true };
        }

        private async Task<IEnumerable<VirtualFolder>> GetChildren( string arg )
        {
            WebDavClient client = GetClient();

            string path = arg;

            if ( string.IsNullOrEmpty( path ) )
            {
                path = new Uri( Authentication.Address ).PathAndQuery;
            }

            if ( !path.EndsWith( "/" ) )
            {
                path += "/";
            }

            PropfindResponse result = await client.Propfind( path );

            if ( !result.IsSuccessful )
            {
                return Enumerable.Empty<VirtualFolder>();
            }

            return result.Resources.Where( x => x.IsCollection && !x.Uri.Equals( path ) ).Select( x =>
                new VirtualFolder
                {
                    Id = x.Uri,
                    Name = !string.IsNullOrEmpty( x.DisplayName ) ? x.DisplayName : x.Uri,
                    ContainsChildren = x.IsCollection
                } );
        }

        public override void Serialize( JObject json, bool _ = false )
        {
            base.Serialize( json, _ );

            if ( Authentication == null )
            {
                return;
            }

            string auth = JsonSerializer.Serialize( Authentication );
            byte[] authBytes = Encoding.UTF8.GetBytes( auth );

            json?.Add( "Authentication",
                Convert.ToBase64String( ProtectedData.Protect( authBytes, null, DataProtectionScope.CurrentUser ) ) );
        }

        public override void Deserialize( JObject json, Options options, bool _ = false )
        {
            base.Deserialize( json, options );

            if ( json == null )
            {
                return;
            }

            try
            {
                if ( json["Authentication"] == null )
                {
                    return;
                }

                byte[] authBytes = ProtectedData.Unprotect(
                    Convert.FromBase64String( json["Authentication"].ToObject<string>() ?? string.Empty ), null,
                    DataProtectionScope.CurrentUser );

                Authentication =
                    JsonSerializer.Deserialize<WebDAVAuthentication>( Encoding.UTF8.GetString( authBytes ) );

                IsLoggedIn = true;
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        public class WebDAVAuthentication
        {
            public string Address { get; set; }
            public string Password { get; set; }
            public string Username { get; set; }
        }
    }
}