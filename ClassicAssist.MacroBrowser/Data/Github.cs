#region License

// Copyright (C) 2020 Reetus
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

using System.Threading.Tasks;
using Octokit;

namespace ClassicAssist.MacroBrowser.Data
{
    public class Github
    {
        private const string REPOSITORY_OWNER = "Reetus";
        private const string REPOSITORY_NAME = "ClassicAssist-Macros";

        private const string MANIFEST_URL =
            "https://raw.githubusercontent.com/Reetus/ClassicAssist-Macros/master/Macros/metadata.json";

        private static Github _instance;
        private static readonly object _instanceLock = new object();
        private readonly GitHubClient _github;

        private Github()
        {
            _github = new GitHubClient( new ProductHeaderValue( "ClassicAssist" ) );
        }

        public static Github GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new Github();
                    }
                }
            }

            return _instance;
        }

        public async Task<string> GetLatestCommitHash()
        {
            Reference reference = await _github.Git.Reference.Get( REPOSITORY_OWNER, REPOSITORY_NAME, "heads/master" );

            return reference.Object.Sha;
        }
    }
}