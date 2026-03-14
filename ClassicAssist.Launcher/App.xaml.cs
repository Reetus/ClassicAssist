#region License

// Copyright (C) 2026 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Threading;
using System.Windows;
using CommandLine;
using Exceptionless;

namespace ClassicAssist.Launcher
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static CommandLineOptions CurrentOptions { get; set; } = new CommandLineOptions();

        protected override void OnStartup( StartupEventArgs e )
        {
            ExceptionlessClient.Default.Configuration.DefaultData.Add( "Locale", Thread.CurrentThread.CurrentUICulture.Name );
            ExceptionlessClient.Default.Startup( "T8v0i7nL90cVRc4sr2pgo5hviThMPRF3OtQ0bK60" );

            Parser.Default.ParseArguments<CommandLineOptions>( e.Args ).WithParsed( o => CurrentOptions = o );

            base.OnStartup( e );
        }
    }
}