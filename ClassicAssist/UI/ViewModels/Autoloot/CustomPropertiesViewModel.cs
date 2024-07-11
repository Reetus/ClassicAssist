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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views.Autoloot;
using ClassicAssist.UO;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;

namespace ClassicAssist.UI.ViewModels.Autoloot
{
    public class CustomPropertiesViewModel : BaseViewModel
    {
        private readonly string _propertiesFileCustom = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data", "Properties.Custom.json" );

        private ICommand _chooseFromItemCommand;
        private ObservableCollection<CustomProperty> _properties = new ObservableCollection<CustomProperty>();
        private ICommand _removeCommand;
        private ICommand _saveCommand;
        private CustomProperty _selectedProperty;

        public CustomPropertiesViewModel()
        {
            LoadCustomProperties();
        }

        public ICommand ChooseFromClilocCommand => new RelayCommand( ChooseFromCliloc, o => true );

        public ICommand ChooseFromItemCommand => _chooseFromItemCommand ?? ( _chooseFromItemCommand = new RelayCommandAsync( ChooseFromItem, o => true ) );

        public ObservableCollection<CustomProperty> Properties
        {
            get => _properties;
            set => SetProperty( ref _properties, value );
        }

        public ICommand RemoveCommand => _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => SelectedProperty != null ) );

        public ICommand SaveCommand => _saveCommand ?? ( _saveCommand = new RelayCommand( Save, o => true ) );

        public CustomProperty SelectedProperty
        {
            get => _selectedProperty;
            set => SetProperty( ref _selectedProperty, value );
        }

        private void ChooseFromCliloc( object obj )
        {
            ClilocSelectionViewModel vm = new ClilocSelectionViewModel();
            ClilocSelectionWindow window = new ClilocSelectionWindow { DataContext = vm };

            window.ShowDialog();

            if ( vm.DialogResult != DialogResult.OK )
            {
                return;
            }

            if ( Properties.Any( p => p.Cliloc == vm.SelectedCliloc.Key ) )
            {
                return;
            }

            Properties.AddSorted( new CustomProperty { Cliloc = vm.SelectedCliloc.Key, Name = vm.SelectedCliloc.Value, Arguments = vm.SelectedCliloc.Value.Contains( "~" ) } );
        }

        private async Task ChooseFromItem( object obj )
        {
            int serial = await Commands.GetTargetSerialAsync( Strings.Target_object___, 90000 );

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
                if ( Properties.Any( p => p.Cliloc == property.Property.Cliloc ) )
                {
                    continue;
                }

                Properties.AddSorted( new CustomProperty
                {
                    Name = property.Name,
                    Cliloc = property.Property.Cliloc,
                    Arguments = property.Property.Arguments != null && property.Property.Arguments.Length > 0,
                    ArgumentIndex = property.Property.Arguments != null ? 0 : -1
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
                ClilocIndex = property.ArgumentIndex, Clilocs = new[] { property.Cliloc }, ConstraintType = 0, Name = property.Name
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
                            Name = constraint.Name, Cliloc = constraint.Clilocs[0], Arguments = constraint.ClilocIndex >= 0, ArgumentIndex = constraint.ClilocIndex
                        };

                        Properties.AddSorted( customProperty );
                    }
                }
            }
        }
    }

    public class CustomProperty : IComparable<CustomProperty>, INotifyPropertyChanged
    {
        private bool _arguments;
        private int _argumentIndex = -1;

        public int ArgumentIndex
        {
            get => _argumentIndex;
            set => SetField( ref _argumentIndex, value );
        }

        public bool Arguments
        {
            get => _arguments;
            set
            {
                switch ( value )
                {
                    case false when ArgumentIndex != -1:
                        ArgumentIndex = -1;
                        break;
                    case true when ArgumentIndex < 0:
                        ArgumentIndex = 0;
                        break;
                }

                SetField( ref _arguments, value );
            }
        }

        public int Cliloc { get; set; }
        public string Name { get; set; }

        public int CompareTo( CustomProperty other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            return ReferenceEquals( null, other ) ? 1 : string.Compare( Name, other.Name, StringComparison.InvariantCultureIgnoreCase );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected bool SetField<T>( ref T field, T value, [CallerMemberName] string propertyName = null )
        {
            if ( EqualityComparer<T>.Default.Equals( field, value ) ) return false;
            field = value;
            OnPropertyChanged( propertyName );
            return true;
        }
    }
}