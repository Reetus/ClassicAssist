using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ClassicAssist.Avalonia.Views;
using ClassicAssist.Shared;
using ClassicAssist.UI.ViewModels;

namespace ClassicAssist.Avalonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load( this );
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                //TODO
                Assistant.Engine.MainWindow = (MainWindow)desktop.MainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
