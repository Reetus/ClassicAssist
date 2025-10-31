using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using System;
using System.Threading;

namespace ClassicAssist.UO.Network.Packets
{
    public class DragItem : BasePacket
    {
        private static readonly object _dragPacketLock = new object();
        private static DateTime _lastDragPacketTime = DateTime.MinValue;
        private readonly int _dragDelayMs;
        public DragItem( int serial, int amount, int dragDelayMs, bool checkAmount = false )
        {
            _dragDelayMs = dragDelayMs;

            if ( checkAmount && amount == -1 )
            {
                Item item = Engine.Items.GetItem( serial );

                if ( item != null )
                {
                    amount = item.Count;
                }
            }

            _writer = new PacketWriter( 7 );
            _writer.Write( (byte) 0x07 );
            _writer.Write( serial );
            _writer.Write( (short) amount );
        }

        /// <summary>
        /// Sphere-X has built-in throtteling on drag packets, so we need to be able to specifically wait between those packets.
        /// </summary>
        public override void ThrottleBeforeSend()
        {
            if (_dragDelayMs <= 0)
            {
                return;
            }

            lock ( _dragPacketLock )
            {
                DateTime nextAllowedDragTime = _lastDragPacketTime + TimeSpan.FromMilliseconds( _dragDelayMs );
                DateTime now = DateTime.Now;

                if ( nextAllowedDragTime > now )
                {
                    Thread.Sleep( nextAllowedDragTime - now );
                }

                _lastDragPacketTime = DateTime.Now;
            }
        }
    }
}