using System.Media;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for AboutControl.xaml
    /// </summary>
    public partial class AboutControl : UserControl
    {
        private Timer _timer;

        public AboutControl()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseEnter( object sender, MouseEventArgs e )
        {
            if ( !( sender is TextBlock textBlock ) )
            {
                return;
            }

            _timer = new Timer( 1000 );
            _timer.Elapsed += ( o, args ) =>
            {
                _timer.Stop();

                if ( !textBlock.IsMouseOver )
                {
                    return;
                }

                using ( SoundPlayer sound = new SoundPlayer( Properties.Resources.kiss ) )
                {
                    sound.Play();
                }
            };

            _timer.Start();
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            _timer?.Stop();
        }
    }
}