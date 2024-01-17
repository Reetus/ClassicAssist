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
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views.ECV.Settings;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Misc
{
    public class EntityCollectionViewerOptions : SetPropertyNotifyChanged
    {
        private bool _alwaysOnTop;
        private ObservableCollection<CombineStacksIgnoreEntry> _combineStacksIgnore;
        private bool _showChildItems;

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set => SetProperty( ref _alwaysOnTop, value );
        }

        public ObservableCollection<CombineStacksIgnoreEntry> CombineStacksIgnore
        {
            get => _combineStacksIgnore;
            set => SetProperty( ref _combineStacksIgnore, value );
        }

        public bool ShowChildItems
        {
            get => _showChildItems;
            set => SetProperty( ref _showChildItems, value );
        }

        public void Deserialize( JToken config )
        {
            if ( config == null )
            {
                return;
            }

            AlwaysOnTop = config["AlwaysOnTop"]?.ToObject<bool>() ?? false;
            ShowChildItems = config["ShowChildItems"]?.ToObject<bool>() ?? false;

            CombineStacksIgnore = new ObservableCollection<CombineStacksIgnoreEntry>();

            if ( config["CombineStacksIgnore"] == null )
            {
                return;
            }

            foreach ( JToken entry in config["CombineStacksIgnore"] )
            {
                CombineStacksIgnore.Add( new CombineStacksIgnoreEntry
                {
                    ID = entry["ID"]?.ToObject<int>() ?? 0, Cliloc = entry["Cliloc"]?.ToObject<int>() ?? -1, Hue = entry["Hue"]?.ToObject<int>() ?? -1
                } );
            }
        }

        public JToken Serialize()
        {
            JObject config = new JObject { { "AlwaysOnTop", AlwaysOnTop }, { "ShowChildItems", ShowChildItems } };

            JArray combineStacksIgnore = new JArray();

            if ( CombineStacksIgnore == null )
            {
                return config;
            }

            foreach ( CombineStacksIgnoreEntry entry in CombineStacksIgnore )
            {
                combineStacksIgnore.Add( new JObject { { "ID", entry.ID }, { "Cliloc", entry.Cliloc }, { "Hue", entry.Hue } } );
            }

            config.Add( "CombineStacksIgnore", combineStacksIgnore );

            return config;
        }
    }
}