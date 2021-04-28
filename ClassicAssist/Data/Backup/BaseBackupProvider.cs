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
using ClassicAssist.Shared.UI;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Backup
{
    public abstract class BaseBackupProvider : SetPropertyNotifyChanged, IBackupProvider
    {
        private string _backupPath;
        private bool _firstRun = true;
        private bool _incremental;
        private bool _isLoggedIn;

        public virtual void Serialize( JObject json )
        {
            json?.Add( "BackupPath", BackupPath );
            json?.Add( "FirstRun", FirstRun );
            json?.Add( "Incremental", Incremental );
        }

        public virtual void Deserialize( JObject json, Options options )
        {
            BackupPath = json?["BackupPath"]?.ToObject<string>();
            FirstRun = json?["FirstRun"]?.ToObject<bool>() ?? true;
            Incremental = json?["Incremental"]?.ToObject<bool>() ?? false;
        }

        public virtual string BackupPath
        {
            get => _backupPath;
            set => SetProperty( ref _backupPath, value );
        }

        public virtual bool Incremental
        {
            get => _incremental;
            set => SetProperty( ref _incremental, value );
        }

        public virtual bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty( ref _isLoggedIn, value );
        }

        public virtual UserControl LoginControl { get; set; } = null;
        public abstract string Name { get; }

        public virtual bool RequiresLogin => false;
        public abstract Task<bool> Write( string fileName );

        public abstract Task<string> GetPath( string currentPath );

        public bool FirstRun
        {
            get => _firstRun;
            set => SetProperty( ref _firstRun, value );
        }
    }
}