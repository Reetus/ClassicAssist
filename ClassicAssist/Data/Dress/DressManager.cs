using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.Data.Dress
{
    public class DressManager : INotifyPropertyChanged
    {
        private static DressManager _instance;
        private static readonly object _instanceLock = new object();
        private ObservableCollectionEx<DressAgentEntry> _items = new ObservableCollectionEx<DressAgentEntry>();

        private DressManager()
        {
            Items.CollectionChanged += OnCollectionChanged;
        }

        public ObservableCollectionEx<DressAgentEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            OnPropertyChanged( nameof( Items ) );
        }

        public static DressManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new DressManager();
                    }
                }
            }

            return _instance;
        }

        public void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}