using System;
using System.Diagnostics;
using System.Windows;
using CommandLine;
using Exceptionless;
using IOPath = System.IO.Path;

namespace ClassicAssist.Updater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Options CurrentOptions { get; set; }

        private void Application_Startup( object sender, StartupEventArgs e )
        {
            ExceptionlessClient.Default.Startup( "T8v0i7nL90cVRc4sr2pgo5hviThMPRF3OtQ0bK60" );

            Parser.Default.ParseArguments<Options>( e.Args ).WithParsed( o => CurrentOptions = o );

            if ( string.IsNullOrEmpty( CurrentOptions.Path ) )
            {
                CurrentOptions.Path = Environment.CurrentDirectory;
            }

            if ( CurrentOptions.CurrentVersion == null )
            {
                if ( Version.TryParse(
                    FileVersionInfo.GetVersionInfo( IOPath.Combine( CurrentOptions.Path, "ClassicAssist.dll" ) )
                        .ProductVersion,
                    out Version version ) )
                {
                    CurrentOptions.CurrentVersion = version;
                }
            }
        }
    }
}