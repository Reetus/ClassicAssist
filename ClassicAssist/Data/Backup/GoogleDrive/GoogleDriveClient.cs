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

using System;
using System.Threading;
using System.Threading.Tasks;
using ClassicAssist.Shared.Resources;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace ClassicAssist.Data.Backup.GoogleDrive
{
    public class GoogleDriveClient
    {
        private const string CLIENT_ID = "247705061222-ikb9intpgf5cb854rj9af0slt5og2cqa.apps.googleusercontent.com";
        private const string CLIENT_SECRET = "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";

        public static UserCredential Credential { get; set; }

        public static async Task<UserCredential> GetAccessTokenAsync( CancellationToken cancellationToken )
        {
            GoogleDriveDataStore dataStore = GoogleDriveDataStore.GetInstance();

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = CLIENT_ID, ClientSecret = CLIENT_SECRET },
                new[] { DriveService.Scope.DriveFile }, AssistantOptions.UserId, cancellationToken, dataStore );

            Credential = credential ?? throw new Exception( Strings.Authentication_error_or_timeout );

            return credential;
        }

        public static async Task LogoutAsync()
        {
            await GoogleDriveDataStore.GetInstance().ClearAsync();
        }

        public static async Task<DriveService> GetServiceClient()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter( TimeSpan.FromMinutes( 2 ) );

            if ( Credential == null )
            {
                await GetAccessTokenAsync( tokenSource.Token );
            }

            DriveService service = new DriveService( new BaseClientService.Initializer
            {
                HttpClientInitializer = Credential, ApplicationName = "ClassicAssist"
            } );

            return service;
        }
    }
}