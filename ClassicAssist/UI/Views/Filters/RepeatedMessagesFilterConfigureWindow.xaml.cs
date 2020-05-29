using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ClassicAssist.Annotations;
using ClassicAssist.Data.Filters;

namespace ClassicAssist.UI.Views.Filters
{
    /// <summary>
    ///     Interaction logic for RepeatedMessagesFilterConfigureWindow.xaml
    /// </summary>
    public partial class RepeatedMessagesFilterConfigureWindow : INotifyPropertyChanged
    {
        private RepeatedMessagesFilter.MessageFilterOptions
            _options = new RepeatedMessagesFilter.MessageFilterOptions();

        public RepeatedMessagesFilterConfigureWindow()
        {
            InitializeComponent();
        }

        public RepeatedMessagesFilterConfigureWindow( RepeatedMessagesFilter.MessageFilterOptions options )
        {
            InitializeComponent();
            Options = options;
        }

        public RepeatedMessagesFilter.MessageFilterOptions Options
        {
            get => _options;
            set
            {
                _options = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}