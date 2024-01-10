using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Data.Filters;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels.Autoloot;
using ClassicAssist.UI.Views;
using ClassicAssist.UI.Views.Autoloot;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.ViewModels.Filters
{
    public class ClilocFilterConfigureViewModel : BaseViewModel
    {
        private ICommand _addItemCommand;
        private ICommand _chooseClilocCommand;
        private ObservableCollection<FilterClilocEntry> _items = new ObservableCollection<FilterClilocEntry>();
        private ICommand _okCommand;
        private ICommand _removeItemCommand;
        private FilterClilocEntry _selectedItem;
        private ICommand _selectHueCommand;

        public ClilocFilterConfigureViewModel()
        {
            ClilocFilter.Filters.ToList().ForEach( f =>
                Items.Add( new FilterClilocEntry { Cliloc = f.Cliloc, Replacement = f.Replacement, Hue = f.Hue } ) );
        }

        public ICommand AddItemCommand => _addItemCommand ?? ( _addItemCommand = new RelayCommand( AddItem ) );

        public ICommand ChooseClilocCommand =>
            _chooseClilocCommand ?? ( _chooseClilocCommand = new RelayCommand( ChooseCliloc ) );

        public ObservableCollection<FilterClilocEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK ) );

        public ICommand RemoveItemCommand =>
            _removeItemCommand ?? ( _removeItemCommand = new RelayCommand( RemoveItem, o => SelectedItem != null ) );

        public FilterClilocEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public ICommand SelectHueCommand => _selectHueCommand ?? ( _selectHueCommand = new RelayCommand( SelectHue ) );

        private void OK( object obj )
        {
            ClilocFilter.Filters.Clear();

            foreach ( FilterClilocEntry entry in Items )
            {
                if ( ClilocFilter.Filters.All( e => e.Cliloc != entry.Cliloc ) )
                {
                    ClilocFilter.Filters.Add( new FilterClilocEntry
                    {
                        Cliloc = entry.Cliloc, Replacement = entry.Replacement, Hue = entry.Hue
                    } );
                }
            }
        }

        private static void ChooseCliloc( object obj )
        {
            if ( !( obj is FilterClilocEntry entry ) )
            {
                return;
            }

            ClilocSelectionViewModel vm = new ClilocSelectionViewModel();
            ClilocSelectionWindow window = new ClilocSelectionWindow { DataContext = vm };

            window.ShowDialog();

            if ( vm.DialogResult != DialogResult.OK )
            {
                return;
            }

            entry.Cliloc = vm.SelectedCliloc.Key;
            entry.Replacement = vm.SelectedCliloc.Value;
        }

        private static void SelectHue( object obj )
        {
            if ( !( obj is FilterClilocEntry entry ) )
            {
                return;
            }

            SingleHuePickerWindow window = new SingleHuePickerWindow
            {
                Topmost = Options.CurrentOptions.AlwaysOnTop, SelectedHue = entry.Hue
            };

            window.ShowDialog();

            if ( window.SelectedHue == -1 )
            {
                return;
            }

            entry.Hue = window.SelectedHue;
        }

        private void RemoveItem( object obj )
        {
            if ( !( obj is FilterClilocEntry entry ) )
            {
                return;
            }

            Items.Remove( entry );
        }

        private void AddItem( object obj )
        {
            Items.Add( new FilterClilocEntry
            {
                Cliloc = 500000, Replacement = Cliloc.GetProperty( 500000 ), Hue = -1
            } );
        }
    }

    public class FilterClilocEntry : SetPropertyNotifyChanged
    {
        private int _cliloc;
        private int _hue = -1;
        private string _replacement;

        public int Cliloc
        {
            get => _cliloc;
            set
            {
                SetProperty( ref _cliloc, value );
                OnPropertyChanged( nameof( Original ) );
            }
        }

        public int Hue
        {
            get => _hue;
            set => SetProperty( ref _hue, value );
        }

        public string Original =>
            DesignerProperties.GetIsInDesignMode( new DependencyObject() )
                ? Replacement
                : UO.Data.Cliloc.GetProperty( Cliloc );

        public string Replacement
        {
            get => _replacement;
            set => SetProperty( ref _replacement, value );
        }
    }
}