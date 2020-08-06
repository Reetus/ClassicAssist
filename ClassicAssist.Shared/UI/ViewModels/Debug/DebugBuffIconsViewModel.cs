using System.Collections.ObjectModel;
using ClassicAssist.Data.BuffIcons;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO.Network;
using Microsoft.Scripting.Utils;

namespace ClassicAssist.Shared.UI.ViewModels.Debug
{
    public class DebugBuffIconsViewModel : BaseViewModel
    {
        private readonly BuffIconManager _manager;
        private ObservableCollection<string> _items = new ObservableCollection<string>();
        private ObservableCollection<string> _messages = new ObservableCollection<string>();
        private string _selectedItem;

        public DebugBuffIconsViewModel()
        {
            _manager = BuffIconManager.GetInstance();

            Items.AddRange( _manager.GetEnabledNames() );

            IncomingPacketHandlers.BufficonEnabledDisabledEvent += OnBufficonEnabledDisabledEvent;
        }

        public ObservableCollection<string> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ObservableCollection<string> Messages
        {
            get => _messages;
            set => SetProperty( ref _messages, value );
        }

        public string SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        private void OnBufficonEnabledDisabledEvent( int type, bool enabled, int duration )
        {
            BuffIconData data = _manager.GetDataByID( type );

            _dispatcher.Invoke( () =>
            {
                Messages.Add( enabled ? $"Enabled: {data.Name}" : $"Disabled: {data?.Name}" );

                Items.Clear();
                Items.AddRange( _manager.GetEnabledNames() );
            } );
        }
    }
}