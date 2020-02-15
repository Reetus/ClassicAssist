using System.Windows;
using ClassicAssist.UI.ViewModels;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for GIFRecorderWindow.xaml
    /// </summary>
    public partial class GIFRecorderWindow : Window
    {
        public GIFRecorderWindow()
        {
            InitializeComponent();
            DataContext = new GIFRecorderViewModel( this );
        }
    }
}