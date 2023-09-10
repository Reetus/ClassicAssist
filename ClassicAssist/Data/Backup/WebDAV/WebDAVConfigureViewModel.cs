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
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Backup.WebDAV
{
    public class WebDAVConfigureViewModel : SetPropertyNotifyChanged
    {
        private string _address;
        private string _password;
        private string _username;

        public WebDAVConfigureViewModel()
        {
            if ( !( AssistantOptions.BackupOptions?.Provider is WebDAVBackupProvider webdav ) )
            {
                return;
            }

            Address = webdav.Authentication.Address;
            Username = webdav.Authentication.Username;
            Password = webdav.Authentication.Password;
        }

        public string Address
        {
            get => _address;
            set
            {
                SetProperty( ref _address, value );

                if ( !( AssistantOptions.BackupOptions?.Provider is WebDAVBackupProvider webdav ) )
                {
                    return;
                }

                webdav.Authentication.Address = value;

                try
                {
                    webdav.BackupPath = new Uri( value ).PathAndQuery;
                }
                catch
                {
                    // ignored
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty( ref _password, value );

                if ( AssistantOptions.BackupOptions?.Provider is WebDAVBackupProvider webdav )
                {
                    webdav.Authentication.Password = value;
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                SetProperty( ref _username, value );

                if ( AssistantOptions.BackupOptions?.Provider is WebDAVBackupProvider webdav )
                {
                    webdav.Authentication.Username = value;
                }
            }
        }
    }
}