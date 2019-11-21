using Avalonia;
using Avalonia.Markup.Xaml;

namespace ClassicAssist.Tester
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
