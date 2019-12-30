using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;
using ClassicAssist.Data;

namespace ClassicAssist.UI.Misc
{
    public class LoadOptionsOnWindowLoaded : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Closing += OnClosing;
        }

        private static void OnClosing( object sender, CancelEventArgs e )
        {
            Options.Save( Options.CurrentOptions );
        }

        private static void OnLoaded( object sender, RoutedEventArgs e )
        {
            Options.Load( Options.DEFAULT_SETTINGS_FILENAME, Options.CurrentOptions );
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.Closing -= OnClosing;
            base.OnDetaching();
        }
    }
}