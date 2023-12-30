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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data.Organizer;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Agents
{
    [TestClass]
    public class OrganizerTests
    {
        [TestMethod]
        public void WillOrganizeAnyHue()
        {
            Engine.Items.Clear();

            Item container = new Item( 0x40000000 )
            {
                Container = new ItemCollection( 0x40000000 )
                {
                    new Item( 0x40000001 ) { ID = 0xf06, Hue = 1000 },
                    new Item( 0x40000002 ) { ID = 0xf06, Hue = 1161 }
                }
            };
            Engine.Items.Add( container );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            OrganizerManager manager = OrganizerManager.GetInstance();

            OrganizerEntry entry = new OrganizerEntry();
            entry.Items.Add( new OrganizerItem { ID = 0xf06, Hue = -1, Amount = -1, Item = "Test" } );

            AutoResetEvent are = new AutoResetEvent( false );

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            Task.Run( () => manager.Organize( entry, container.Serial, container.Serial ) ).Wait();

            bool result = are.WaitOne( 4000 );

            Assert.IsTrue( result );

            manager.Items.Clear();
            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.PacketWaitEntries = null;
            Engine.Items.Clear();
            return;

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x06 )
                {
                    int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];

                    PacketWriter writer = new PacketWriter();
                    writer.Write( 0x3C );
                    writer.Seek( 19, SeekOrigin.Begin );
                    writer.Write( serial );

                    Engine.SendPacketToClient( writer );
                }

                if ( data[0] == 0x07 )
                {
                    are.Set();
                }
            }
        }

        [TestMethod]
        public void WillOrganizeOnlyHue()
        {
            Engine.Items.Clear();

            Item container = new Item( 0x40000000 )
            {
                Container = new ItemCollection( 0x40000000 )
                {
                    new Item( 0x40000001 ) { ID = 0xf06, Hue = 1000 },
                    new Item( 0x40000002 ) { ID = 0xf06, Hue = 1161 }
                }
            };
            Engine.Items.Add( container );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            OrganizerManager manager = OrganizerManager.GetInstance();

            OrganizerEntry entry = new OrganizerEntry();
            entry.Items.Add( new OrganizerItem { ID = 0xf06, Hue = 1161, Amount = -1, Item = "Test" } );

            AutoResetEvent are = new AutoResetEvent( false );

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            Task.Run( () => manager.Organize( entry, container.Serial, container.Serial ) ).Wait();

            bool result = are.WaitOne( 4000 );

            Assert.IsTrue( result );

            manager.Items.Clear();
            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.PacketWaitEntries = null;
            Engine.Items.Clear();
            return;

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x06 )
                {
                    int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];

                    PacketWriter writer = new PacketWriter();
                    writer.Write( 0x3C );
                    writer.Seek( 19, SeekOrigin.Begin );
                    writer.Write( serial );

                    Engine.SendPacketToClient( writer );
                }

                if ( data[0] == 0x07 )
                {
                    int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];

                    if ( serial == 0x40000002 )
                    {
                        are.Set();
                    }
                    else
                    {
                        Assert.Fail();
                    }
                }
            }
        }

        [TestMethod]
        public async Task WontOrganizeDestinationContainerDescendants()
        {
            // Destination container is descendant of source container
            // Don't organize destination container items
            Engine.Items.Clear();
            Engine.PacketWaitEntries = new PacketWaitEntries();

            Item sourceContainer = new Item( 0x40000000 )
            {
                Container = new ItemCollection( 0x40000000 )
                {
                    new Item( 0x40000003 ) { ID = 0x1f14, Owner = 0x40000000 }
                }
            };

            Item destinationContainer = new Item( 0x40000001 )
            {
                Container = new ItemCollection( 0x40000001 )
                {
                    new Item( 0x40000004 ) { ID = 0x1f14, Owner = 0x40000001 }
                }
            };

            sourceContainer.Container.Add( destinationContainer );
            Engine.Items.Add( sourceContainer );

            OrganizerManager manager = OrganizerManager.GetInstance();

            OrganizerEntry entry = new OrganizerEntry();
            entry.Items.Add( new OrganizerItem { ID = 0x1f14, Hue = -1, Amount = -1, Item = "Test" } );

            AutoResetEvent are = new AutoResetEvent( false );

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            await manager.Organize( entry, sourceContainer.Serial, destinationContainer.Serial );

            bool result = are.WaitOne( 4000 );

            Assert.IsTrue( result );

            manager.Items.Clear();
            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.PacketWaitEntries = null;
            Engine.Items.Clear();

            return;

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 )
                {
                    int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];

                    if ( serial == 0x40000003 )
                    {
                        are.Set();
                    }
                    else
                    {
                        Assert.Fail();
                    }
                }
            }
        }

        [TestMethod]
        public async Task WillOrganizeSourceInDestination()
        {
            // Source container is descendant of destination container
            // Organize source container items
            Engine.Items.Clear();
            Engine.PacketWaitEntries = new PacketWaitEntries();

            Item sourceContainer = new Item( 0x40000000 )
            {
                Container = new ItemCollection( 0x40000000 )
                {
                    new Item( 0x40000003 ) { ID = 0x1f14, Owner = 0x40000000 }
                }
            };

            Item destinationContainer = new Item( 0x40000001 )
            {
                Container = new ItemCollection( 0x40000001 )
                {
                    new Item( 0x40000004 ) { ID = 0x1f14, Owner = 0x40000001 }, sourceContainer
                }
            };

            Engine.Items.Add( destinationContainer );

            OrganizerManager manager = OrganizerManager.GetInstance();

            OrganizerEntry entry = new OrganizerEntry();
            entry.Items.Add( new OrganizerItem { ID = 0x1f14, Hue = -1, Amount = -1, Item = "Test" } );

            AutoResetEvent are = new AutoResetEvent( false );

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            await manager.Organize( entry, sourceContainer.Serial, destinationContainer.Serial );

            bool result = are.WaitOne( 4000 );

            Assert.IsTrue( result );

            manager.Items.Clear();
            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.PacketWaitEntries = null;
            Engine.Items.Clear();

            return;

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 )
                {
                    int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];

                    if ( serial == 0x40000003 )
                    {
                        are.Set();
                    }
                    else
                    {
                        Assert.Fail();
                    }
                }
            }
        }
    }
}