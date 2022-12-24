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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Prompt = Microsoft.Identity.Client.Prompt;

namespace ClassicAssist.Data.Backup.OneDrive
{
    public static class OneDriveClient
    {
        private const string APP_ID = "edb89377-2d79-40d7-864b-3ae9fa56ea1c";
        private static readonly string[] _scopes = { "user.read", "Files.ReadWrite.All", "offline_access" };

        public static IPublicClientApplication GetClient()
        {
            IPublicClientApplication app = PublicClientApplicationBuilder.Create( APP_ID )
                .WithAuthority( "https://login.microsoftonline.com/common" ).WithRedirectUri( "http://localhost/" )
                .Build();

            TokenCacheHelper.EnableSerialization( app.UserTokenCache );

            return app;
        }

        public static async Task<AuthenticationResult> GetAccessTokenAsync( bool interactiveIfRequired )
        {
            IPublicClientApplication client = GetClient();

            IAccount[] accounts = ( await client.GetAccountsAsync() ).ToArray();
            IAccount firstAccount = accounts.FirstOrDefault();

            AuthenticationResult msalResult;

            try
            {
                msalResult = await client.AcquireTokenSilent( _scopes, firstAccount ).ExecuteAsync();
            }
            catch ( MsalUiRequiredException )
            {
                if ( interactiveIfRequired )
                {
                    // This block is executed if the user requires interactive authentication.
                    msalResult = await client.AcquireTokenInteractive( _scopes )
                        .WithAccount( accounts.FirstOrDefault() ).WithPrompt( Prompt.SelectAccount ).ExecuteAsync();
                }
                else
                {
                    throw;
                }
            }

            return msalResult;
        }

        public static async Task<GraphServiceClient> GetGraphClient()
        {
            AuthenticationResult authentication = await GetAccessTokenAsync( true );

            GraphServiceClient graphClient = new GraphServiceClient( new DelegateAuthenticationProvider(
                async requestMessage =>
                {
                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue( "Bearer", authentication.AccessToken );
                    await Task.CompletedTask;
                } ) );

            return graphClient;
        }

        public static async Task LogoutAsync()
        {
            IPublicClientApplication client = GetClient();

            IEnumerable<IAccount> accounts = await client.GetAccountsAsync();

            foreach ( IAccount account in accounts )
            {
                await client.RemoveAsync( account );
            }
        }
    }
}