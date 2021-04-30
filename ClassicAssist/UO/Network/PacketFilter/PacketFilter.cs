using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassicAssist.UO.Network.PacketFilter
{
    public class PacketFilter
    {
        private List<PacketFilterInfo> _filters = new List<PacketFilterInfo>();

        public void Add( byte packet, PacketFilterCondition[] constraints )
        {
            Add( new PacketFilterInfo( packet, constraints ) );
        }

        public void Add( PacketFilterInfo pfi )
        {
            if ( _filters == null )
            {
                _filters = new List<PacketFilterInfo>();
            }

            _filters.Add( pfi );
        }

        public void Remove( PacketFilterInfo pfi )
        {
            if ( _filters == null )
            {
                _filters = new List<PacketFilterInfo>();
            }

            if ( _filters.Contains( pfi ) )
            {
                _filters.Remove( pfi );
            }
        }

        public bool Remove( byte packet, PacketFilterCondition[] constraints )
        {
            if ( _filters == null )
            {
                _filters = new List<PacketFilterInfo>();
            }

            for ( int i = 0; i < _filters.Count; i++ )
            {
                if ( _filters[i].PacketID != packet )
                {
                    continue;
                }

                if ( constraints == null && _filters[i].GetConditions() == null )
                {
                    _filters.Remove( _filters[i] );
                    return true;
                }

                bool remove = true;

                if ( constraints == null || _filters[i].GetConditions() == null ||
                     _filters[i].GetConditions().Length != constraints.Length )
                {
                    continue;
                }

                for ( int x = 0; x < _filters[i].GetConditions().Length; x++ )
                {
                    if ( !_filters[i].GetConditions()[x].Equals( constraints[x] ) )
                    {
                        remove = false;
                    }
                }

                if ( !remove )
                {
                    continue;
                }

                _filters.Remove( _filters[i] );
                return true;
            }

            return false;
        }

        public bool Contains( PacketFilterInfo pfi )
        {
            return _filters != null && _filters.Contains( pfi );
        }

        public int Count()
        {
            return _filters?.Count ?? 0;
        }

        public void Clear()
        {
            _filters?.Clear();
        }

        public bool MatchFilter( byte[] packet )
        {
            return MatchFilter( packet, out PacketFilterInfo _ );
        }

        public bool MatchFilter( byte[] packet, out PacketFilterInfo pfi )
        {
            pfi = null;

            if ( _filters == null )
            {
                return false;
            }

            bool result = false;

            for ( int i = 0; i < _filters.Count; i++ )
            {
                if ( packet[0] == _filters[i].PacketID )
                {
                    if ( _filters[i].GetConditions() == null )
                    {
                        // No condition so just match packetid
                        result = true;
                        pfi = _filters[i];
                    }
                    else
                    {
                        foreach ( PacketFilterCondition fc in _filters[i].GetConditions() )
                        {
                            if ( fc.Position + fc.Length > packet.Length )
                            {
                                result = false;
                                continue;
                            }

                            byte[] tmp = new byte[fc.Length];
                            Buffer.BlockCopy( packet, fc.Position, tmp, 0, fc.Length );

                            if ( !tmp.SequenceEqual( fc.GetBytes() ) )
                            {
                                result = false;
                                break;
                            }

                            result = true;
                            pfi = _filters?[i] ?? null;
                        }
                    }
                }

                if ( result )
                {
                    break;
                }
            }

            if ( !result )
            {
                pfi = null;
            }

            return result;
        }

        public int MatchFilterAll( byte[] packet, out PacketFilterInfo[] pfis )
        {
            List<PacketFilterInfo> pfiList = new List<PacketFilterInfo>();
            pfis = null;

            if ( _filters == null || packet == null )
            {
                return 0;
            }

            for ( int i = 0; i < _filters.Count; i++ )
            {
                if ( _filters[i] == null || packet[0] != _filters[i].PacketID )
                {
                    continue;
                }

                if ( _filters[i].GetConditions() == null )
                {
                    // No condition so just match packetid
                    pfiList.Add( _filters[i] );
                }
                else
                {
                    foreach ( PacketFilterCondition fc in _filters[i].GetConditions() )
                    {
                        if ( fc.Position + fc.Length > packet.Length )
                        {
                            continue;
                        }

                        byte[] tmp = new byte[fc.Length];
                        Buffer.BlockCopy( packet, fc.Position, tmp, 0, fc.Length );

                        if ( !tmp.SequenceEqual( fc.GetBytes() ) )
                        {
                            break;
                        }

                        pfiList.Add( _filters[i] );
                    }
                }
            }

            pfis = pfiList.ToArray();

            return pfiList.Count;
        }
    }
}