using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Network;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugJournalViewModel : BaseViewModel
    {
        private ICommand _clearCommand;
        private ICommand _copyCommand;
        private bool _enabled;
        private ObservableCollection<string> _items = new ObservableCollection<string>();
        private string _selectedItem;

        public ICommand ClearCommand => _clearCommand ?? ( _clearCommand = new RelayCommand( Clear, o => true ) );

        public ICommand CopyCommand => _copyCommand ?? ( _copyCommand = new RelayCommand( Copy, o => o != null ) );

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if ( value != _enabled )
                {
                    if ( value )
                    {
                        SetEnabled();
                    }
                    else
                    {
                        SetDisabled();
                    }
                }

                SetProperty( ref _enabled, value );
            }
        }

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

        private void SetDisabled()
        {
            IncomingPacketHandlers.JournalEntryAddedEvent -= OnJournalEntryAddedEvent;
        }

        private void SetEnabled()
        {
            JournalEntry[] buffer = Engine.Journal.GetEntireBuffer();

            foreach ( JournalEntry journalEntry in buffer )
            {
                Items.Add( GetString( journalEntry ) );
            }

            IncomingPacketHandlers.JournalEntryAddedEvent += OnJournalEntryAddedEvent;
        }

        private static void Copy( object obj )
        {
            if ( !( obj is string text ) )
            {
                return;
            }

            try
            {
                Clipboard.SetText( text );
            }
            catch ( Exception )
            {
                // ignored
            }
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