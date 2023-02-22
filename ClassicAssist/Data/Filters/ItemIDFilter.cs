#region License

// Copyright (C) 2023 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Collections.ObjectModel;
using System.Linq;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels.Filters;
using ClassicAssist.UI.Views.Filters;
using ClassicAssist.UO.Network.PacketFilter;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Filters
{
    [FilterOptions( Name = "ItemID Filter", DefaultEnabled = false )]
    public class ItemIDFilter : DynamicFilterEntry, IConfigurableFilter
    {
        public ObservableCollection<ItemIDFilterEntry> Items { get; set; } =
            new ObservableCollection<ItemIDFilterEntry>();

        public void Configure()
        {
            ItemIDFilterConfigureWindowViewModel vm = new ItemIDFilterConfigureWindowViewModel( Items );
            ItemIDFilterConfigureWindow window = new ItemIDFilterConfigureWindow { DataContext = vm };
            window.ShowDialog();
        }

        public void Deserialize( JToken token )
        {
            if ( token?["Items"] == null )
            {
                return;
            }

            foreach ( JToken itemsToken in token["Items"] )
            {
                ItemIDFilterEntry entry = new ItemIDFilterEntry
                {
                    Enabled = itemsToken["Enabled"]?.ToObject<bool>() ?? false,
                    SourceID = itemsToken["SourceID"]?.ToObject<int>() ?? 0,
                    DestinationID = itemsToken["DestinationID"]?.ToObject<int>() ?? 0,
                    Hue = itemsToken["Hue"]?.ToObject<int>() ?? -1
                };

                Items.Add( entry );
            }
        }

        public JObject Serialize()
        {
            JObject config = new JObject();

            JArray items = new JArray();

            foreach ( ItemIDFilterEntry item in Items )
            {
                items.Add( new JObject
                {
                    { "Enabled", item.Enabled },
                    { "SourceID", item.SourceID },
                    { "DestinationID", item.DestinationID },
                    { "Hue", item.Hue }
                } );
            }

            config.Add( "Items", items );

            return config;
        }

        public void ResetOptions()
        {
            Items.Clear();
        }

        public override bool CheckPacket( ref byte[] packet, ref int length, PacketDirection direction )
        {
            if ( packet == null || !Enabled )
            {
                return false;
            }

            if ( direction != PacketDirection.Incoming )
            {
                return false;
            }

            switch ( packet[0] )
            {
                case 0xF3:
                {
                    int itemId = ( packet[8] << 8 ) | packet[9];

                    ItemIDFilterEntry entry = Items.FirstOrDefault( e => e.SourceID == itemId && e.Enabled );

                    if ( entry == null )
                    {
                        return false;
                    }

                    packet[8] = (byte) ( entry.DestinationID >> 8 );
                    packet[9] = (byte) entry.DestinationID;

                    if ( entry.Hue == -1 )
                    {
                        return false;
                    }

                    packet[21] = (byte) ( entry.Hue >> 8 );
                    packet[22] = (byte) entry.Hue;
                    break;
                }
                case 0x3C:
                {
                    bool oldStyle = false;

                    int count = ( packet[3] << 8 ) | packet[4];

                    if ( packet.Length / 20 != count )
                    {
                        oldStyle = true;
                    }

                    for ( int i = 0; i < count; i++ )
                    {
                        int offset = 5 + i * ( oldStyle ? 19 : 20 );

                        int itemId = ( packet[offset + 4] << 8 ) | packet[offset + 5];

                        ItemIDFilterEntry entry = Items.FirstOrDefault( e => e.SourceID == itemId && e.Enabled );

                        if ( entry == null )
                        {
                            continue;
                        }

                        packet[offset + 4] = (byte) ( entry.DestinationID >> 8 );
                        packet[offset + 5] = (byte) entry.DestinationID;

                        if ( entry.Hue == -1 )
                        {
                            continue;
                        }

                        packet[offset + ( oldStyle ? 17 : 18 )] = (byte) ( entry.Hue >> 8 );
                        packet[offset + ( oldStyle ? 18 : 19 )] = (byte) entry.Hue;
                    }

                    break;
                }
                case 0x25:
                {
                    int itemId = ( packet[5] << 8 ) | packet[6];

                    ItemIDFilterEntry entry = Items.FirstOrDefault( e => e.SourceID == itemId && e.Enabled );

                    if ( entry == null )
                    {
                        return false;
                    }

                    packet[5] = (byte) ( entry.DestinationID >> 8 );
                    packet[6] = (byte) entry.DestinationID;

                    bool oldStyle = packet.Length != 21;

                    if ( entry.Hue == -1 )
                    {
                        return false;
                    }

                    packet[oldStyle ? 18 : 19] = (byte) ( entry.Hue >> 8 );
                    packet[oldStyle ? 19 : 20] = (byte) entry.Hue;
                    break;
                }
                case 0x1A:
                {
                    int serial = ( packet[3] << 24 ) | ( packet[4] << 16 ) | ( packet[5] << 8 ) | packet[6];
                    int itemId = ( packet[7] << 8 ) | packet[8];

                    ItemIDFilterEntry entry = Items.FirstOrDefault( e => e.SourceID == itemId && e.Enabled );

                    if ( entry == null )
                    {
                        return false;
                    }

                    packet[7] = (byte) ( entry.DestinationID >> 8 );
                    packet[8] = (byte) entry.DestinationID;

                    if ( entry.Hue == -1 )
                    {
                        return false;
                    }

                    bool hasAmount = ( serial & 0x80000000 ) != 0;

                    int x = hasAmount ? ( packet[11] << 8 ) | packet[12] : ( packet[9] << 8 ) | packet[10];
                    int y = hasAmount ? ( packet[13] << 8 ) | packet[14] : ( packet[11] << 8 ) | packet[12];

                    bool hasLightSource = ( x & 0x8000 ) != 0;
                    bool hasHue = ( y & 0x8000 ) != 0;
                    bool hasFlags = ( y & 0x4000 ) != 0;

                    byte flags = 0;

                    if ( hasFlags )
                    {
                        int flagsOffset = 14;

                        if ( hasAmount )
                        {
                            flagsOffset += 2;
                        }

                        if ( hasLightSource )
                        {
                            flagsOffset += 1;
                        }

                        if ( hasHue )
                        {
                            flagsOffset += 2;
                        }

                        flags = packet[flagsOffset];
                    }

                    if ( !hasHue )
                    {
                        y |= 0x8000;

                        packet[hasAmount ? 13 : 11] = (byte) ( y >> 8 );
                        packet[hasAmount ? 14 : 12] = (byte) y;

                        Array.Resize( ref packet, length + 2 );
                        length = packet.Length;
                    }

                    int hueOffset = 14;

                    if ( hasAmount )
                    {
                        hueOffset += 2;
                    }

                    if ( hasLightSource )
                    {
                        hueOffset += 1;
                    }

                    packet[hueOffset] = (byte) ( entry.Hue >> 8 );
                    packet[hueOffset + 1] = (byte) entry.Hue;

                    if ( hasFlags )
                    {
                        packet[hueOffset + 2] = flags;
                    }

                    break;
                }
            }

            return false;
        }
    }

    public class ItemIDFilterEntry : SetPropertyNotifyChanged
    {
        private int _destinationId;
        private bool _enabled;
        private int _hue = -1;
        private int _sourceId;

        public int DestinationID
        {
            get => _destinationId;
            set => SetProperty( ref _destinationId, value );
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public int Hue
        {
            get => _hue;
            set => SetProperty( ref _hue, value );
        }

        public int SourceID
        {
            get => _sourceId;
            set => SetProperty( ref _sourceId, value );
        }
    }
}