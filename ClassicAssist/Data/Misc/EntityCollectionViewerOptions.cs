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

using System.Collections.ObjectModel;
using ClassicAssist.Misc;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views.ECV.Settings.Models;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Misc
{
    public class EntityCollectionViewerOptions : SetPropertyNotifyChanged
    {
        private bool _alwaysOnTop;
        private ObservableCollection<CombineStacksOpenContainersIgnoreEntry> _combineStacksIgnore;
        private ObservableCollection<CombineStacksOpenContainersIgnoreEntry> _openContainersIgnore;
        private bool _showChildItems;

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set => SetProperty( ref _alwaysOnTop, value );
        }

        public ObservableCollection<CombineStacksOpenContainersIgnoreEntry> CombineStacksIgnore
        {
            get => _combineStacksIgnore;
            set => SetProperty( ref _combineStacksIgnore, value );
        }

        public string Hash { get; set; }

        public ObservableCollection<CombineStacksOpenContainersIgnoreEntry> OpenContainersIgnore
        {
            get => _openContainersIgnore;
            set => SetProperty( ref _openContainersIgnore, value );
        }

        public bool ShowChildItems
        {
            get => _showChildItems;
            set => SetProperty( ref _showChildItems, value );
        }

        public static EntityCollectionViewerOptions Deserialize( JObject config )
        {
            EntityCollectionViewerOptions options = new EntityCollectionViewerOptions();

            if ( config == null )
            {
                return options;
            }

            options.AlwaysOnTop = config["AlwaysOnTop"]?.ToObject<bool>() ?? false;
            options.ShowChildItems = config["ShowChildItems"]?.ToObject<bool>() ?? false;

            options.CombineStacksIgnore = new ObservableCollection<CombineStacksOpenContainersIgnoreEntry>();

            if ( config["CombineStacksIgnore"] != null )
            {
                foreach ( JToken entry in config["CombineStacksIgnore"] )
                {
                    options.CombineStacksIgnore.Add( new CombineStacksOpenContainersIgnoreEntry
                    {
                        ID = entry["ID"]?.ToObject<int>() ?? 0, Cliloc = entry["Cliloc"]?.ToObject<int>() ?? -1, Hue = entry["Hue"]?.ToObject<int>() ?? -1
                    } );
                }
            }

            options.OpenContainersIgnore = new ObservableCollection<CombineStacksOpenContainersIgnoreEntry>();

            if ( config["OpenContainersIgnore"] == null )
            {
                return options;
            }

            foreach ( JToken entry in config["OpenContainersIgnore"] )
            {
                options.OpenContainersIgnore.Add( new CombineStacksOpenContainersIgnoreEntry
                {
                    ID = entry["ID"]?.ToObject<int>() ?? 0, Cliloc = entry["Cliloc"]?.ToObject<int>() ?? -1, Hue = entry["Hue"]?.ToObject<int>() ?? -1
                } );
            }

            options.Hash = config.ToString().SHA1();

            return options;
        }

        public static JToken Serialize( EntityCollectionViewerOptions options )
        {
            JObject config = new JObject { { "AlwaysOnTop", options.AlwaysOnTop }, { "ShowChildItems", options.ShowChildItems } };

            JArray combineStacksIgnore = new JArray();

            if ( options.CombineStacksIgnore != null )
            {
                foreach ( CombineStacksOpenContainersIgnoreEntry entry in options.CombineStacksIgnore )
                {
                    combineStacksIgnore.Add( new JObject { { "ID", entry.ID }, { "Cliloc", entry.Cliloc }, { "Hue", entry.Hue } } );
                }

                config.Add( "CombineStacksIgnore", combineStacksIgnore );
            }

            JArray openContainersIgnore = new JArray();

            if ( options.OpenContainersIgnore == null )
            {
                return config;
            }

            foreach ( CombineStacksOpenContainersIgnoreEntry entry in options.OpenContainersIgnore )
            {
                openContainersIgnore.Add( new JObject { { "ID", entry.ID }, { "Cliloc", entry.Cliloc }, { "Hue", entry.Hue } } );
            }

            config.Add( "OpenContainersIgnore", openContainersIgnore );

            return config;
        }
    }
}