using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ClassicAssist.Annotations;
using ClassicAssist.UO.Data;
using ReactiveUI;

namespace ClassicAssist.Avalonia.Views
{
    public class HuePickerWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<HuePickerEntry> _filteredItems = new ObservableCollection<HuePickerEntry>();
        private string _filterText;
        private ObservableCollection<HuePickerEntry> _items = new ObservableCollection<HuePickerEntry>();
        private ICommand _okCommand;
        private int _selectedHue;
        private HuePickerEntry _selectedItem;

        public HuePickerWindow()
        {
            InitializeComponent();

            Hues.Initialize( @"C:\Users\johns\Documents\UO\Ultima Online Classic" );

            for ( int i = 0; i < 3000; i++ )
            {
                Items.Add( new HuePickerEntry { Index = i + 1, Entry = Hues._lazyHueEntries.Value[i] } );
            }

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

        public ICommand OKCommand =>
            _okCommand ?? ( _okCommand = ReactiveCommand.Create<HuePickerEntry>( OK,
                this.WhenAnyValue( e => e.SelectedItem, selector: e => e != null ) ) );

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

        public new event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        public virtual void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
        }

        private void OK( object obj )
        {
            if ( !( obj is HuePickerEntry entry ) )
            {
                return;
            }

            SelectedHue = entry.Index;
        }

        private void ApplyFilter( string value )
        {
            FilteredItems = new ObservableCollection<HuePickerEntry>( Items.Where( i =>
                string.IsNullOrEmpty( value ) || i.Index.ToString().StartsWith( value ) ) );
        }
    }

    public class HuePickerEntry
    {
        public HueEntry Entry { get; set; }
        public string EntryName => Entry.Name ?? "Unknown";
        public int Index { get; set; }
    }
}