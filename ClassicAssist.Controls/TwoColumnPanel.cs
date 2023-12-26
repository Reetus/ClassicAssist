using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClassicAssist.Controls
{
    public class TwoColumnGrid : Grid
    {
        private readonly StackPanel leftStackPanel;
        private readonly StackPanel rightStackPanel;
        private List<UIElement> _children = new List<UIElement>();

        public TwoColumnGrid()
        {
            ColumnDefinitions.Add( new ColumnDefinition() );
            ColumnDefinitions.Add( new ColumnDefinition() );

            leftStackPanel = new StackPanel { Margin = new Thickness( 0, 0, 5, 0 ) };
            rightStackPanel = new StackPanel { Margin = new Thickness( 5, 0, 0, 0 ) };

            ScrollViewer leftScrollViewer = new ScrollViewer { Content = leftStackPanel };
            ScrollViewer rightScrollViewer = new ScrollViewer { Content = rightStackPanel };

            SetColumn( leftScrollViewer, 0 );
            SetColumn( rightScrollViewer, 1 );

            Children.Add( leftScrollViewer );
            Children.Add( rightScrollViewer );

            Loaded += ( s, e ) =>
            {
                object obj = FindName( "children" );

                if ( !( obj is StackPanel stackPanel ) )
                {
                    return;
                }

                List<UIElement> children = stackPanel.Children.Cast<UIElement>().ToList();

                if ( children.Count == 0 )
                {
                    return;
                }

                _children = children;

                DivideChildren();

                Children.Remove( stackPanel );
            };

            SizeChanged += ( s, e ) => DivideChildren();
        }

        private void DivideChildren()
        {
            if ( _children.Count == 0 )
            {
                return;
            }

            leftStackPanel.Children.Clear();
            rightStackPanel.Children.Clear();

            UpdateLayout();

            double column1Height = 0;
            double column2Height = 0;
            double availableHeight = ActualHeight;

            foreach ( UIElement child in _children )
            {
                if ( VisualTreeHelper.GetParent( child ) is Panel parent )
                {
                    parent.Children.Remove( child );
                }

                child.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );
                double childHeight = child.DesiredSize.Height;

                // If the child fits in the first column, add it there
                if ( column1Height + childHeight <= availableHeight )
                {
                    leftStackPanel.Children.Add( child );
                    column1Height += childHeight;
                }
                // Otherwise, add it to one with the least height
                else
                {
                    if ( column1Height < column2Height )
                    {
                        leftStackPanel.Children.Add( child );
                        column1Height += childHeight;
                    }
                    else
                    {
                        rightStackPanel.Children.Add( child );
                        column2Height += childHeight;
                    }
                }
            }
        }
    }
}