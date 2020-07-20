using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using ClassicAssist.Shared;
using ClassicAssist.Data;
using ClassicAssist.UO.Network;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugJournalViewModel : BaseViewModel
    {
        private ICommand _clearCommand;
        private ObservableCollection<string> _items = new ObservableCollection<string>();
        private string _selectedItem;

        public DebugJournalViewModel()
        {
            JournalEntry[] buffer = Engine.Journal.GetBuffer();

            foreach ( JournalEntry journalEntry in buffer )
            {
                Items.Add( GetString( journalEntry ) );
            }

            IncomingPacketHandlers.JournalEntryAddedEvent += OnJournalEntryAddedEvent;
        }

        public ICommand ClearCommand => _clearCommand ?? ( _clearCommand = new RelayCommand( Clear, o => true ) );

        public ObservableCollection<string> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public string SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        private void Clear( object obj )
        {
            Items.Clear();
        }

        private static string GetString( JournalEntry journalEntry )
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine( $"Name: {journalEntry.Name}" );
            sb.AppendLine( $"Serial: 0x{journalEntry.Serial:x8}" );
            sb.AppendLine( $"ID: 0x{journalEntry.ID:x4}" );
            sb.AppendLine( $"Cliloc: {journalEntry.Cliloc}" );
            sb.AppendLine( $"Text: {journalEntry.Text}" );
            sb.AppendLine( $"Arguments: {string.Join( ",", journalEntry.Arguments ?? new string[0] )}" );
            sb.AppendLine( $"Language: {journalEntry.SpeechLanguage}" );
            sb.AppendLine( $"Type: {journalEntry.SpeechType}" );

            return sb.ToString();
        }

        private void OnJournalEntryAddedEvent( JournalEntry je )
        {
            _dispatcher.Invoke( () => { Items.Add( GetString( je ) ); } );
        }
    }
}