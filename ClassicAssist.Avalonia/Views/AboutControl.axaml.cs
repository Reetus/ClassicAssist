using System;
using System.Media;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

namespace ClassicAssist.Avalonia.Views
{
    public class AboutControl : UserControl
    {
        private readonly TextBlock _control;
        private Timer _timer;

        public AboutControl()
        {
            InitializeComponent();

            _control = this.FindControl<TextBlock>( "CreditText" );

            _control.PointerEnter += OnPointerEnter;
            _control.PointerLeave += OnPointerLeave;
        }

        private void OnPointerLeave( object sender, PointerEventArgs e )
        {
            _timer.Stop();
        }

        private void OnPointerEnter( object sender, PointerEventArgs e )
        {
            _timer = new Timer( 1000 );
            _timer.Elapsed += ( o, args ) =>
            {
                _timer.Stop();

                if ( !_control.IsPointerOver )
                {
                    return;
                }

                using ( SoundPlayer sound = new SoundPlayer( AvaloniaLocator.Current.GetService<IAssetLoader>()
                    .Open( new Uri( "avares://ClassicAssist.Avalonia/Assets/kiss.wav" ) ) ) )
                {
                    sound.Play();
                }
            };

            _timer.Start();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
        }
    }
}