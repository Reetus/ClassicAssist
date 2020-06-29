using System.Collections.ObjectModel;
using ClassicAssist.Data.SpecialMoves;
using Microsoft.Scripting.Utils;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugSpecialMovesViewModel : BaseViewModel
    {
        private readonly SpecialMovesManager _manager;
        private ObservableCollection<string> _items = new ObservableCollection<string>();
        private ObservableCollection<string> _messages = new ObservableCollection<string>();
        private string _selectedItem;

        public DebugSpecialMovesViewModel()
        {
            _manager = SpecialMovesManager.GetInstance();

            Items.AddRange( _manager.GetEnabledNames() );

            _manager.SpecialMovesChanged += OnSpecialMovesChanged;
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

        private void OnSpecialMovesChanged( string name, bool enabled )
        {
            _dispatcher.Invoke( () =>
            {
                Messages.Add( enabled ? $"Enabled: {name}" : $"Disabled: {name}" );

                Items.Clear();
                Items.AddRange( _manager.GetEnabledNames() );
            } );
        }
    }
}