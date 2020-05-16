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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.ViewModels.Autoloot
{
    public class PropertySelectionViewModel : BaseViewModel
    {
        private DialogResult _dialogResult = DialogResult.Cancel;
        private ICommand _okCommand;
        private ObservableCollection<SelectProperties> _properties = new ObservableCollection<SelectProperties>();

        public PropertySelectionViewModel()
        {
        }

        public PropertySelectionViewModel( IEnumerable<Property> properties )
        {
            foreach ( Property property in properties )
            {
                Properties.Add( new SelectProperties
                {
                    Name = Cliloc.GetProperty( property.Cliloc ), Property = property, Selected = false
                } );
            }
        }

        public DialogResult DialogResult
        {
            get => _dialogResult;
            set => SetProperty( ref _dialogResult, value );
        }

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK, o => true ) );

        public ObservableCollection<SelectProperties> Properties
        {
            get => _properties;
            set => SetProperty( ref _properties, value );
        }

        private void OK( object obj )
        {
            DialogResult = DialogResult.OK;
        }
    }

    public class SelectProperties
    {
        public string Name { get; set; }
        public Property Property { get; set; }
        public bool Selected { get; set; }
    }
}