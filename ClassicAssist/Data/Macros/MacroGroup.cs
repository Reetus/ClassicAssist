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
using System.Collections.ObjectModel;
using System.Linq;
using ClassicAssist.UI.Controls.DraggableTreeView;
using ClassicAssist.UI.ViewModels;

namespace ClassicAssist.Data.Macros
{
    public class MacroGroup : SetPropertyNotifyChanged, IDraggableGroup
    {
        private string _name;
        public ObservableCollection<IDraggable> Children { get; set; } = new ObservableCollection<IDraggable>();

        public string Name
        {
            get => _name;
            set
            {
                foreach ( IDraggableEntry draggable in Children.Where( i => i is IDraggableEntry )
                    .Cast<IDraggableEntry>() )
                {
                    draggable.Group = value;
                }

                SetProperty( ref _name, value );
            }
        }

        public int CompareTo( IDraggable other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            if ( ReferenceEquals( null, other ) )
            {
                return 1;
            }

            return string.Compare( Name, other.Name, StringComparison.Ordinal );
        }
    }
}