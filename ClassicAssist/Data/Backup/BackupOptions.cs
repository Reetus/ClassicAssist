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
using ClassicAssist.Misc;
using ClassicAssist.Shared.UI;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Backup
{
    public class BackupOptions : SetPropertyNotifyChanged, ISettingProvider, ICloneable
    {
        public const string DefaultBackupPath = "Backup";
        private int _days = 7;
        private bool _enabled = true;
        private IBackupProvider _provider = new LocalBackupProvider();

        public int Days
        {
            get => _days;
            set => SetProperty( ref _days, value );
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public Dictionary<string, string> Hashes { get; set; } = new Dictionary<string, string>();

        public DateTime LastBackup { get; set; }

        public IBackupProvider Provider
        {
            get => _provider;
            set => SetProperty( ref _provider, value );
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void Serialize( JObject json, bool _ = false)
        {
            JObject backup = new JObject { { "Enabled", Enabled }, { "Days", Days }, { "LastBackup", LastBackup } };

            JObject hashes = new JObject();

            foreach ( KeyValuePair<string, string> keyValuePair in Hashes )
            {
                hashes.Add( keyValuePair.Key, keyValuePair.Value );
            }

            backup.Add( "Hashes", hashes );

            JObject provider = new JObject { { "Name", Provider.GetType().ToString() } };

            Provider.Serialize( provider );

            backup.Add( "Provider", provider );

            json?.Add( "Backup", backup );
        }

        public void Deserialize( JObject json, Options options, bool _ = false )
        {
            if ( json?["Backup"] == null )
            {
                return;
            }

            JToken config = json["Backup"];

            Enabled = config["Enabled"]?.ToObject<bool>() ?? true;
            Days = config["Days"]?.ToObject<int>() ?? 7;
            LastBackup = config["LastBackup"]?.ToObject<DateTime>() ?? default;

            if ( config["Hashes"] != null )
            {
                foreach ( JToken token in config["Hashes"] )
                {
                    JProperty property = token.ToObject<JProperty>();

                    if ( property != null )
                    {
                        Hashes.Add( property.Name, property.Value.ToObject<string>() );
                    }
                }
            }

            if ( config["Provider"] != null )
            {
                Type type = Type.GetType( config["Provider"]["Name"]?.ToObject<string>() ??
                                          typeof( LocalBackupProvider ).ToString() ) ?? typeof( LocalBackupProvider );
                Provider = (IBackupProvider) Activator.CreateInstance( type );
                Provider.Deserialize( (JObject) config["Provider"], Options.CurrentOptions );
            }
            else
            {
                Provider = new LocalBackupProvider();
            }
        }
    }
}