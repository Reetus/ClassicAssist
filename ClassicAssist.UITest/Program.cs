using System;
using System.Globalization;
using System.Threading;
using ClassicAssist.Data;
using ClassicAssist.UI.Views;

namespace ClassicAssist.UITest
{
    internal class Program
    {
        private static MainWindow _window;

        [STAThread]
        private static void Main( string[] args )
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo( "ko-KR" );
            //new MacrosCommandWindow().ShowDialog();
            AssistantOptions.Load();
            _window = new MainWindow();
            _window.ShowDialog();
        }
    }
}