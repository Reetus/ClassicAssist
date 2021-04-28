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
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.ViewModels.Autoloot
{
    public class ClilocSelectionViewModel : BaseViewModel
    {
        private ObservableCollection<ClilocEntry> _allClilocs = new ObservableCollection<ClilocEntry>();
        private DialogResult _dialogResult = DialogResult.Cancel;
        private ObservableCollection<ClilocEntry> _filteredClilocs = new ObservableCollection<ClilocEntry>();
        private string _filterText;
        private ICommand _okCommand;
        private ClilocEntry _selectedCliloc;

        public ClilocSelectionViewModel()
        {
            foreach ( KeyValuePair<int, string> kvp in Cliloc.GetItems() )
            {
                AllClilocs.Add( new ClilocEntry { Key = kvp.Key, Value = kvp.Value } );
            }

            UpdateEntries( _filterText );
        }

        public ObservableCollection<ClilocEntry> AllClilocs
        {
            get => _allClilocs;
            set => SetProperty( ref _allClilocs, value );
        }

        public DialogResult DialogResult
        {
            get => _dialogResult;
            set => SetProperty( ref _dialogResult, value );
        }

        public ObservableCollection<ClilocEntry> FilteredClilocs
        {
            get => _filteredClilocs;
            set => SetProperty( ref _filteredClilocs, value );
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                SetProperty( ref _filterText, value );
                UpdateEntries( value );
            }
        }

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK, o => SelectedCliloc != null ) );

        public ClilocEntry SelectedCliloc
        {
            get => _selectedCliloc;
            set => SetProperty( ref _selectedCliloc, value );
        }

        private void OK( object obj )
        {
            DialogResult = DialogResult.OK;
        }

        private void UpdateEntries( string filterText )
        {
            IEnumerable<ClilocEntry> matches = AllClilocs.Where( m =>
                string.IsNullOrEmpty( filterText ) || m.Key.ToString().Contains( filterText ) ||
                m.Value.ToLower().Contains( filterText.ToLower() ) );

            FilteredClilocs.Clear();

            foreach ( ClilocEntry clilocEntry in matches )
            {
                FilteredClilocs.Add( clilocEntry );
            }
        }

        public class ClilocEntry
        {
            public int Key { get; set; }
            public string Value { get; set; }
        }
    }
}