#region License

// Copyright (C) 2021 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugPacketQueueViewModel : BaseViewModel
    {
        private readonly DispatcherTimer _timer;
        private ICommand _clearCommand;
        private int _incomingQueueLength;
        private ObservableCollection<string> _items = new ObservableCollection<string>();
        private int _outgoingQueueLength;
        private int _threshold;

        public DebugPacketQueueViewModel()
        {
            _timer = new DispatcherTimer( TimeSpan.FromMilliseconds( 50 ), DispatcherPriority.Normal, OnTick,
                Dispatcher.CurrentDispatcher );
            _timer.Start();

            Threshold = Options.CurrentOptions.SlowHandlerThreshold;
            Engine.SlowHandlerEvent += OnSlowHandlerEvent;
        }

        public ICommand ClearCommand => _clearCommand ?? ( _clearCommand = new RelayCommand( Clear, o => true ) );

        public int IncomingQueueLength
        {
            get => _incomingQueueLength;
            set => SetProperty( ref _incomingQueueLength, value );
        }

        public ObservableCollection<string> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public int OutgoingQueueLength
        {
            get => _outgoingQueueLength;
            set => SetProperty( ref _outgoingQueueLength, value );
        }

        public int Threshold
        {
            get => _threshold;
            set
            {
                SetProperty( ref _threshold, value );
                Options.CurrentOptions.SlowHandlerThreshold = value;
            }
        }

        private void Clear( object obj )
        {
            _dispatcher.Invoke( () => Items.Clear() );
        }

        ~DebugPacketQueueViewModel()
        {
            Engine.SlowHandlerEvent -= OnSlowHandlerEvent;
            _timer.Stop();
        }

        private void OnSlowHandlerEvent( PacketDirection direction, string handlername, TimeSpan elapsed )
        {
            _dispatcher.Invoke( () => Items.Add( $"{direction} {handlername} {elapsed}" ) );
        }

        private void OnTick( object sender, EventArgs e )
        {
            IncomingQueueLength = Engine.IncomingQueue.Count;
            OutgoingQueueLength = Engine.OutgoingQueue.Count;
        }
    }
}