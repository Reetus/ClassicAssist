using System;
using System.Diagnostics;
using System.Windows;
using CommandLine;
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