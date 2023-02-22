using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using ClassicAssist.Misc;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Data;
using Microsoft.Scripting.Utils;

namespace ClassicAssist.UI.Views.Filters.ItemIDFilter
{
    /// <summary>
    ///     Interaction logic for ItemIDSelectionWindow.xaml
    /// </summary>
    public partial class ItemIDSelectionWindow : INotifyPropertyChanged
    {
        private bool _imagesLoaded;
        private ICommand _okCommand;
        private string _searchText;
        private ImageData _selectedItem;

        public ItemIDSelectionWindow()
        {
            InitializeComponent();

            LoadImageData().ConfigureAwait( false );
        }

        public ObservableCollection<ImageData> FilterImages { get; set; } = new ObservableCollection<ImageData>();
        public ObservableCollection<ImageData> Images { get; set; } = new ObservableCollection<ImageData>();

        public bool ImagesLoaded
        {
            get => _imagesLoaded;
            set => SetField( ref _imagesLoaded, value );
        }

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK, o => o != null ) );

        public bool Result { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetField( ref _searchText, value );
                UpdateFilter();
            }
        }

        public ImageData SelectedItem
        {
            get => _selectedItem;
            set => SetField( ref _selectedItem, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Task LoadImageData()
        {
            return Task.Run( () =>
            {
                List<ImageData> images = new List<ImageData>();

                for ( int i = 0; i < 10000; i++ )
                {
                    ImageSource image = Art.GetStatic( i ).ToImageSource();
                    image.Freeze();

                    images.Add( new ImageData
                    {
                        ID = i, ImageSource = image, Name = TileData.GetStaticTile( i ).Name
                    } );
                }

                ImagesLoaded = true;

                Dispatcher.Invoke( () =>
                {
                    Images.AddRange( images );
                    UpdateFilter();
                } );
            } );
        }

        private void UpdateFilter()
        {
            if ( string.IsNullOrEmpty( SearchText ) )
            {
                FilterImages.AddRange( Images );
                return;
            }

            List<ImageData> filterItems = Images.Where( e => e.Name.Contains( SearchText ) ).ToList();

            if ( int.TryParse( SearchText, out int searchInt ) )
            {
                filterItems.AddRange( Images.Where( e => e.ID.Equals( searchInt ) ) );
            }

            if ( SearchText.StartsWith( "0x" ) && SearchText.Length >= 3 )
            {
                int int32 = Convert.ToInt32( SearchText, 16 );

                filterItems.AddRange( Images.Where( e => e.ID.Equals( int32 ) ) );
            }

            FilterImages.Clear();
            FilterImages.AddRange( filterItems );
        }

        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected bool SetField<T>( ref T field, T value, [CallerMemberName] string propertyName = null )
        {
            if ( EqualityComparer<T>.Default.Equals( field, value ) )
            {
                return false;
            }

            field = value;
            OnPropertyChanged( propertyName );
            return true;
        }

        private void OK( object obj )
        {
            Result = true;
        }

        public class ImageData
        {
            public int ID { get; set; }
            public ImageSource ImageSource { get; set; }
            public string Name { get; set; }
        }
    }
}