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

[AttributeUsage( AttributeTargets.Assembly )]
// ReSharper disable once CheckNamespace
public class BuildDateAttribute : Attribute
{
    /* https://www.meziantou.net/getting-the-date-of-build-of-a-dotnet-assembly-at-runtime.htm */
    public BuildDateAttribute( string value )
    {
        DateTime = new DateTime( Convert.ToInt64(value), DateTimeKind.Utc ).ToLocalTime();
    }

    public DateTime DateTime { get; set; }
}