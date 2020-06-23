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
using System.Reflection;
using System.Threading.Tasks;
using ClassicAssist.MacroBrowser.Data;
using ClassicAssist.UI.ViewModels;
using Microsoft.Scripting.Utils;

namespace ClassicAssist.MacroBrowser
{
    public class MainViewModel : BaseViewModel
    {
        private readonly Dictionary<string, Func<Task<string[]>>>
            _data = new Dictionary<string, Func<Task<string[]>>>();

        private readonly Database _database = Database.GetInstance();
        private ObservableCollection<string> _authors = new ObservableCollection<string>();
        private ObservableCollection<string> _categories = new ObservableCollection<string>();
        private ObservableCollection<string> _eras = new ObservableCollection<string>();

        private bool _loading = true;
        private ObservableCollection<string> _macros = new ObservableCollection<string>();
        private string _selectedItem;
        private string _selectedMacro;
        private ObservableCollection<string> _shards = new ObservableCollection<string>();

        public MainViewModel()
        {
            _data.Add( nameof( Macros ), async () => await _database.GetMacros() );
            _data.Add( nameof( Shards ), async () => await _database.GetShards() );
            _data.Add( nameof( Eras ), async () => await _database.GetEras() );
            _data.Add( nameof( Authors ), async () => await _database.GetAuthors() );
            _data.Add( nameof( Categories ), async () => await _database.GetCategories() );

            Task.Run( async () =>
            {
                await RefreshData();

                Loading = false;
            } );
        }

        public ObservableCollection<string> Authors
        {
            get => _authors;
            set => SetProperty( ref _authors, value );
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty( ref _categories, value );
        }

        public ObservableCollection<string> Eras
        {
            get => _eras;
            set => SetProperty( ref _eras, value );
        }

        public bool Loading
        {
            get => _loading;
            set => SetProperty( ref _loading, value );
        }

        public ObservableCollection<string> Macros
        {
            get => _macros;
            set => SetProperty( ref _macros, value );
        }

        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty( ref _selectedItem, value );
                GetMacro( value ).ConfigureAwait( false );
            }
        }

        public string SelectedMacro
        {
            get => _selectedMacro;
            set => SetProperty( ref _selectedMacro, value );
        }

        public ObservableCollection<string> Shards
        {
            get => _shards;
            set => SetProperty( ref _shards, value );
        }

        private async Task GetMacro( string value )
        {
            Loading = true;
            string macro = await _database.GetMacroByName( value );

            SelectedMacro = macro;

            Loading = false;
        }

        private async Task RefreshData()
        {
            try
            {
                foreach ( KeyValuePair<string, Func<Task<string[]>>> kvp in _data )
                {
                    PropertyInfo property =
                        GetType().GetProperty( kvp.Key, BindingFlags.Instance | BindingFlags.Public );

                    if ( property == null )
                    {
                        continue;
                    }

                    string[] data = await kvp.Value.Invoke();

                    if ( data == null )
                    {
                        continue;
                    }

                    ObservableCollection<string> obj = (ObservableCollection<string>) property.GetValue( this );

                    if ( obj == null )
                    {
                        continue;
                    }

                    _dispatcher.Invoke( () =>
                    {
                        obj.Clear();
                        obj.AddRange( data );
                    } );
                }
            }
            catch ( Exception e )
            {
            }
        }
    }
}