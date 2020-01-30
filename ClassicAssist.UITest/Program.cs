using System;
using System.Globalization;
using System.Threading;
using ClassicAssist.Data;
using ClassicAssist.Data.Filters;
using ClassicAssist.UI.Views;
using ClassicAssist.UI.Views.Filters;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UITest
{
    internal class Program
    {
        private static MainWindow _window;

        [STAThread]
        private static void Main( string[] args )
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo( "en-GB" );
            //new MacrosCommandWindow().ShowDialog();
            AssistantOptions.Load();
            _window = new MainWindow();
            _window.ShowDialog();
        }
    }
}