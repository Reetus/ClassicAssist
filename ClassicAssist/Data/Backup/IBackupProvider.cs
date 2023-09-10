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

using System.Threading.Tasks;
using System.Windows.Controls;
using ClassicAssist.Misc;

namespace ClassicAssist.Data.Backup
{
    public interface IBackupProvider : ISettingProvider
    {
        string BackupPath { get; set; }
        bool FirstRun { get; set; }
        bool Incremental { get; set; }
        bool IsLoggedIn { get; set; }
        UserControl LoginControl { get; set; }
        string Name { get; }
        bool RequiresLogin { get; }
        Task<bool> Write( string fileName );
        Task<string> GetPath( string currentPath );
        void OnBackupStart();
        void OnBackupFinish();
    }
}