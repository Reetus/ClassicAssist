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

using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ClassicAssist.Launcher
{
    public static class Utility
    {
        public static async Task<IPAddress> ResolveAddress( string hostname )
        {
            if ( IPAddress.TryParse( hostname, out IPAddress address ) )
            {
                return address;
            }

            IPHostEntry hostentry = await Dns.GetHostEntryAsync( hostname );

            return hostentry?.AddressList.FirstOrDefault( i => i.AddressFamily == AddressFamily.InterNetwork );
        }
    }
}