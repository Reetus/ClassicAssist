using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Shared.UI;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public abstract class HotkeyEntryViewModel<T> : BaseViewModel where T : HotkeyEntry
    {
        private readonly HotkeyCommand _category;
        private ObservableCollectionEx<T> _items = new ObservableCollectionEx<T>();
        protected List<HotkeyCommand> _staticOptions = new List<HotkeyCommand>();

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

            if ( _staticOptions.Any() )
            {
                foreach ( HotkeyCommand hotkey in _staticOptions )
                {
                    _category.Children.Add( hotkey );
                }
            }

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

        protected void SerializeStatic( JObject organizer )
        {
            JObject staticHotkeys = new JObject();

            foreach ( HotkeyCommand option in _staticOptions )
            {
                JObject obj = new JObject
                {
                    { "Keys", option.Hotkey.ToJObject() },
                    { "PassToUO", option.PassToUO },
                    { "Disableable", option.Disableable }
                };

                staticHotkeys.Add( option.Name, obj );
            }

            organizer.Add( "Static", staticHotkeys );
        }

        protected void DeserializeStatic( JObject obj )
        {
            if ( !( obj?["Static"] is JObject ) )
            {
                return;
            }

            foreach ( HotkeyCommand option in _staticOptions )
            {
                if ( !( obj["Static"][option.Name] is JObject json ) )
                {
                    continue;
                }

                option.Hotkey = new ShortcutKeys( json["Keys"] );
                option.PassToUO = GetJsonValue( json, "PassToUO", option.PassToUO );
                option.Disableable = GetJsonValue( json, "Disableable", option.Disableable );
            }
        }
    }
}