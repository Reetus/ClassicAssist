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
using System.Windows;
using System.Windows.Input;
using ClassicAssist.Browser.Data;
using ClassicAssist.Browser.Models;
using ClassicAssist.Data.Macros;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels;
using Microsoft.Scripting.Utils;

namespace ClassicAssist.Browser
{
    public class MacroBrowserViewModel : BaseViewModel
    {
        private readonly Dictionary<string, Func<Task<string[]>>>
            _data = new Dictionary<string, Func<Task<string[]>>>();

        private readonly Database _database = Database.GetInstance();
        private string _authorFilter;
        private ObservableCollection<string> _authors = new ObservableCollection<string>();

        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        private Category _categoryFilter;

        private ICommand _clearCommand;
        private ICommand _copyToClipboardCommand;
        private ICommand _createMacroCommand;
        private string _eraFilter;
        private ObservableCollection<string> _eras = new ObservableCollection<string>();

        private bool _loading = true;
        private ObservableCollection<Metadata> _macros = new ObservableCollection<Metadata>();
        private ICommand _openGithubCommand;
        private Metadata _selectedItem;
        private string _shardFilter;
        private ObservableCollection<string> _shards = new ObservableCollection<string>();

        public MacroBrowserViewModel()
        {
            _data.Add( nameof( Shards ), async () => await _database.GetShards() );
            _data.Add( nameof( Eras ), async () => await _database.GetEras() );
            _data.Add( nameof( Authors ), async () => await _database.GetAuthors() );

            Task.Run( async () =>
            {
                try
                {
                    await RefreshData();
                }
                catch ( Exception e )
                {
                    Console.WriteLine( e.ToString() );
                }
                finally
                {
                    Loading = false;
                }
            } );
        }

        public string AuthorFilter
        {
            get => _authorFilter;
            set
            {
                SetProperty( ref _authorFilter, value );
                ApplyFilter();
            }
        }

        public ObservableCollection<string> Authors
        {
            get => _authors;
            set => SetProperty( ref _authors, value );
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty( ref _categories, value );
        }

        public Category CategoryFilter
        {
            get => _categoryFilter;
            set
            {
                SetProperty( ref _categoryFilter, value );
                ApplyFilter();
            }
        }

        public ICommand ClearCommand => _clearCommand ?? ( _clearCommand = new RelayCommand( ClearFilter, o => true ) );

        public ICommand CopyToClipboardCommand =>
            _copyToClipboardCommand ??
            ( _copyToClipboardCommand = new RelayCommand( CopyToClipboard, o => SelectedItem != null ) );

        public ICommand CreateMacroCommand =>
            _createMacroCommand ??
            ( _createMacroCommand = new RelayCommandAsync( CreateMacro, o => SelectedItem != null ) );

        public string EraFilter
        {
            get => _eraFilter;
            set
            {
                SetProperty( ref _eraFilter, value );
                ApplyFilter();
            }
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

        public ObservableCollection<Metadata> Macros
        {
            get => _macros;
            set => SetProperty( ref _macros, value );
        }

        public ICommand OpenGithubCommand =>
            _openGithubCommand ?? ( _openGithubCommand = new RelayCommand( OpenGithub, o => SelectedItem != null ) );

        public Metadata SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty( ref _selectedItem, value );
                GetMacro( value ).ConfigureAwait( false );
            }
        }

        public string ShardFilter
        {
            get => _shardFilter;
            set
            {
                SetProperty( ref _shardFilter, value );
                ApplyFilter();
            }
        }

        public ObservableCollection<string> Shards
        {
            get => _shards;
            set => SetProperty( ref _shards, value );
        }

        private async Task CreateMacro( object obj )
        {
            if ( !( obj is Metadata macro ) )
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show( Strings.Macro_Warning, Strings.Warning, MessageBoxButton.YesNo,
                MessageBoxImage.Warning );

            if ( result != MessageBoxResult.Yes )
            {
                return;
            }

            if ( string.IsNullOrEmpty( macro.Macro ) )
            {
                macro.Macro = await _database.GetMacroById( macro.Id );
            }

            MacroManager.GetInstance().NewPublicMacro?.Invoke( macro );
        }

        private static void CopyToClipboard( object obj )
        {
            if ( !( obj is string macro ) )
            {
                return;
            }

            try
            {
                Clipboard.SetText( macro );
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        private void ClearFilter( object obj )
        {
            if ( !( obj is FilterType filterType ) )
            {
                return;
            }

            switch ( filterType )
            {
                case FilterType.Shard:
                    ShardFilter = null;
                    break;
                case FilterType.Era:
                    EraFilter = null;
                    break;
                case FilterType.Author:
                    AuthorFilter = null;
                    break;
                case FilterType.Category:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            List<Filter> filters = new List<Filter>
            {
                new Filter { FilterType = FilterType.Shard, Value = ShardFilter },
                new Filter { FilterType = FilterType.Era, Value = EraFilter },
                new Filter { FilterType = FilterType.Author, Value = AuthorFilter },
                new Filter { FilterType = FilterType.Category, Category = CategoryFilter }
            };

            _database.GetMacros( filters ).ContinueWith( t =>
            {
                _dispatcher.Invoke( () =>
                {
                    Macros.Clear();
                    Macros.AddRange( t.Result );
                } );
            } );
        }

        private async Task GetMacro( Metadata value )
        {
            if ( value == null )
            {
                return;
            }

            try
            {
                Loading = true;
                string macro = await _database.GetMacroById( value.Id );

                value.Macro = macro;
            }
            catch ( Exception e )
            {
                MessageBox.Show( $"{Strings.Failed_to_download_macro___}\n\n{e.Message}", Strings.Error,
                    MessageBoxButton.OK, MessageBoxImage.Error );
            }
            finally
            {
                Loading = false;
            }
        }

        private async Task RefreshData()
        {
            try
            {
                Macros = new ObservableCollection<Metadata>( await _database.GetMacros() );

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

                await _dispatcher.InvokeAsync( async () => Categories.AddRange( await _database.GetCategories() ) );
            }
            catch ( Exception e )
            {
                MessageBox.Show( e.Message, Strings.Error );
            }
        }

        private void OpenGithub( object obj )
        {
            if ( !( obj is string macro ) )
            {
                return;
            }

            _database.OpenGithubMacroURL( macro );
        }
    }
}