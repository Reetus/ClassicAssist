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

using System.Collections.ObjectModel;
using System.Windows.Input;
using ClassicAssist.Data.Filters;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views.Filters;
using Microsoft.Scripting.Utils;

namespace ClassicAssist.UI.ViewModels.Filters
{
    public class TextFilterConfigureViewModel : BaseViewModel
    {
        private RelayCommand _addEntryCommand;
        private ICommand _configureMessageTypesCommand;
        private ICommand _okCommand;
        private ICommand _removeEntryCommand;

        public TextFilterConfigureViewModel()
        {
            Items.AddRange( TextFilter.Filters );
        }

        public ICommand AddEntryCommand =>
            _addEntryCommand ?? ( _addEntryCommand = new RelayCommand( AddEntry, o => true ) );

        public ICommand ConfigureMessageTypesCommand =>
            _configureMessageTypesCommand ??
            ( _configureMessageTypesCommand = new RelayCommand( ConfigureMessageTypes ) );

        public ObservableCollection<TextFilterEntry> Items { get; set; } = new ObservableCollection<TextFilterEntry>();

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK, o => true ) );

        public ICommand RemoveEntryCommand =>
            _removeEntryCommand ?? ( _removeEntryCommand = new RelayCommand( RemoveEntry, o => o != null ) );

        private void AddEntry( object obj )
        {
            Items.Add( new TextFilterEntry() );
        }

        private void OK( object obj )
        {
            TextFilter.Filters.Clear();
            TextFilter.Filters.AddRange( Items );
        }

        private void RemoveEntry( object obj )
        {
            if ( !( obj is TextFilterEntry filterEntry ) )
            {
                return;
            }

            Items.Remove( filterEntry );
        }

        private static void ConfigureMessageTypes( object obj )
        {
            if ( !( obj is TextFilterEntry filterEntry ) )
            {
                return;
            }

            TextFilterConfigureMessageTypesWindow window = new TextFilterConfigureMessageTypesWindow( filterEntry );
            window.ShowDialog();

            filterEntry.MessageTypes = window.MessageTypes;
        }
    }
}