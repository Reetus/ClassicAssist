using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ClassicAssist.Avalonia.Views.Agents
{
    public class VendorSellTabControl : UserControl
    {
        public VendorSellTabControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
        }
    }
}
