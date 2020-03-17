using System;
using System.Threading;
using System.Windows;
using Exceptionless;

namespace ClassicAssist.Launcher
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup( StartupEventArgs e )
        {
            ExceptionlessClient.Default.Configuration.DefaultData.Add( "Locale", Thread.CurrentThread.CurrentUICulture.Name );
            ExceptionlessClient.Default.Startup( "T8v0i7nL90cVRc4sr2pgo5hviThMPRF3OtQ0bK60" );
            base.OnStartup( e );
        }
    }
}