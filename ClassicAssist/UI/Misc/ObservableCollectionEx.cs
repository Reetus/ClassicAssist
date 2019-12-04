using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ClassicAssist.UI.Misc
{
    public sealed class ObservableCollectionEx<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        // this collection also reacts to changes in its components' properties

        public ObservableCollectionEx()
        {
            CollectionChanged += ObservableCollectionEx_CollectionChanged;
        }

        private void ObservableCollectionEx_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            switch ( e.Action )
            {
                case NotifyCollectionChangedAction.Remove:

                    foreach ( T item in e.OldItems )
                    {
                        //Removed items
                        item.PropertyChanged -= EntityViewModelPropertyChanged;
                    }

                    break;
                case NotifyCollectionChangedAction.Add:

                    foreach ( T item in e.NewItems )
                    {
                        //Added items
                        item.PropertyChanged += EntityViewModelPropertyChanged;
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:

                    break;
                case NotifyCollectionChangedAction.Move:

                    break;
                case NotifyCollectionChangedAction.Reset:

                    break;
                default:

                    throw new ArgumentOutOfRangeException();
            }
        }

        public void EntityViewModelPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            //This will get called when the property of an object inside the collection changes - note you must make it a 'reset' - dunno why
            NotifyCollectionChangedEventArgs args =
                new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
            OnCollectionChanged( args );
        }
    }
}