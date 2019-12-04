using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Counters;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class CountersTabViewModel : BaseViewModel, ISettingProvider
    {
        private ICommand _insertEntryCommand;
        private ObservableCollection<CountersAgentEntry> _items = new ObservableCollection<CountersAgentEntry>();
        private ICommand _recountCommand;
        private ICommand _removeEntryCommand;
        private CountersAgentEntry _selectedItem;
        private bool _warn = true;
        private int _warnAmount;

        public CountersTabViewModel()
        {
            CountersManager manager = CountersManager.GetInstance();

            manager.Items = Items;
            manager.RecountAll = () => Recount( null );
        }

        public ICommand InsertEntryCommand =>
            _insertEntryCommand ?? ( _insertEntryCommand = new RelayCommandAsync( InsertEntry, o => true ) );

        public ObservableCollection<CountersAgentEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand RecountCommand =>
            _recountCommand ?? ( _recountCommand = new RelayCommand( Recount, o => Items.Count > 0 ) );

        public ICommand RemoveEntryCommand =>
            _removeEntryCommand ?? ( _removeEntryCommand = new RelayCommand( RemoveEntry, o => SelectedItem != null ) );

        public CountersAgentEntry SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        public bool Warn
        {
            get => _warn;
            set => SetProperty( ref _warn, value );
        }

        public int WarnAmount
        {
            get => _warnAmount;
            set => SetProperty( ref _warnAmount, value );
        }

        public void Serialize( JObject json )
        {
            JObject options = new JObject { { "Warn", Warn }, { "WarnAmount", WarnAmount } };

            JArray items = new JArray();

            foreach ( CountersAgentEntry entry in Items )
            {
                JObject entryObj = new JObject
                {
                    { "Name", entry.Name }, { "Graphic", entry.Graphic }, { "Color", entry.Color }
                };

                items.Add( entryObj );
            }

            options.Add( "Items", items );

            json.Add( "Counters", options );
        }

        public void Deserialize( JObject json, Options options )
        {
            if ( json["Counters"] == null )
            {
                return;
            }

            Warn = json["Counters"]["Warn"]?.ToObject<bool>() ?? false;
            WarnAmount = json["Counters"]["WarnAmount"]?.ToObject<int>() ?? 0;

            foreach ( JToken token in json["Counters"]["Items"] )
            {
                if ( token != null )
                {
                    Items.Add( new CountersAgentEntry
                    {
                        Name = token["Name"].ToObject<string>() ?? "Unknown",
                        Graphic = token["Graphic"].ToObject<int>(),
                        Color = token["Color"].ToObject<int>()
                    } );
                }
            }
        }

        private void RemoveEntry( object obj )
        {
            if ( !( obj is CountersAgentEntry entry ) )
            {
                return;
            }

            Items.Remove( entry );
        }

        private void Recount( object obj )
        {
            foreach ( CountersAgentEntry item in Items )
            {
                int count = item.Count;

                item.Recount();

                if ( Warn && item.Count <= WarnAmount && count > WarnAmount )
                {
                    Commands.SystemMessage( string.Format( Strings.Counter___0___amount_is_now__1____, item.Name,
                        item.Count ) );
                }
            }
        }

        private async Task InsertEntry( object arg )
        {
            int serial = await Commands.GetTargeSerialAsync( Strings.Target_object___ );

            if ( serial == 0 )
            {
                Commands.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            if ( item == null )
            {
                Commands.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            string name = TileData.GetStaticTile( item.ID ).Name;

            if ( string.IsNullOrEmpty( name ) )
            {
                name = item.Name;
            }

            CountersAgentEntry entry =
                new CountersAgentEntry { Name = name, Graphic = item.ID, Color = item.Hue };

            entry.Recount();

            Items.Add( entry );
        }
    }
}