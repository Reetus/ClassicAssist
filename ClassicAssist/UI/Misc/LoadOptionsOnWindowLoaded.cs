using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Interactivity;
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
#if !DEBUG
            ExceptionlessClient.Default.Configuration.SetUserIdentity( AssistantOptions.UserId, AssistantOptions.UserId );
            ExceptionlessClient.Default.Configuration.UseSessions( true );
            ExceptionlessClient.Default.Configuration.DefaultData.Add( "Locale", Thread.CurrentThread.CurrentUICulture.Name );
            ExceptionlessClient.Default.Startup( "T8v0i7nL90cVRc4sr2pgo5hviThMPRF3OtQ0bK60" );
#endif
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.Closing -= OnClosing;
            base.OnDetaching();
        }
    }
}