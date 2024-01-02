using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ClassicAssist.Annotations;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for HuePickerWindow.xaml
    /// </summary>
    public partial class SingleHuePickerWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<HuePickerEntry> _filteredItems = new ObservableCollection<HuePickerEntry>();
        private string _filterText;
        private ObservableCollection<HuePickerEntry> _items = new ObservableCollection<HuePickerEntry>();
        private ICommand _okCommand;
        private int _selectedHue = -1;
        private HuePickerEntry _selectedItem;

        public SingleHuePickerWindow()
        {
            InitializeComponent();

            for ( int i = 0; i < 3000; i++ )
            {
                Items.Add( new HuePickerEntry { Index = i + 1, Entry = Hues._lazyHueEntries.Value[i] } );
            }

            SelectedItem = Items.FirstOrDefault( i => i.Index == SelectedHue );

            ApplyFilter( _filterText );
        }

        public ObservableCollection<HuePickerEntry> FilteredItems
        {
            get => _filteredItems;
            set => SetProperty( ref _filteredItems, value );
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                SetProperty( ref _filterText, value );
                ApplyFilter( value );
            }
        }

        public ObservableCollection<HuePickerEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK, o => SelectedItem != null ) );

        public int SelectedHue
        {
            get => _selectedHue;
            set => SetProperty( ref _selectedHue, value );
        }

        public HuePickerEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ApplyFilter( string value )
        {
            FilteredItems = new ObservableCollection<HuePickerEntry>( Items.Where( i =>
                string.IsNullOrEmpty( value ) || i.Index.ToString().StartsWith( value ) ) );
        }

        public static bool GetHue( out int hue )
        {
            HuePickerWindow window = new HuePickerWindow();

            window.ShowDialog();

            hue = window.SelectedHue;

            return hue != -1;
        }

        private void OK( object obj )
        {
            if ( !( obj is HuePickerEntry entry ) )
            {
                return;
            }

            SelectedHue = entry.Index;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        public virtual void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            if ( obj != null && obj.Equals( value ) )
            {
                return;
            }

            obj = value;
            OnPropertyChanged( propertyName );
        }

        public class HuePickerEntry
        {
            public HueEntry Entry { get; set; }
            public string EntryName => Entry.Name ?? "Unknown";
            public int Index { get; set; }
        }
    }
}