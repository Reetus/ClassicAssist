using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;
using Assistant;
using ClassicAssist.Data;

namespace ClassicAssist.Misc
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
            Options.Save( Engine.StartupPath ?? Environment.CurrentDirectory );
        }

        private static void OnLoaded( object sender, RoutedEventArgs e )
        {
            Options.Load( Engine.StartupPath ?? Environment.CurrentDirectory, Options.CurrentOptions );
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.Closing -= OnClosing;
            base.OnDetaching();
        }
    }
}