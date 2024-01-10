using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;

namespace ClassicAssist.UI.Misc
{
    /*
     * https://weblogs.asp.net/akjoshi/Attached-behavior-for-auto-scrolling-containers-while-doing-drag-amp-drop
     */

    public class ScrollOnDragDrop : Behavior<ScrollViewer>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            Subscribe( AssociatedObject );
        }

        protected override void OnDetaching()
        {
            Unsubscribe( AssociatedObject );

            base.OnDetaching();
        }

        private static void Subscribe( FrameworkElement container )
        {
            container.PreviewDragOver += OnContainerPreviewDragOver;
        }

        private static void OnContainerPreviewDragOver( object sender, DragEventArgs e )
        {
            if ( !( sender is FrameworkElement container ) )
            {
                return;
            }

            ScrollViewer scrollViewer = container as ScrollViewer ?? GetFirstVisualChild<ScrollViewer>( container );

            if ( scrollViewer == null )
            {
                return;
            }

            const double tolerance = 60;
            double verticalPos = e.GetPosition( container ).Y;
            const double offset = 20;

            if ( verticalPos < tolerance ) // Top of visible list? 
            {
                scrollViewer.ScrollToVerticalOffset( scrollViewer.VerticalOffset - offset ); //Scroll up. 
            }
            else if ( verticalPos > container.ActualHeight - tolerance ) //Bottom of visible list? 
            {
                scrollViewer.ScrollToVerticalOffset( scrollViewer.VerticalOffset + offset ); //Scroll down.     
            }
        }

        private static void Unsubscribe( FrameworkElement container )
        {
            container.PreviewDragOver -= OnContainerPreviewDragOver;
        }

        public static T GetFirstVisualChild<T>( DependencyObject depObj ) where T : DependencyObject
        {
            if ( depObj == null )
            {
                return null;
            }

            for ( int i = 0; i < VisualTreeHelper.GetChildrenCount( depObj ); i++ )
            {
                DependencyObject child = VisualTreeHelper.GetChild( depObj, i );

                if ( child is T dependencyObject )
                {
                    return dependencyObject;
                }

                T childItem = GetFirstVisualChild<T>( child );

                if ( childItem != null )
                {
                    return childItem;
                }
            }

            return null;
        }
    }
}