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
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace ClassicAssist.Shared
{
    public class Github
    {
        public static GitHubClient GetClient( string path )
        {
            UpdaterSettings options = UpdaterSettings.Load( path );

            GitHubClient client = new GitHubClient( new ProductHeaderValue( Settings.Default.RepositoryName ) );

            if ( !string.IsNullOrEmpty( options.GithubAccessToken ) )
            {
                client.Credentials = new Credentials( options.GithubAccessToken );
            }

            return client;
        }

        public static async Task<Release> GetLatestRelease( string path )
        {
            GitHubClient client = GetClient( path );

            UpdaterSettings options = UpdaterSettings.Load( path );

            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll( Settings.Default.RepositoryOwner, Settings.Default.RepositoryName );

            Release latestRelease = releases?.OrderByDescending( r => r.CreatedAt )
                .FirstOrDefault( r => options.InstallPrereleases || !r.Prerelease );

            return latestRelease;
        }

        public static async Task<string> GetUpdateText( string path )
        {
            GitHubClient client = GetClient( path );

            UpdaterSettings options = UpdaterSettings.Load( path );

            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll( Settings.Default.RepositoryOwner, Settings.Default.RepositoryName );

            IReadOnlyList<GitHubCommit> commits = await client.Repository.Commit.GetAll( Settings.Default.RepositoryOwner, Settings.Default.RepositoryName );

            IEnumerable<Release> latestReleases = releases.OrderByDescending( c => c.CreatedAt ).Take( 15 );

            StringBuilder commitMessage = new StringBuilder();

            foreach ( Release release in latestReleases.Where( e => options.InstallPrereleases || !e.Prerelease ) )
            {
                string releaseMessage = release.Body;

                if ( string.IsNullOrEmpty( releaseMessage ) )
                {
                    GitHubCommit commit = commits.FirstOrDefault( c => c.Commit.Sha == release.TargetCommitish );

                    releaseMessage = commit?.Commit.Message ?? "Unknown";
                }

                commitMessage.AppendLine( $"{release.CreatedAt.Date.Date.ToShortDateString()}:" );
                commitMessage.AppendLine( releaseMessage );
                commitMessage.AppendLine();
            }

            return commitMessage.ToString();
        }
    }
}