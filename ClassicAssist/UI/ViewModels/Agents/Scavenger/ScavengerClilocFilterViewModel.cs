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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using ClassicAssist.Data.Scavenger;
using ClassicAssist.Shared.UI;
using Microsoft.Scripting.Utils;

namespace ClassicAssist.UI.ViewModels.Agents.Scavenger
{
    public class ScavengerClilocFilterViewModel : BaseViewModel
    {
        private ICommand _addCommand;
        private bool _enabled;

        private ObservableCollection<ScavengerClilocFilterEntry> _items =
            new ObservableCollection<ScavengerClilocFilterEntry>();

        private ICommand _okCommand;
        private ICommand _removeCommand;

        public ScavengerClilocFilterViewModel()
        {
        }

        public ScavengerClilocFilterViewModel( bool enabled, IEnumerable<ScavengerClilocFilterEntry> items )
        {
            Enabled = enabled;
            Items.Clear();
            Items.AddRange( items );

            if ( !Items.Any() )
            {
                Items.AddRange( new[]
                {
                    new ScavengerClilocFilterEntry { Cliloc = 501643 },
                    new ScavengerClilocFilterEntry { Cliloc = 501644 }
                } );
            }
        }

        public ICommand AddCommand => _addCommand ?? ( _addCommand = new RelayCommand( Add, o => true ) );

        public DialogResult DialogResult { get; set; } = DialogResult.Cancel;

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public ObservableCollection<ScavengerClilocFilterEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand OkCommand => _okCommand ?? ( _okCommand = new RelayCommand( Ok, o => true ) );

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => o != null ) );

        private void Ok( object obj )
        {
            DialogResult = DialogResult.OK;
        }

        private void Add( object obj )
        {
            Items.Add( new ScavengerClilocFilterEntry { Cliloc = 501643 } );
        }

        private void Remove( object obj )
        {
            if ( !( obj is ScavengerClilocFilterEntry filterEntry ) )
            {
                return;
            }

            Items.Remove( filterEntry );
        }
    }
}