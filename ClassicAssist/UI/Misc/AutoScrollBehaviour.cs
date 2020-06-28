using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ClassicAssist.UI.Misc
{
    internal class ListBoxBehavior
    {
        private static readonly Dictionary<ListBox, Capture> Associations = new Dictionary<ListBox, Capture>();

        public static readonly DependencyProperty ScrollOnNewItemProperty =
            DependencyProperty.RegisterAttached( "ScrollOnNewItem", typeof( bool ), typeof( ListBoxBehavior ),
                new UIPropertyMetadata( false, OnScrollOnNewItemChanged ) );

        public static bool GetScrollOnNewItem( DependencyObject obj )
        {
            return (bool) obj.GetValue( ScrollOnNewItemProperty );
        }

        public static void SetScrollOnNewItem( DependencyObject obj, bool value )
        {
            obj.SetValue( ScrollOnNewItemProperty, value );
        }

        public static void OnScrollOnNewItemChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ListBox listBox = d as ListBox;

            if ( listBox == null )
            {
                return;
            }

            bool oldValue = (bool) e.OldValue, newValue = (bool) e.NewValue;

            if ( newValue == oldValue )
            {
                return;
            }

            if ( newValue )
            {
                listBox.Loaded += ListBox_Loaded;
                listBox.Unloaded += ListBox_Unloaded;
                PropertyDescriptor itemsSourcePropertyDescriptor =
                    TypeDescriptor.GetProperties( listBox )["ItemsSource"];
                itemsSourcePropertyDescriptor.AddValueChanged( listBox, ListBox_ItemsSourceChanged );
            }
            else
            {
                listBox.Loaded -= ListBox_Loaded;
                listBox.Unloaded -= ListBox_Unloaded;

                if ( Associations.ContainsKey( listBox ) )
                {
                    Associations[listBox].Dispose();
                }

                PropertyDescriptor itemsSourcePropertyDescriptor =
                    TypeDescriptor.GetProperties( listBox )["ItemsSource"];
                itemsSourcePropertyDescriptor.RemoveValueChanged( listBox, ListBox_ItemsSourceChanged );
            }
        }

        private static void ListBox_ItemsSourceChanged( object sender, EventArgs e )
        {
            ListBox listBox = (ListBox) sender;

            if ( Associations.ContainsKey( listBox ) )
            {
                Associations[listBox].Dispose();
            }

            Associations[listBox] = new Capture( listBox );
        }

        private static void ListBox_Unloaded( object sender, RoutedEventArgs e )
        {
            ListBox listBox = (ListBox) sender;

            if ( Associations.ContainsKey( listBox ) )
            {
                Associations[listBox].Dispose();
            }

            listBox.Unloaded -= ListBox_Unloaded;
        }

        private static void ListBox_Loaded( object sender, RoutedEventArgs e )
        {
            ListBox listBox = (ListBox) sender;

            listBox.Loaded -= ListBox_Loaded;
            Associations[listBox] = new Capture( listBox );
        }

        private class Capture : IDisposable
        {
            private readonly INotifyCollectionChanged incc;
            private readonly ListBox listBox;

            public Capture( ListBox listBox )
            {
                this.listBox = listBox;
                incc = listBox.ItemsSource as INotifyCollectionChanged;

                if ( incc != null )
                {
                    incc.CollectionChanged += incc_CollectionChanged;
                }
            }

            public void Dispose()
            {
                if ( incc != null )
                {
                    incc.CollectionChanged -= incc_CollectionChanged;
                }
            }

            private void incc_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
            {
                if ( e.Action == NotifyCollectionChangedAction.Add )
                {
                    listBox.ScrollIntoView( e.NewItems[0] );
                    listBox.SelectedItem = e.NewItems[0];
                }
            }
        }
    }

    public static class AutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached( "AutoScroll", typeof( bool ), typeof( AutoScrollBehavior ),
                new PropertyMetadata( false, AutoScrollPropertyChanged ) );

        public static void AutoScrollPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            ScrollViewer scrollViewer = obj as ScrollViewer;

            if ( scrollViewer != null && (bool) args.NewValue )
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                scrollViewer.ScrollToEnd();
            }
            else
            {
                if ( scrollViewer != null )
                {
                    scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                }
            }
        }

        private static void ScrollViewer_ScrollChanged( object sender, ScrollChangedEventArgs e )
        {
            // Only scroll to bottom when the extent changed. Otherwise you can't scroll up
            if ( e.ExtentHeightChange != 0 )
            {
                ScrollViewer scrollViewer = sender as ScrollViewer;
                scrollViewer?.ScrollToBottom();
            }
        }

        public static bool GetAutoScroll( DependencyObject obj )
        {
            return (bool) obj.GetValue( AutoScrollProperty );
        }

        public static void SetAutoScroll( DependencyObject obj, bool value )
        {
            obj.SetValue( AutoScrollProperty, value );
        }
    }
}