using System;
using System.Windows;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for EntityCollectionViewer.xaml
    /// </summary>
    public partial class EntityCollectionViewer : Window
    {
        public EntityCollectionViewer()
        {
            InitializeComponent();

            Closed += OnClosed;
        }

        private void OnClosed( object sender, EventArgs e )
        {
            Closed -= OnClosed;

            ( DataContext as IDisposable )?.Dispose();
        }
    }
}