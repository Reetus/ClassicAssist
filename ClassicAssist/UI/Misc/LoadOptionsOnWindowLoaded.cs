using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Interactivity;
using Assistant;
using ClassicAssist.Data;
using Exceptionless;

namespace ClassicAssist.UI.Misc
{
    public class LoadOptionsOnWindowLoaded : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Closing += OnClosing;
        }

        private static void OnClosing( object sender, CancelEventArgs e )
        {
            Options.Save( Options.CurrentOptions );
            AssistantOptions.Save();
        }

        private static void OnLoaded( object sender, RoutedEventArgs e )
        {
            AssistantOptions.OnWindowLoaded();
            ExceptionlessClient.Default.Configuration.SetUserIdentity( AssistantOptions.UserId );
            ExceptionlessClient.Default.Configuration.UseSessions();
            ExceptionlessClient.Default.SubmittingEvent += ( o, args ) =>
            {
                args.Event.SetProperty( "Locale", Thread.CurrentThread?.CurrentUICulture?.Name );
                args.Event.SetProperty( "PlayerName", Engine.Player?.Name ?? "Unknown" );
                args.Event.SetProperty( "PlayerSerial", Engine.Player?.Serial ?? 0 );
                args.Event.SetProperty( "Shard", Engine.CurrentShard?.Name ?? "Unknown" );
            };
            ExceptionlessClient.Default.Startup( "T8v0i7nL90cVRc4sr2pgo5hviThMPRF3OtQ0bK60" );
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.Closing -= OnClosing;
            base.OnDetaching();
        }
    }
}