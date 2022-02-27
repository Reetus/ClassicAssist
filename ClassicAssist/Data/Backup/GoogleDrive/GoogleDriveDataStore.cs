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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Backup.GoogleDrive
{
    public class GoogleDriveDataStore : IDataStore
    {
        private static GoogleDriveDataStore _instance;
        private static readonly object _instanceLock = new object();
        private readonly Dictionary<string, object> _credentials = new Dictionary<string, object>();

        private GoogleDriveDataStore()
        {
        }

        public Task StoreAsync<T>( string key, T value )
        {
            if ( _credentials.ContainsKey( key ) )
            {
                _credentials.Remove( key );
            }

            _credentials.Add( key, value );

            return Task.CompletedTask;
        }

        public Task DeleteAsync<T>( string key )
        {
            if ( _credentials.ContainsKey( key ) )
            {
                _credentials.Remove( key );
            }

            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>( string key )
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

            if ( !_credentials.ContainsKey( key ) )
            {
                tcs.SetResult( default );
            }
            else
            {
                tcs.SetResult( (T) _credentials[key] );
            }

            return tcs.Task;
        }

        public Task ClearAsync()
        {
            _credentials.Clear();

            return Task.CompletedTask;
        }

        public static GoogleDriveDataStore GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new GoogleDriveDataStore();
                    }
                }
            }

            return _instance;
        }

        public void Serialize( JObject json )
        {
            if ( json == null )
            {
                return;
            }

            JArray credentials = new JArray();

            foreach ( KeyValuePair<string, object> keyValuePair in _credentials.Where( keyValuePair =>
                         keyValuePair.Value is TokenResponse ) )
            {
                string value = Convert.ToBase64String( ProtectedData.Protect(
                    Encoding.ASCII.GetBytes( JsonConvert.SerializeObject( keyValuePair.Value ) ), null,
                    DataProtectionScope.CurrentUser ) );

                credentials.Add( new JObject { { "Key", keyValuePair.Key }, { "Value", value } } );
            }

            json.Add( "Credentials", credentials );
        }

        public void Deserialize( JObject json )
        {
            if ( json?["Credentials"] == null )
            {
                return;
            }

            foreach ( JToken jToken in json["Credentials"] )
            {
                string key = jToken["Key"]?.ToObject<string>() ?? string.Empty;
                string value = jToken["Value"]?.ToObject<string>() ?? string.Empty;

                byte[] unprotected = ProtectedData.Unprotect( Convert.FromBase64String( value ), null,
                    DataProtectionScope.CurrentUser );

                TokenResponse obj =
                    JsonConvert.DeserializeObject<TokenResponse>( Encoding.ASCII.GetString( unprotected ) );

                if ( !string.IsNullOrEmpty( key ) )
                {
                    _credentials.Add( key, obj );
                }
            }
        }
    }
}