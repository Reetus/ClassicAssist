using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ClassicAssist.Avalonia.Views.Agents
{
    public class OrganizerTabControl : UserControl
    {
        public OrganizerTabControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
        }
    }
}
