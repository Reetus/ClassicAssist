using System.Windows.Controls;
using System.Windows.Input;
using ClassicAssist.UI.ViewModels.Debug;

namespace ClassicAssist.UI.Views.Debug
{
    /// <summary>
    ///     Interaction logic for DebugKeyboardControl.xaml
    /// </summary>
    public partial class DebugKeyboardControl : UserControl
    {
        public delegate void dKeyDown( KeyEventArgs e );

        public DebugKeyboardControl()
        {
            DataContext = new DebugKeyboardViewModel( this );
            InitializeComponent();
        }

        public event dKeyDown WPFKeyDownEvent;

        protected override void OnKeyDown( KeyEventArgs e )
        {
            WPFKeyDownEvent?.Invoke( e );
            base.OnKeyDown( e );
        }
    }
}