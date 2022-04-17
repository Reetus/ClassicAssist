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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassicAssist.Shared
{
    public class Updater
    {
        public static async Task<ReleaseVersion> GetReleases( bool preleases )
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync( preleases ? Settings.Default.PrereleaseManifestURL : Settings.Default.UpdateManifestURL );

            if ( response.StatusCode != HttpStatusCode.OK )
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();

            ReleaseVersion releaseVersion;

            try
            {
                releaseVersion = JsonConvert.DeserializeObject<ReleaseVersion>( json );
            }
            catch ( Exception )
            {
                return null;
            }

            return releaseVersion;
        }

        public static async Task<string> GetUpdateText( bool prereleases )
        {
            ReleaseVersion releaseVersion = await GetReleases( prereleases );

            if ( releaseVersion?.Entries == null )
            {
                return string.Empty;
            }

            StringBuilder commitMessage = new StringBuilder();

            foreach ( ChangelogEntry release in releaseVersion.Entries )
            {
                commitMessage.AppendLine( $"{release.CreatedAt.Date.Date.ToShortDateString()}:" );
                commitMessage.AppendLine( release.Description );
                commitMessage.AppendLine();
            }

            return commitMessage.ToString();
        }
    }
}