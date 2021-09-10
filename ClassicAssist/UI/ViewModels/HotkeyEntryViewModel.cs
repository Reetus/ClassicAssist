using System.Collections.Specialized;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Misc;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public abstract class HotkeyEntryViewModel<T> : BaseViewModel where T : HotkeyEntry
    {
        private readonly HotkeyCommand _category;
        private ObservableCollectionEx<T> _items = new ObservableCollectionEx<T>();

        protected HotkeyEntryViewModel( string name )
        {
            _category = new HotkeyCommand { Name = name, IsCategory = true };

            HotkeyManager hotkey = HotkeyManager.GetInstance();

            hotkey.AddCategory( _category );

            Items.CollectionChanged += OnCollectionChanged;

            _category.Children = new ObservableCollectionEx<HotkeyEntry>();
        }

        public ObservableCollectionEx<T> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        ~HotkeyEntryViewModel()
        {
            Items.CollectionChanged -= OnCollectionChanged;
        }

        protected virtual void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            _category.Children = new ObservableCollectionEx<HotkeyEntry>();

            foreach ( T item in Items )
            {
                _category.Children.Add( item );
            }
        }

        protected void SetJsonValue( JToken json, string name, JToken value )
        {
            json[name] = value;
        }

        protected T2 GetJsonValue<T2>( JToken json, string name, T2 defaultValue )
        {
            if ( json == null )
            {
                return defaultValue;
            }

            return json[name] == null ? defaultValue : json[name].ToObject<T2>();
        }
    }
}