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
using System.Diagnostics;
using System.Reflection;
using Semver;

namespace ClassicAssist.Shared
{
    public static class VersionHelpers
    {
        public static SemVersion GetProductVersion( Assembly assembly )
        {
            return GetProductVersion( assembly.Location );
        }

        public static SemVersion GetProductVersion( string fileName )
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo( fileName );

            return SemVersion.Parse( versionInfo.ProductVersion, SemVersionStyles.Strict );
        }

        public static bool IsVersionNewer( string currentVersion, string newVersion )
        {
            try
            {
                SemVersion currentSemver = SemVersion.Parse( currentVersion, SemVersionStyles.Strict );
                SemVersion newSemver = SemVersion.Parse( newVersion, SemVersionStyles.Strict );

                if ( currentSemver.Prerelease.Equals( "develop" ) )
                {
                    /* Don't update develop */
                    return false;
                }

                return SemVersion.ComparePrecedence( currentSemver, newSemver ) == -1;
            }
            catch ( Exception )
            {
                return true;
            }
        }
    }
}