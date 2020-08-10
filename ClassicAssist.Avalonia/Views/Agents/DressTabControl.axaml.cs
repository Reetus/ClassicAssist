using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ClassicAssist.Avalonia.Views.Agents
{
    public class DressTabControl : UserControl
    {
        public DressTabControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
        }
    }
}
