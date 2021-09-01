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

using System;
using System.Threading;

namespace ClassicAssist.UO.Network
{
    public abstract class BaseQueueItem
    {
        protected BaseQueueItem()
        {
            WaitHandle = new AutoResetEvent( false );
        }

        public bool DelaySend { get; set; }
        public bool Result { get; set; }
        public CancellationToken Token { get; set; } = CancellationToken.None;
        public EventWaitHandle WaitHandle { get; set; }
    }

    public class PacketQueueItem : BaseQueueItem
    {
        public PacketQueueItem( byte[] packet, int length, bool delaySend, string caller )
        {
            Packet = packet;
            Length = length;
            DelaySend = delaySend;
            Caller = caller;
        }

        public string Caller { get; set; }

        public int Length { get; set; }
        public byte[] Packet { get; set; }
    }

    public class ActionQueueItem : BaseQueueItem
    {
        public ActionQueueItem( Func<object, bool> action )
        {
            Action = action;
        }

        public Func<object, bool> Action { get; set; }
        public object Arguments { get; set; }
        public string Caller { get; set; }
        public bool CheckRange { get; set; }
        public int Serial { get; set; }
    }
}