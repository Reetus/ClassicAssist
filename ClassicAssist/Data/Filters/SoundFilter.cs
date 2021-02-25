﻿#region License

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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UI.Views.Filters;
using ClassicAssist.UO.Network.PacketFilter;
using Microsoft.Scripting.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "Sound Filter", DefaultEnabled = false )]
    public class SoundFilter : DynamicFilterEntry, IConfigurableFilter
    {
        private const string DATA_FILE = "SoundFilters.json";

        public SoundFilter()
        {
            string dataPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data" );

            if ( !File.Exists( Path.Combine( dataPath, DATA_FILE ) ) )
            {
                return;
            }

            Items.AddRange(
                JsonConvert.DeserializeObject<SoundFilterEntry[]>(
                    File.ReadAllText( Path.Combine( dataPath, DATA_FILE ) ) ) );

            foreach ( SoundFilterEntry item in Items )
            {
                item.LocalizedName = Strings.ResourceManager.GetString( item.Name ) ?? item.Name;
                item.Category = Strings.ResourceManager.GetString( item.Category ) ?? item.Category;
            }
        }

        public static bool IsEnabled { get; set; }

        public ObservableCollection<SoundFilterEntry> Items { get; set; } =
            new ObservableCollection<SoundFilterEntry>();

        public void Configure()
        {
            SoundFilterConfigureWindow window = new SoundFilterConfigureWindow( Items );
            window.ShowDialog();
        }

        public void Deserialize( JToken token )
        {
            if ( token?["Items"] == null )
            {
                return;
            }

            foreach ( JToken itemsToken in token["Items"] )
            {
                string name = itemsToken["Name"]?.ToObject<string>() ?? string.Empty;

                if ( string.IsNullOrEmpty( name ) )
                {
                    continue;
                }

                SoundFilterEntry entry = Items.FirstOrDefault( e => e.Name == name );

                if ( entry == null )
                {
                    continue;
                }

                entry.Enabled = itemsToken["Enabled"]?.ToObject<bool>() ?? false;
            }
        }

        public JObject Serialize()
        {
            JObject config = new JObject();

            JArray items = new JArray();

            foreach ( SoundFilterEntry entry in Items )
            {
                JObject obj = new JObject { ["Name"] = entry.Name, ["Enabled"] = entry.Enabled };

                items.Add( obj );
            }

            config.Add( "Items", items );

            return config;
        }

        public void ResetOptions()
        {
            foreach ( SoundFilterEntry item in Items )
            {
                item.Enabled = item.DefaultEnabled;
            }
        }

        protected override void OnChanged( bool enabled )
        {
            IsEnabled = enabled;
        }

        public override bool CheckPacket( ref byte[] packet, ref int length, PacketDirection direction )
        {
            if ( packet == null || !IsEnabled )
            {
                return false;
            }

            if ( packet[0] != 0x54 || direction != PacketDirection.Incoming )
            {
                return false;
            }

            int soundId = ( packet[2] << 8 ) | packet[3];

            IEnumerable<int> allEnabledSoundIds = Items.Where( e => e.Enabled ).SelectMany( e => e.SoundIDs );

            return allEnabledSoundIds.Contains( soundId );
        }
    }

    public class SoundFilterEntry
    {
        public string Category { get; set; }
        public bool DefaultEnabled { get; set; }
        public bool Enabled { get; set; }
        public bool IsExpanded { get; set; } = true;
        public string LocalizedName { get; set; }
        public string Name { get; set; }
        public int[] SoundIDs { get; set; }
    }
}