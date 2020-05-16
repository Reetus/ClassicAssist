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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Resources;
using ClassicAssist.UI.Views.Autoloot;
using ClassicAssist.UO;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;

namespace ClassicAssist.UI.ViewModels.Autoloot
{
    public class CustomPropertiesViewModel : BaseViewModel
    {
        private readonly string _propertiesFileCustom =
            Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data", "Properties.Custom.json" );

        private ICommand _chooseFromItemCommand;
        private ObservableCollection<CustomProperty> _properties = new ObservableCollection<CustomProperty>();
        private ICommand _removeCommand;
        private ICommand _saveCommand;
        private CustomProperty _selectedProperty;

        public CustomPropertiesViewModel()
        {
            LoadCustomProperties();
        }

        public ICommand ChooseFromItemCommand =>
            _chooseFromItemCommand ?? ( _chooseFromItemCommand = new RelayCommandAsync( ChooseFromItem, o => true ) );

        public ObservableCollection<CustomProperty> Properties
        {
            get => _properties;
            set => SetProperty( ref _properties, value );
        }

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => SelectedProperty != null ) );

        public ICommand SaveCommand => _saveCommand ?? ( _saveCommand = new RelayCommand( Save, o => true ) );

        public CustomProperty SelectedProperty
        {
            get => _selectedProperty;
            set => SetProperty( ref _selectedProperty, value );
        }

        private async Task ChooseFromItem( object obj )
        {
            int serial = await Commands.GetTargeSerialAsync( Strings.Target_object___, 90000 );

            if ( serial == 0 )
            {
                Commands.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                Commands.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            if ( item.Properties == null )
            {
                Commands.SystemMessage( Strings.Item_properties_null_or_not_loaded___ );
                return;
            }

            PropertySelectionViewModel vm = new PropertySelectionViewModel( item.Properties );
            PropertySelectionWindow window = new PropertySelectionWindow { DataContext = vm };
            window.ShowDialog();

            if ( vm.DialogResult != DialogResult.OK )
            {
                return;
            }

            IEnumerable<SelectProperties> selectedProperties = vm.Properties.Where( p => p.Selected );

            foreach ( SelectProperties property in selectedProperties )
            {
                Properties.Add( new CustomProperty
                {
                    Name = property.Name,
                    Cliloc = property.Property.Cliloc,
                    Arguments = property.Property.Arguments != null && property.Property.Arguments.Length > 0
                } );
            }
        }

        private void Remove( object obj )
        {
            if ( SelectedProperty == null )
            {
                return;
            }

            Properties.Remove( SelectedProperty );
        }

        private void Save( object obj )
        {
            SaveCustomProperties();
        }

        private void SaveCustomProperties()
        {
            List<PropertyEntry> properties = Properties.Select( property => new PropertyEntry
            {
                ClilocIndex = property.Arguments ? 0 : -1,
                Clilocs = new[] { property.Cliloc },
                ConstraintType = 0,
                Name = property.Name
            } ).ToList();

            File.WriteAllText( _propertiesFileCustom, JsonConvert.SerializeObject( properties ) );
        }

        private void LoadCustomProperties()
        {
            if ( !File.Exists( _propertiesFileCustom ) )
            {
                return;
            }

            JsonSerializer serializer = new JsonSerializer();

            using ( StreamReader sr = new StreamReader( _propertiesFileCustom ) )
            {
                using ( JsonTextReader reader = new JsonTextReader( sr ) )
                {
                    PropertyEntry[] constraints = serializer.Deserialize<PropertyEntry[]>( reader );

                    foreach ( PropertyEntry constraint in constraints )
                    {
                        CustomProperty customProperty = new CustomProperty
                        {
                            Name = constraint.Name,
                            Cliloc = constraint.Clilocs[0],
                            Arguments = constraint.ClilocIndex == 0
                        };

                        Properties.Add( customProperty );
                    }
                }
            }
        }
    }

    public class CustomProperty
    {
        public bool Arguments { get; set; }
        public int Cliloc { get; set; }
        public string Name { get; set; }
    }
}