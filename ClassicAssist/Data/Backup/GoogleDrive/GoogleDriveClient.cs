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
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
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
                new[] { DriveService.Scope.DriveFile }, AssistantOptions.UserId, cancellationToken,
                dataStore );

            if ( credential == null )
            {
                throw new Exception( Strings.Authentication_error_or_timeout );
            }

            if ( !credential.Token.Scope.Contains( DriveService.Scope.DriveFile ) )
            {
                await GoogleDriveDataStore.GetInstance().ClearAsync();

                throw new Exception( Strings
                    .You_must_tick_the___See__edit__create_and_delete_only_the_specific_Google_Drive_files_that_you_use_with_this_app____option_when_logging_in_to_enable_Google_Drive_backup );
            }

            Credential = credential;

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