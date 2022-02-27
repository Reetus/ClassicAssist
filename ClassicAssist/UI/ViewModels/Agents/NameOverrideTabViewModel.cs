#region License

// Copyright (C) 2022 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.NameOverride;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class NameOverrideTabViewModel : BaseViewModel, ISettingProvider
    {
        private ICommand _addCommand;
        private ICommand _addEmptyCommand;
        private bool _enabled;
        private ICommand _removeCommand;
        private NameOverrideEntry _selectedItem;

        public NameOverrideTabViewModel()
        {
            NameOverrideManager manager = NameOverrideManager.GetInstance();

            manager.Enabled = () => _enabled;
            manager.Items = Items;
        }

        public ICommand AddCommand =>
            _addCommand ?? ( _addCommand = new RelayCommandAsync( Add, o => true ) );

        public ICommand AddEmptyCommand =>
            _addEmptyCommand ?? ( _addEmptyCommand = new RelayCommand( AddEmpty, o => true ) );

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public ObservableCollection<NameOverrideEntry> Items { get; set; } =
            new ObservableCollection<NameOverrideEntry>();

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => SelectedItem != null ) );

        public NameOverrideEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public void Serialize( JObject json, bool global = false )
        {
            if ( json == null )
            {
                return;
            }

            JObject obj = new JObject { { "Enabled", Enabled } };

            JArray entries = new JArray();

            foreach ( NameOverrideEntry entry in Items )
            {
                entries.Add( new JObject
                {
                    { "Enabled", entry.Enabled }, { "Serial", entry.Serial }, { "Name", entry.Name }
                } );
            }

            obj.Add( "Entries", entries );

            json.Add( "NameOverride", obj );
        }

        public void Deserialize( JObject json, Options options, bool global = false )
        {
            if ( json?["NameOverride"] == null )
            {
                return;
            }

            JToken config = json["NameOverride"];

            Items.Clear();

            Enabled = config["Enabled"]?.ToObject<bool>() ?? false;

            if ( config["Entries"] == null )
            {
                return;
            }

            foreach ( JToken token in config["Entries"] )
            {
                bool enabled = token["Enabled"]?.ToObject<bool>() ?? false;
                int serial = token["Serial"]?.ToObject<int>() ?? -1;
                string name = token["Name"]?.ToObject<string>() ?? string.Empty;

                NameOverrideEntry entry = new NameOverrideEntry { Enabled = enabled, Serial = serial, Name = name };

                if ( serial != -1 )
                {
                    Items.Add( entry );
                }
            }
        }

        private void Remove( object obj )
        {
            if ( !( obj is NameOverrideEntry entry ) )
            {
                return;
            }

            _dispatcher.Invoke( () => { Items.Remove( entry ); } );
        }

        private void AddEmpty( object obj )
        {
            _dispatcher.Invoke( () => { Items.Add( new NameOverrideEntry { Enabled = false, Name = Strings.Name } ); } );
        }

        private async Task Add( object arg )
        {
            int serial = await Commands.GetTargetSerialAsync();

            if ( serial != 0 )
            {
                if ( !UOMath.IsMobile( serial ) )
                {
                    Commands.SystemMessage( Strings.Mobile_not_found___ );
                    return;
                }

                Entity entity = UOMath.IsMobile( serial )
                    ? (Entity) Engine.Mobiles.GetMobile( serial )
                    : Engine.Items.GetItem( serial );

                NameOverrideEntry entry = new NameOverrideEntry
                {
                    Enabled = true, Serial = serial, Name = entity != null ? entity.Name : Strings.Unknown
                };

                _dispatcher.Invoke( () => { Items.Add( entry ); } );
            }
        }
    }
}