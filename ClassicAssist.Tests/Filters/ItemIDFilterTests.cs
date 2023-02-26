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

using System.Collections.ObjectModel;
using ClassicAssist.Data.Filters;
using ClassicAssist.UO.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Filters
{
    [TestClass]
    public class ItemIDFilterTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            DynamicFilterEntry.Filters.Clear();
        }

        [TestMethod]
        public void WillReplaceItemID()
        {
            _ = new ItemIDFilter
            {
                Enabled = true,
                Items = new ObservableCollection<ItemIDFilterEntry>
                {
                    new ItemIDFilterEntry { SourceID = 1, DestinationID = 2, Enabled = true }
                }
            };

            IncomingPacketFilters.Initialize();

            byte[] packet =
            {
                0xF3, 0x00, 0x01, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            int length = packet.Length;

            IncomingPacketFilters.CheckPacket( ref packet, ref length );

            int itemId = ( packet[8] << 8 ) | packet[9];

            Assert.AreEqual( 2, itemId );
        }

        [TestMethod]
        public void WillReplaceHue()
        {
            _ = new ItemIDFilter
            {
                Enabled = true,
                Items = new ObservableCollection<ItemIDFilterEntry>
                {
                    new ItemIDFilterEntry { SourceID = 1, DestinationID = 1, Hue = 50, Enabled = true }
                }
            };

            IncomingPacketFilters.Initialize();

            byte[] packet =
            {
                0xF3, 0x00, 0x01, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            int length = packet.Length;

            IncomingPacketFilters.CheckPacket( ref packet, ref length );

            int hue = ( packet[21] << 8 ) | packet[22];

            Assert.AreEqual( 50, hue );
        }

        [TestMethod]
        public void WillReplaceItemID1A()
        {
            _ = new ItemIDFilter
            {
                Enabled = true,
                Items = new ObservableCollection<ItemIDFilterEntry>
                {
                    new ItemIDFilterEntry { SourceID = 1, DestinationID = 35, Enabled = true }
                }
            };

            IncomingPacketFilters.Initialize();

            byte[] packet = { 0x1A, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 };

            int length = packet.Length;

            IncomingPacketFilters.CheckPacket( ref packet, ref length );

            int itemId = ( packet[7] << 8 ) | packet[8];

            Assert.AreEqual( 35, itemId );
        }

        [TestMethod]
        public void WillReplaceHue1A()
        {
            _ = new ItemIDFilter
            {
                Enabled = true,
                Items = new ObservableCollection<ItemIDFilterEntry>
                {
                    new ItemIDFilterEntry { SourceID = 1, DestinationID = 1, Hue = 50, Enabled = true }
                }
            };

            IncomingPacketFilters.Initialize();

            byte[] packet =
            {
                0x1A, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00
            };

            int length = packet.Length;

            IncomingPacketFilters.CheckPacket( ref packet, ref length );

            int hue = ( packet[14] << 8 ) | packet[15];

            Assert.AreEqual( 50, hue );
        }

        [TestMethod]
        public void WillReplaceHue1ANoHue()
        {
            _ = new ItemIDFilter
            {
                Enabled = true,
                Items = new ObservableCollection<ItemIDFilterEntry>
                {
                    new ItemIDFilterEntry { SourceID = 1, DestinationID = 1, Hue = 50, Enabled = true }
                }
            };

            IncomingPacketFilters.Initialize();

            byte[] packet =
            {
                0x1A, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            int length = packet.Length;

            IncomingPacketFilters.CheckPacket( ref packet, ref length );

            int hue = ( packet[14] << 8 ) | packet[15];

            Assert.AreEqual( 50, hue );
        }
    }
}