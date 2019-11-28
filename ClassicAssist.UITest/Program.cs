using System;
using System.Globalization;
using System.Threading;
using ClassicAssist.UI.Views;
using ClassicAssist.UI.Views.Macros;

namespace ClassicAssist.UITest
{
    internal class Program
    {
        private static MainWindow _window;

        [STAThread]
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-GB");
            //new MacrosCommandWindow().ShowDialog();
            _window = new MainWindow();
            _window.ShowDialog();
        }
    }
}