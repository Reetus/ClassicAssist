using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ClassicAssist.Avalonia.Views
{
    public class AgentsTabControl : UserControl
    {
        public AgentsTabControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
        }
    }
}
