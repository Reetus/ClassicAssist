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

using Microsoft.Identity.Client;

namespace ClassicAssist.Data.Backup.OneDrive
{
    public static class TokenCacheHelper
    {
        public static void BeforeAccessNotification( TokenCacheNotificationArgs args )
        {
            if ( !( AssistantOptions.BackupOptions.Provider is OneDriveBackupProvider provider ) )
            {
                return;
            }

            args.TokenCache.DeserializeMsalV3( provider.Authentication );
        }

        public static void AfterAccessNotification( TokenCacheNotificationArgs args )
        {
            if ( !( AssistantOptions.BackupOptions.Provider is OneDriveBackupProvider provider ) )
            {
                return;
            }

            provider.Authentication = args.TokenCache.SerializeMsalV3();
        }

        internal static void EnableSerialization( ITokenCache tokenCache )
        {
            tokenCache.SetBeforeAccess( BeforeAccessNotification );
            tokenCache.SetAfterAccess( AfterAccessNotification );
        }
    }
}