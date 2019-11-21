using System;
using System.Collections.Generic;
using Assistant;
using ClassicAssist.UO.Objects;
using ClassicAssist.ViewModels;
using ReactiveUI;

namespace ClassicAssist.UI.ViewModels
{
    public class AboutTabViewModel : ViewModelBase
    {
        private DateTime _connectedTime;
        private TimeSpan _connected;
        private int _itemCount;
        private int _mobileCount;

        public AboutTabViewModel()
        {
            Engine.ConnectedEvent += EngineOnConnectedEvent;
            Engine.Items.ContainerContentsChanged += ItemsOnContainerContentsChanged;
            Engine.Mobiles.CollectionChanged += MobilesOnCollectionChanged;
        }

        private void MobilesOnCollectionChanged( int totalcount )
        {
            MobileCount = totalcount;
        }

        private void ItemsOnContainerContentsChanged( bool added, IEnumerable<Item> items )
        {
            ItemCount = Engine.Items.GetTotalItemCount();
        }

        public int ItemCount
        {
            get => _itemCount;
            set => this.RaiseAndSetIfChanged(ref _itemCount, value);
        }

        public int MobileCount
        {
            get => _mobileCount;
            set => this.RaiseAndSetIfChanged(ref _mobileCount, value);
        }

        private void EngineOnConnectedEvent()
        {
            _connectedTime = DateTime.Now;
            Connected = DateTime.Now - _connectedTime;
        }

        public TimeSpan Connected
        {
            get => _connected;
            set => this.RaiseAndSetIfChanged(ref _connected, value);
        }
    }
}