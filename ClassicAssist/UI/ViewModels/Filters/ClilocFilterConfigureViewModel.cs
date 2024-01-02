using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ClassicAssist.Data.Filters;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.ViewModels.Filters
{
    public class ClilocFilterConfigureViewModel : BaseViewModel
    {
        private ICommand _addItemCommand;
        private ObservableCollection<ClilocEntry> _allClilocs = new ObservableCollection<ClilocEntry>();
        private ObservableCollection<ClilocEntry> _filteredClilocs = new ObservableCollection<ClilocEntry>();
        private string _filterText;
        private ObservableCollection<FilterClilocEntry> _items = new ObservableCollection<FilterClilocEntry>();
        private ICommand _removeItemCommand;
        private ICommand _saveItemsCommand;
        private ClilocEntry _selectedClilocEntry;
        private FilterClilocEntry _selectedItem;

        public ClilocFilterConfigureViewModel()
        {
            if ( DesignerProperties.GetIsInDesignMode( new DependencyObject() ) )
            {
                for ( int i = 0; i < 10; i++ )
                {
                    AllClilocs.Add( new ClilocEntry { Key = i, Value = $"Cliloc {i}" } );
                }

                Items.Add( new FilterClilocEntry { Cliloc = 100, Replacement = "Replacement" } );
            }
            else
            {
                foreach ( KeyValuePair<int, string> kvp in Cliloc.GetItems() )
                {
                    AllClilocs.Add( new ClilocEntry { Key = kvp.Key, Value = kvp.Value } );
                }
            }

            foreach ( KeyValuePair<int, string> filterFilter in ClilocFilter.Filters )
            {
                Items.Add( new FilterClilocEntry { Cliloc = filterFilter.Key, Replacement = filterFilter.Value } );
            }

            UpdateEntries( string.Empty );
        }

        public ICommand AddItemCommand => _addItemCommand ?? ( _addItemCommand = new RelayCommand( AddItem ) );

        public ObservableCollection<ClilocEntry> AllClilocs
        {
            get => _allClilocs;
            set => SetProperty( ref _allClilocs, value );
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
                UpdateEntries( _filterText );
            }
        }

        public ObservableCollection<FilterClilocEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand RemoveItemCommand =>
            _removeItemCommand ?? ( _removeItemCommand = new RelayCommand( RemoveItem, o => SelectedItem != null ) );

        public ICommand SaveItemsCommand =>
            _saveItemsCommand ?? ( _saveItemsCommand = new RelayCommand( SaveItems, o => true ) );

        public ClilocEntry SelectedClilocEntry
        {
            get => _selectedClilocEntry;
            set => SetProperty( ref _selectedClilocEntry, value );
        }

        public FilterClilocEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        private void RemoveItem( object obj )
        {
            if ( !( obj is FilterClilocEntry entry ) )
            {
                return;
            }

            Items.Remove( entry );
        }

        private void SaveItems( object obj )
        {
            ClilocFilter.Filters.Clear();

            foreach ( FilterClilocEntry entry in Items )
            {
                if ( !ClilocFilter.Filters.ContainsKey( entry.Cliloc ) )
                {
                    ClilocFilter.Filters.Add( entry.Cliloc, entry.Replacement );
                }
            }
        }

        private void UpdateEntries( string filterText )
        {
            IEnumerable<ClilocEntry> matches = AllClilocs.Where( m =>
                string.IsNullOrEmpty( filterText ) || m.Value.ToLower().Contains( filterText.ToLower() ) );
            FilteredClilocs.Clear();

            foreach ( ClilocEntry clilocEntry in matches )
            {
                FilteredClilocs.Add( clilocEntry );
            }
        }

        private void AddItem( object obj )
        {
            if ( !( obj is ClilocEntry clilocEntry ) )
            {
                return;
            }

            if ( Items.Any( i => i.Cliloc == clilocEntry.Key ) )
            {
                return;
            }

            Items.Add( new FilterClilocEntry { Cliloc = clilocEntry.Key, Replacement = clilocEntry.Value } );
        }
    }

    public class ClilocEntry
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

    public class FilterClilocEntry
    {
        public int Cliloc { get; set; }

        public string Original =>
            DesignerProperties.GetIsInDesignMode( new DependencyObject() )
                ? Replacement
                : UO.Data.Cliloc.GetProperty( Cliloc );

        public string Replacement { get; set; }
    }
}