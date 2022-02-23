using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ClassicAssist.Data.Filters;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.Filters
{
    /// <summary>
    ///     Interaction logic for TextFilterConfigureMessageTypesWindow.xaml
    /// </summary>
    public partial class TextFilterConfigureMessageTypesWindow
    {
        private ICommand _okCommand;

        public TextFilterConfigureMessageTypesWindow( TextFilterEntry textFilterEntry )
        {
            MessageTypes = textFilterEntry.MessageTypes;

            Items = ConvertFromFlags( textFilterEntry.MessageTypes );

            InitializeComponent();
        }

        public ObservableCollection<Item> Items { get; set; }
        public TextFilterMessageType MessageTypes { get; set; }

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK ) );

        private static ObservableCollection<Item> ConvertFromFlags( TextFilterMessageType messageTypes )
        {
            ObservableCollection<Item> items = new ObservableCollection<Item>();

            IEnumerable<TextFilterMessageType> values = Enum.GetValues( typeof( TextFilterMessageType ) )
                .Cast<TextFilterMessageType>().Where( e =>
                    !new[] { TextFilterMessageType.None, TextFilterMessageType.All }.Contains( e ) );

            foreach ( TextFilterMessageType value in values )
            {
                items.Add( new Item { MessageType = value, Enabled = messageTypes.HasFlag( value ) } );
            }

            return items;
        }

        private void OK( object obj )
        {
            MessageTypes = ConvertToFlags( Items );
        }

        private static TextFilterMessageType ConvertToFlags( IEnumerable<Item> items )
        {
            return items.Where( e => e.Enabled ).Aggregate( TextFilterMessageType.None,
                ( current, item ) => current | item.MessageType );
        }
    }

    public class Item
    {
        public bool Enabled { get; set; }
        public TextFilterMessageType MessageType { get; set; }
    }
}