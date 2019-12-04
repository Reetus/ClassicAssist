using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ClassicAssist.Misc;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.ViewModels
{
    public class EntityCollectionViewerViewModel : BaseViewModel
    {
        private ObservableCollection<EntityCollectionData> _entities;
        private ICommand _itemDoubleClickCommand;
        private bool _showProperties;
        private bool _topmost;

        public EntityCollectionViewerViewModel()
        {
            Entities = new ObservableCollection<EntityCollectionData>
            {
                new EntityCollectionData { Entity = new Item( 1 ) },
                new EntityCollectionData { Entity = new Item( 2 ) }
            };
        }

        public EntityCollectionViewerViewModel( ItemCollection collection )
        {
            Entities = new ObservableCollection<EntityCollectionData>( collection.ToEntityCollectionData() );
        }

        public ObservableCollection<EntityCollectionData> Entities
        {
            get => _entities;
            set => SetProperty( ref _entities, value );
        }

        public ICommand ItemDoubleClickCommand =>
            _itemDoubleClickCommand ?? ( _itemDoubleClickCommand = new RelayCommand( ItemDoubleClick, o => true ) );

        public bool ShowProperties
        {
            get => _showProperties;
            set => SetProperty( ref _showProperties, value );
        }

        public bool Topmost
        {
            get => _topmost;
            set => SetProperty( ref _topmost, value );
        }

        private void ItemDoubleClick( object obj )
        {
            if ( !( obj is EntityCollectionData ecd ) )
            {
                return;
            }

            ObjectInspectorWindow window =
                new ObjectInspectorWindow { DataContext = new ObjectInspectorViewModel( ecd.Entity ) };

            window.ShowDialog();
        }
    }

    public class EntityCollectionData
    {
        public BitmapSource Bitmap => Art.GetStatic( Entity.ID, Entity.Hue ).ToBitmapSource();
        public Entity Entity { get; set; }
        public string Name => Entity.Name;
    }

    public static class ExtensionMethods
    {
        public static List<EntityCollectionData> ToEntityCollectionData( this ItemCollection itemCollection )
        {
            Item[] items = itemCollection.GetItems();

            IEnumerable<Item> noNames = items.Where( i => string.IsNullOrEmpty( i.Name ) );

            foreach ( Item item in noNames )
            {
                item.Name = $"0x{item.Serial:x8}";
            }

            return items.Select( item => new EntityCollectionData { Entity = item } ).ToList();
        }
    }
}