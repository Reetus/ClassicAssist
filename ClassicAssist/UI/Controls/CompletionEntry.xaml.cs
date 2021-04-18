using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using ClassicAssist.Annotations;
using ClassicAssist.UI.ViewModels;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     Interaction logic for CompletionEntry.xaml
    /// </summary>
    public partial class CompletionEntry : INotifyPropertyChanged
    {
        private ICommand _copyToClipboardCommand;
        private string _entryExample;
        private string _entryName;
        private bool _isButtonEnabled;
        private bool _isExpanded;
        private ICommand _toggleExpandedCommand;

        public CompletionEntry( string entryName, string entryExample )
        {
            InitializeComponent();
            EntryName = entryName;
            EntryExample = entryExample;
            IsButtonEnabled = !string.IsNullOrEmpty( EntryExample );

            CodeTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader( Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ),
                    "Python.Dark.xshd" ) ), HighlightingManager.Instance );
        }

        public ICommand CopyToClipboardCommand =>
            _copyToClipboardCommand ?? ( _copyToClipboardCommand = new RelayCommand( CopyToClipboard, o => true ) );

        public string EntryExample
        {
            get => _entryExample;
            set
            {
                _entryExample = value;
                OnPropertyChanged();
            }
        }

        public string EntryName
        {
            get => _entryName;
            set
            {
                _entryName = value;
                OnPropertyChanged();
            }
        }

        public bool IsButtonEnabled
        {
            get => _isButtonEnabled;
            set
            {
                _isButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public ICommand ToggleExpandedCommand =>
            _toggleExpandedCommand ?? ( _toggleExpandedCommand = new RelayCommand( ToggleExpanded, o => true ) );

        public event PropertyChangedEventHandler PropertyChanged;

        private void ToggleExpanded( object obj )
        {
            IsExpanded = !IsExpanded;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        private static void CopyToClipboard( object obj )
        {
            if ( !( obj is string macro ) )
            {
                return;
            }

            try
            {
                Clipboard.SetText( macro );
            }
            catch ( Exception )
            {
                // ignored
            }
        }
    }
}