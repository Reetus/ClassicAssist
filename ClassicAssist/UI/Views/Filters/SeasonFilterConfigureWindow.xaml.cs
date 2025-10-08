using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ClassicAssist.Annotations;
using ClassicAssist.Data.Filters;

namespace ClassicAssist.UI.Views.Filters
{
    /// <summary>
    ///     Interaction logic for SeasonFilterConfigureWindow.xaml
    /// </summary>
    public partial class SeasonFilterConfigureWindow : Window, INotifyPropertyChanged
    {
        private Season _selectedSeason;

        public SeasonFilterConfigureWindow()
        {
            InitializeComponent();
        }

        public SeasonFilterConfigureWindow( Season selectedSeason )
        {
            InitializeComponent();
            SelectedSeason = selectedSeason;
        }

        public Season SelectedSeason
        {
            get => _selectedSeason;
            set
            {
                _selectedSeason = value;
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