using System.Collections.Specialized;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Misc;
using ClassicAssist.UI.Misc;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public abstract class HotkeySettableViewModel<T> : BaseViewModel where T : HotkeySettable
    {
        private readonly HotkeyEntry _category;
        private ObservableCollectionEx<T> _items = new ObservableCollectionEx<T>();

        protected HotkeySettableViewModel( string name )
        {
            _category = new HotkeyEntry { IsCategory = true, Name = name };

            HotkeyManager hotkey = HotkeyManager.GetInstance();

            hotkey.Items.AddSorted( _category );

            Items.CollectionChanged += OnCollectionChanged;
        }

        public ObservableCollectionEx<T> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        protected virtual void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            _category.Children = new ObservableCollectionEx<HotkeySettable>();

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