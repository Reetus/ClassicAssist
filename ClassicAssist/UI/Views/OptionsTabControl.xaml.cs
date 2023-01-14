using System.Windows.Controls;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for OptionsTabControl.xaml
    /// </summary>
    public partial class OptionsTabControl : UserControl
    {
        public OptionsTabControl()
        {
            InitializeComponent();
        }

        private void TextBox_MacrosGumpHeight_TextChanged( object sender, TextChangedEventArgs e )
        {
            //UO.Gumps.MacrosGump.Initialize();
            //UO.Gumps.MacrosGump.ResendGump( true );
            //UO.Gumps.MacrosGump.SetPosition(100, 100);
        }

        private void TextBox_MacrosGumpWidth_TextChanged( object sender, TextChangedEventArgs e )
        {
            //UO.Gumps.MacrosGump.Initialize();
            //UO.Gumps.MacrosGump.ResendGump( true );
        }
    }
}