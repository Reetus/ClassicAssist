#region License

// Copyright (C) 2020 Reetus
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

using System.Collections.ObjectModel;
using System.Linq;
using ClassicAssist.Data.Vendors;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugVendorBuyViewModel : BaseViewModel
    {
        private bool _isEnabled;
        private ObservableCollection<string> _messages = new ObservableCollection<string>();

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if ( value != _isEnabled )
                {
                    if ( value )
                    {
                        IncomingPacketHandlers.VendorBuyDisplayEvent += OnVendorBuyDisplayEvent;
                    }
                    else
                    {
                        IncomingPacketHandlers.VendorBuyDisplayEvent -= OnVendorBuyDisplayEvent;
                    }
                }

                SetProperty( ref _isEnabled, value );
            }
        }

        public ObservableCollection<string> Messages
        {
            get => _messages;
            set => SetProperty( ref _messages, value );
        }

        private void OnVendorBuyDisplayEvent( int serial, ShopListEntry[] entries )
        {
            _dispatcher.Invoke( () =>
            {
                Messages.Add( $"Vendor: Serial: 0x{serial:x}" );

                foreach ( ShopListEntry entry in entries )
                {
                    bool isCliloc = entry.Name.All( char.IsDigit );

                    Messages.Add(
                        $"\tName: {entry.Name} {( isCliloc ? $"({Cliloc.GetProperty( int.Parse( entry.Name ) )})" : string.Empty )}, Amount: {entry.Amount}, Price: {entry.Price}, ItemID: {entry.Item.ID} (0x{entry.Item.ID:x})" );
                }
            } );
        }
    }
}