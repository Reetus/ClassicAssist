﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ClassicAssist.UO.Network.PacketFilter
{
    public class PacketWaitEntries
    {
        public delegate void dWaitEntryAddedRemoved( PacketWaitEntry entry );

        private readonly List<PacketWaitEntry> _waitEntries = new List<PacketWaitEntry>();
        private readonly object _waitEntryLock = new object();

        public event dWaitEntryAddedRemoved WaitEntryAddedEvent;
        public event dWaitEntryAddedRemoved WaitEntryRemovedEvent;

        public PacketWaitEntry Add( PacketFilterInfo pfi, PacketDirection direction, bool autoRemove = false,
            bool filteredOnly = false, bool includeInternal = false )
        {
            PacketWaitEntry we = new PacketWaitEntry
            {
                PFI = pfi,
                Lock = new AutoResetEvent( false ),
                PacketDirection = direction,
                AutoRemove = autoRemove,
                MatchInternal = includeInternal
            };

            lock ( _waitEntryLock )
            {
                _waitEntries.Add( we );
                WaitEntryAddedEvent?.Invoke( we );
            }

            return we;
        }

        public bool CheckWait( byte[] packet, PacketDirection direction, bool filteredPacket = false )
        {
            lock ( _waitEntryLock )
            {
                if ( _waitEntries.Count == 0 )
                {
                    return false;
                }
            }

            List<PacketWaitEntry> matchedEntries = new List<PacketWaitEntry>();

            lock ( _waitEntryLock )
            {
                foreach ( PacketWaitEntry t in _waitEntries.Where( t => packet[0] == t.PFI.PacketID ).Where( t =>
                             direction == t.PacketDirection && ( t.MatchInternal || !filteredPacket)))
                {
                    if ( t.PFI.GetConditions() == null )
                    {
                        // No condition so just match packetid
                        matchedEntries.Add( t );
                    }
                    else
                    {
                        bool result = false;

                        foreach ( PacketFilterCondition fc in t.PFI.GetConditions() )
                        {
                            if ( fc.Position + fc.Length > packet.Length )
                            {
                                result = false;

                                break;
                            }

                            byte[] tmp = new byte[fc.Length];
                            Buffer.BlockCopy( packet, fc.Position, tmp, 0, fc.Length );

                            bool equals = tmp.SequenceEqual( fc.GetBytes() );

                            if ( fc.Negate && equals || !fc.Negate && !equals )
                            {
                                result = false;

                                break;
                            }

                            result = true;
                        }

                        if ( result )
                        {
                            matchedEntries.Add( t );
                        }
                    }
                }
            }

            if ( matchedEntries.Count == 0 )
            {
                return false;
            }

            List<PacketWaitEntry> removeList = new List<PacketWaitEntry>();

            foreach ( PacketWaitEntry entry in matchedEntries )
            {
                entry.Packet = new byte[packet.Length];
                Buffer.BlockCopy( packet, 0, entry.Packet, 0, packet.Length );
                entry.Lock.Set();
                entry.PFI.Action?.Invoke( packet, entry.PFI );

                if ( entry.AutoRemove )
                {
                    removeList.Add( entry );
                }
            }

            removeList.ForEach( Remove );

            return true;
        }

        public void Clear()
        {
            lock ( _waitEntryLock )
            {
                foreach ( PacketWaitEntry we in _waitEntries )
                {
                    we.Lock.Set();
                }

                _waitEntries?.Clear();
            }
        }

        public PacketWaitEntry[] GetEntries()
        {
            lock ( _waitEntryLock )
            {
                return _waitEntries?.ToArray();
            }
        }

        public void Remove( PacketWaitEntry we )
        {
            lock ( _waitEntryLock )
            {
                _waitEntries.Remove( we );
                WaitEntryRemovedEvent?.Invoke( we );
            }
        }

        public void RemoveRange( IEnumerable<PacketWaitEntry> wes )
        {
            lock ( _waitEntryLock )
            {
                foreach ( PacketWaitEntry we in wes )
                {
                    _waitEntries.Remove( we );
                    WaitEntryRemovedEvent?.Invoke( we );
                }
            }
        }
    }
}