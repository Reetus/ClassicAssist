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

using System.IO;
using ClassicAssist.Shared.UI;
using Newtonsoft.Json;

namespace ClassicAssist.Shared
{
    public class UpdaterSettings : SetPropertyNotifyChanged
    {
        private string _githubAccessToken;
        private bool _installPrereleases;
        private const string SETTINGS_FILE = "updater.settings.json";

        public string GithubAccessToken
        {
            get => _githubAccessToken;
            set => SetProperty( ref _githubAccessToken, value );
        }

        public bool InstallPrereleases
        {
            get => _installPrereleases;
            set =>
                SetProperty( ref _installPrereleases, value );
        }

        public static UpdaterSettings Load( string path )
        {
            string settingsFile = Path.Combine( path, SETTINGS_FILE );

            if ( !File.Exists( settingsFile ) )
            {
                return new UpdaterSettings();
            }

            using ( StreamReader streamReader = new StreamReader( settingsFile ) )
            {
                JsonSerializer serializer = new JsonSerializer();

                using ( JsonTextReader reader = new JsonTextReader( streamReader ) )
                {
                    UpdaterSettings updaterSettings = serializer.Deserialize<UpdaterSettings>( reader );

                    return updaterSettings ?? new UpdaterSettings();
                }
            }
        }

        public static void Save( UpdaterSettings updaterSettings, string path )
        {
            string settingsFile = Path.Combine( path, SETTINGS_FILE );

            using ( StreamWriter streamWriter = new StreamWriter( settingsFile ) )
            {
                JsonSerializer serializer = new JsonSerializer();

                using ( JsonTextWriter
                    writer = new JsonTextWriter( streamWriter ) { Formatting = Formatting.Indented } )
                {
                    serializer.Serialize( writer, updaterSettings );
                }
            }
        }
    }
}