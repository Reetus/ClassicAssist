using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;
using ClassicAssist.Data.Filters;

namespace ClassicAssist.UI.Views.Filters
{
    /// <summary>
    ///     Interaction logic for LightLevelFilterConfigureWindow.xaml
    /// </summary>
    public partial class LightLevelFilterConfigureWindow : INotifyPropertyChanged
    {
        private LightLevelOptions _options = new LightLevelOptions();

        public LightLevelFilterConfigureWindow()
        {
            InitializeComponent();
        }

        public LightLevelFilterConfigureWindow( LightLevelOptions options )
        {
            InitializeComponent();
            Options = options;
        }

        public LightLevelOptions Options
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
