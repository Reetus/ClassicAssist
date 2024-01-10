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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassicAssist.Shared
{
    public class Updater
    {
        public static async Task<IEnumerable<ChangelogEntry>> GetReleases()
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync( Settings.Default.UpdateManifestURL );

            if ( response.StatusCode != HttpStatusCode.OK )
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonConvert.DeserializeObject<IEnumerable<ChangelogEntry>>( json );
            }
            catch ( Exception )
            {
                return null;
            }
        }

        public static async Task<ChangelogEntry> GetLatestRelease( bool prereleases )
        {
            IEnumerable<ChangelogEntry> releases = await GetReleases();

            ChangelogEntry latestRelease = releases.FirstOrDefault( e => prereleases || !e.Prerelease );

            return latestRelease;
        }

        public static async Task<string> GetUpdateText( bool prereleases )
        {
            IEnumerable<ChangelogEntry> releases = await GetReleases();

            if ( !prereleases )
            {
                releases = releases.Where( e => !e.Prerelease );
            }

            StringBuilder commitMessage = new StringBuilder();

            foreach ( ChangelogEntry release in releases )
            {
                commitMessage.AppendLine( $"{release.CreatedAt.Date.Date.ToShortDateString()}:" );
                commitMessage.AppendLine( release.Description );
            }

            return commitMessage.ToString();
        }
    }
}