using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Assistant;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UI.ViewModels.Agents;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ClassicAssist.Tests.Agents
{
    [TestClass]
    public class AutolootTests
    {
        [TestMethod]
        public void WillRehueMatchingItemProperties()
        {
            Cliloc.Initialize( () => new Dictionary<int, string> { { 1060413, "faster casting ~NUMBER~" } } );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = true,
                Autoloot = false,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = 0x108a
            };

            AutolootConstraintEntry constraintEntry = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Faster Casting" ),
                Operator = AutolootOperator.GreaterThan,
                Value = 1
            };

            lootEntry.Constraints.Add( constraintEntry );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketReceivedEvent( byte[] data, int length )
            {
                if ( data[0] == 0x25 )
                {
                    are.Set();
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketReceivedEvent += OnPacketReceivedEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );
                entry.Packet = containerContentsPacket;
                entry.Lock.Set();

                byte[] propertiesPacket =
                {
                    0xD6, 0x00, 0x53, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x00, 0x00, 0x01, 0x6E, 0x08, 0xB4, 0x00,
                    0x0F, 0xA0, 0xEA, 0x00, 0x00, 0x00, 0x10, 0x5E, 0x94, 0x00, 0x02, 0x31, 0x00, 0x00, 0x10, 0x2E,
                    0x64, 0x00, 0x14, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x34, 0x00, 0x34, 0x00, 0x30, 0x00, 0x38,
                    0x00, 0x35, 0x00, 0x09, 0x00, 0x39, 0x00, 0x00, 0x10, 0x2E, 0x3C, 0x00, 0x02, 0x32, 0x00, 0x00,
                    0x10, 0x2E, 0x3D, 0x00, 0x02, 0x31, 0x00, 0x00, 0x10, 0x2E, 0x83, 0x00, 0x02, 0x34, 0x00, 0x00,
                    0x00, 0x00, 0x00
                };

                handler = IncomingPacketHandlers.GetHandler( 0xD6 );

                handler.OnReceive( new PacketReader( propertiesPacket, propertiesPacket.Length, false ) );
            };

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;

            Engine.InternalPacketReceivedEvent -= OnPacketReceivedEvent;
        }

        [TestMethod]
        public void WillRehueMatchingObjectProperties()
        {
            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = true,
                Autoloot = false,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = 0x108a
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Hue" ),
                Operator = AutolootOperator.Equal,
                Value = 0
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketReceivedEvent( byte[] data, int length )
            {
                if ( data[0] == 0x25 )
                {
                    are.Set();
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketReceivedEvent += OnPacketReceivedEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );
                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;

            Engine.InternalPacketReceivedEvent -= OnPacketReceivedEvent;
        }

        [TestMethod]
        public void WillLootMatchingObjectProperties()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = 0x108a
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "ID" ),
                Operator = AutolootOperator.Equal,
                Value = 0x108a
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 || data[0] == 0x08 )
                {
                    are.Set();
                }
                else if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );
                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WontLootDisabledInGuardzone()
        {
            Engine.Player = new PlayerMobile( 0x01 ) { X = 652, Y = 869 };
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 )
            {
                ID = 0x2006,
                Count = 400,
                Container = new ItemCollection( 0x40000000 ),
                X = 652,
                Y = 869
            };
            corpse.Container.Add( new Item( 0x4313FC5E ) { ID = 0x108a } );

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true, DisableInGuardzone = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = 0x108a
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Hue" ),
                Operator = AutolootOperator.Equal,
                Value = 0
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                    return;
                }

                Assert.Fail();
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );
                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WontRehueNotEnabledEntry()
        {
            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = true,
                Autoloot = false,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = 0x108a,
                Enabled = false
            };

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            void OnPacketReceivedEvent( byte[] data, int length )
            {
                Assert.Fail();
            }

            Engine.InternalPacketReceivedEvent += OnPacketReceivedEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );
                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;

            Engine.InternalPacketReceivedEvent -= OnPacketReceivedEvent;
        }

        [TestMethod]
        public void WontRehueNotEnabled()
        {
            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = true,
                Autoloot = false,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = 0x108a,
                Enabled = true
            };

            vm.Items.Add( lootEntry );
            vm.Enabled = false;

            Engine.PacketWaitEntries = new PacketWaitEntries();

            void OnPacketReceivedEvent( byte[] data, int length )
            {
                Assert.Fail();
            }

            Engine.InternalPacketReceivedEvent += OnPacketReceivedEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );
                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;

            Engine.InternalPacketReceivedEvent -= OnPacketReceivedEvent;
        }

        [TestMethod]
        public void WontMatchNotIncluded()
        {
            PropertyEntry property1 = new PropertyEntry { ClilocIndex = -1, Clilocs = new[] { 1 }, Name = "Test" };
            PropertyEntry property2 = new PropertyEntry { ClilocIndex = -1, Clilocs = new[] { 2 }, Name = "Test 2" };

            ItemCollection container = new ItemCollection( 2 )
            {
                new Item( 1 ) { Properties = new[] { new Property { Cliloc = 1, Text = "Test" } } }
            };

            AutolootEntry filter = new AutolootEntry
            {
                Constraints = new ObservableCollection<AutolootConstraintEntry>
                {
                    new AutolootConstraintEntry
                    {
                        Operator = AutolootOperator.Equal, Property = property1, Value = 0
                    }
                }
            };

            IEnumerable<Item> matchItems = AutolootHelpers.AutolootFilter( container.GetItems(), filter );

            Assert.IsTrue( matchItems.Any() );

            container.Clear();

            container.Add( new Item( 3 )
            {
                Properties = new[]
                {
                    new Property { Cliloc = 1, Text = "Test" }, new Property { Cliloc = 2, Text = "Test 2" }
                }
            } );

            filter.Constraints.Add( new AutolootConstraintEntry
            {
                Operator = AutolootOperator.NotPresent, Property = property2, Value = 0
            } );

            matchItems = AutolootHelpers.AutolootFilter( container.GetItems(), filter );

            Assert.IsTrue( !matchItems.Any() );

            Engine.Items.Clear();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ActionPacketQueue.Clear();
        }

        [TestMethod]
        public void WontLootDisabledLootHumanoids()
        {
            Engine.Player = new PlayerMobile( 0x01 ) { X = 652, Y = 869 };
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 )
            {
                ID = 0x2006,
                Count = 400,
                Container = new ItemCollection( 0x40000000 ),
                X = 652,
                Y = 869
            };
            corpse.Container.Add( new Item( 0x4313FC5E ) { ID = 0x108a } );

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true, LootHumanoids = false };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = 0x108a
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Hue" ),
                Operator = AutolootOperator.Equal,
                Value = 0
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                    return;
                }

                are.Set();
                Assert.Fail();
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );
                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            are.WaitOne( 2000 );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillLootByPriority()
        {
            Engine.Player = new PlayerMobile( 0x01 ) { X = 652, Y = 869 };
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );
            Engine.ClientVersion = Version.Parse( "7.0.45.0" );

            Item corpse = new Item( 0x40000000 )
            {
                ID = 0x2006,
                Count = 400,
                Container = new ItemCollection( 0x40000000 ),
                X = 652,
                Y = 869
            };
            corpse.Container.Add( new Item( 0x4546E4AC ) { ID = 0xaa } );
            corpse.Container.Add( new Item( 0x44215a1c ) { ID = 0xbb } );

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true, LootHumanoids = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = 0xaa,
                Priority = AutolootPriority.Normal
            };

            AutolootEntry lootEntry2 = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = 0xbb,
                Priority = AutolootPriority.High
            };

            vm.Items.Add( lootEntry );
            vm.Items.Add( lootEntry2 );

            /*
             * 0x44215a1c should be dragged first because the priority is higher
             * thats all the test checks
             */

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            bool lootedItem = false;

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0xD6 || data[0] == 0x08 )
                {
                    return;
                }

                int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];

                if ( !serial.Equals( 0x44215a1c ) && !lootedItem )
                {
                    are.Set();
                    Assert.Fail( "Autoloot wrong item" );
                }

                lootedItem = true;
                are.Set();
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x2D, 0x00, 0x02, 0x45, 0x46, 0xE4, 0xAC, 0x00, 0xAA, 0x00, 0x00, 0x01, 0x00, 0x2D,
                    0x00, 0x50, 0x00, 0x41, 0x5D, 0x8E, 0xDB, 0x05, 0x54, 0x44, 0x21, 0x5A, 0x1C, 0x00, 0xBB, 0x00,
                    0x00, 0x01, 0x00, 0x38, 0x00, 0x70, 0x00, 0x41, 0x5D, 0x8E, 0xDB, 0x04, 0x8D
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );
                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            are.WaitOne( 2000 );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillLootItemPropertiesLMI()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = -1
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                // 1151489
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Lesser Magic Item" )
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 || data[0] == 0x08 )
                {
                    are.Set();
                }
                else if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );

                byte[] properties =
                {
                    0xD6, 0x00, 0x00, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x11, 0x92, 0x01, 0x00
                };

                PacketHandler propertiesHandler = IncomingPacketHandlers.GetHandler( 0xD6 );

                propertiesHandler.OnReceive( new PacketReader( properties, properties.Length, false ) );

                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            Cliloc.Initialize( () => new Dictionary<int, string> { { 1151489, "Lesser Magic Item" } } );

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillLootItemPropertiesClilocAsText()
        {
            /*
             * UOAlive artifact properties as strings instead of clilocs
             */
            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = -1
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Greater Artifact" )
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 || data[0] == 0x08 )
                {
                    are.Set();
                }
                else if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );

                byte[] properties =
                {
                    0xD6, 0x00, 0x00, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x0F, 0xEA, 0x1B, 0x00
                };

                byte[] bytes = Encoding.Unicode.GetBytes( "<BASEFONT COLOR=#FAFF06>Greater Artifact" );

                byte[] newProperties = properties.Append( (byte) bytes.Length ).ToArray().Concat( bytes ).ToArray();

                PacketHandler propertiesHandler = IncomingPacketHandlers.GetHandler( 0xD6 );

                propertiesHandler.OnReceive( new PacketReader( newProperties, newProperties.Length, false ) );

                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            Cliloc.Initialize( () =>
                new Dictionary<int, string> { { 1042971, "~NOTHING~" }, { 1151493, "Greater Artifact" } } );
            AutolootManager.GetInstance().MatchTextValue = () => true;

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillAutolootCustomProperties()
        {
            string dataPath = Path.Combine( Environment.CurrentDirectory, "Data" );

            if ( !Directory.Exists( dataPath ) )
            {
                Directory.CreateDirectory( dataPath );
            }

            string propertiesFile = Path.Combine( dataPath, "Properties.Custom.json" );

            if ( File.Exists( propertiesFile ) )
            {
                File.Delete( propertiesFile );
            }

            PropertyEntry propertyEntry = new PropertyEntry
            {
                ClilocIndex = -1,
                Clilocs = new[] { 1010141 },
                ConstraintType = PropertyType.Properties,
                Name = "Test"
            };

            Cliloc.Initialize( () => new Dictionary<int, string> { { propertyEntry.Clilocs[0], propertyEntry.Name } } );

            using ( FileStream file = File.OpenWrite( propertiesFile ) )
            {
                using ( StreamWriter sw = new StreamWriter( file ) )
                {
                    string json = JsonConvert.SerializeObject( new[] { propertyEntry } );

                    sw.WriteLine( json );
                }
            }

            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            propertyEntry = vm.Constraints.FirstOrDefault( e => e.Name == "Test" );

            if ( propertyEntry == null )
            {
                Assert.Fail( "Constraint not present" );
            }

            AutolootEntry autolootEntry = new AutolootEntry
            {
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>( new[]
                {
                    new AutolootConstraintEntry { Property = propertyEntry }
                } ),
                ID = -1
            };

            vm.Items.Add( autolootEntry );

            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                switch ( data[0] )
                {
                    case 0x07:
                    case 0x08:
                        are.Set();
                        break;
                    case 0xD6:
                    case 0x06:
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
            }

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );

                byte[] properties =
                {
                    0xD6, 0x00, 0x00, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x0F, 0x69, 0xDD, 0x00
                };

                PacketHandler propertiesHandler = IncomingPacketHandlers.GetHandler( 0xD6 );

                propertiesHandler.OnReceive( new PacketReader( properties, properties.Length, false ) );

                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            vm.OnCorpseEvent( 0x40000000 );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillLootSkillBonus()
        {
            Cliloc.Initialize( () =>
                new Dictionary<int, string> { { 1060451, "~1_skillname~ +~2_val~" }, { 1044085, "Magery" } } );

            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = -1
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Skill Bonus" ),
                Value = 15,
                Additional = "Magery",
                Operator = AutolootOperator.GreaterThan
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 || data[0] == 0x08 )
                {
                    are.Set();
                }
                else if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x40, 0x05, 0x00, 0xf3, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );

                byte[] properties =
                {
                    0xD6, 0x00, 0x73, 0x00, 0x01, 0x40, 0x05, 0x00, 0xF3, 0x00, 0x00, 0x01, 0x12, 0x9D, 0x1A, 0x00,
                    0x10, 0x67, 0x98, 0x00, 0x00, 0x00, 0x10, 0x5E, 0x94, 0x00, 0x02, 0x31, 0x00, 0x00, 0x10, 0x2E,
                    0x63, 0x00, 0x16, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x34, 0x00, 0x34, 0x00, 0x30, 0x00, 0x38,
                    0x00, 0x35, 0x00, 0x09, 0x00, 0x32, 0x00, 0x30, 0x00, 0x00, 0x10, 0x2E, 0x64, 0x00, 0x16, 0x23,
                    0x00, 0x31, 0x00, 0x30, 0x00, 0x34, 0x00, 0x34, 0x00, 0x31, 0x00, 0x31, 0x00, 0x30, 0x00, 0x09,
                    0x00, 0x32, 0x00, 0x30, 0x00, 0x00, 0x10, 0x2E, 0x5C, 0x00, 0x02, 0x35, 0x00, 0x00, 0x10, 0x2E,
                    0x58, 0x00, 0x02, 0x33, 0x00, 0x00, 0x10, 0x2E, 0x83, 0x00, 0x04, 0x32, 0x00, 0x30, 0x00, 0x00,
                    0x00, 0x00, 0x00
                };

                PacketHandler propertiesHandler = IncomingPacketHandlers.GetHandler( 0xD6 );

                propertiesHandler.OnReceive( new PacketReader( properties, properties.Length, false ) );

                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WontLootSkillBonusLessThan()
        {
            Cliloc.Initialize( () =>
                new Dictionary<int, string> { { 1060451, "~1_skillname~ +~2_val~" }, { 1044085, "Magery" } } );

            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = -1
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Skill Bonus" ),
                Value = 5,
                Additional = "Magery",
                Operator = AutolootOperator.LessThan
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 || data[0] == 0x08 )
                {
                    are.Set();
                }
                else if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x40, 0x05, 0x00, 0xf3, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );

                byte[] properties =
                {
                    0xD6, 0x00, 0x73, 0x00, 0x01, 0x40, 0x05, 0x00, 0xF3, 0x00, 0x00, 0x01, 0x12, 0x9D, 0x1A, 0x00,
                    0x10, 0x67, 0x98, 0x00, 0x00, 0x00, 0x10, 0x5E, 0x94, 0x00, 0x02, 0x31, 0x00, 0x00, 0x10, 0x2E,
                    0x63, 0x00, 0x16, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x34, 0x00, 0x34, 0x00, 0x30, 0x00, 0x38,
                    0x00, 0x35, 0x00, 0x09, 0x00, 0x32, 0x00, 0x30, 0x00, 0x00, 0x10, 0x2E, 0x64, 0x00, 0x16, 0x23,
                    0x00, 0x31, 0x00, 0x30, 0x00, 0x34, 0x00, 0x34, 0x00, 0x31, 0x00, 0x31, 0x00, 0x30, 0x00, 0x09,
                    0x00, 0x32, 0x00, 0x30, 0x00, 0x00, 0x10, 0x2E, 0x5C, 0x00, 0x02, 0x35, 0x00, 0x00, 0x10, 0x2E,
                    0x58, 0x00, 0x02, 0x33, 0x00, 0x00, 0x10, 0x2E, 0x83, 0x00, 0x04, 0x32, 0x00, 0x30, 0x00, 0x00,
                    0x00, 0x00, 0x00
                };

                PacketHandler propertiesHandler = IncomingPacketHandlers.GetHandler( 0xD6 );

                propertiesHandler.OnReceive( new PacketReader( properties, properties.Length, false ) );

                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 2000 );

            Assert.IsFalse( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillLootTalismanSkillBonus()
        {
            Cliloc.Initialize( () =>
                new Dictionary<int, string> { { 1072394, "~1_NAME~ Bonus: ~2_val~%" }, { 1044067, "Blacksmithing" } } );

            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = -1
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Skill Bonus" ),
                Value = 15,
                Additional = "Blacksmithing",
                Operator = AutolootOperator.GreaterThan
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 || data[0] == 0x08 )
                {
                    are.Set();
                }
                else if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x40, 0x05, 0x08, 0x0f, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );

                byte[] properties =
                {
                    0xD6, 0x00, 0x69, 0x00, 0x01, 0x40, 0x05, 0x08, 0x0F, 0x00, 0x00, 0x00, 0x5D, 0xA4, 0x34, 0x00,
                    0x10, 0x5D, 0x10, 0x00, 0x10, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x31, 0x00, 0x38, 0x00, 0x32,
                    0x00, 0x38, 0x00, 0x35, 0x00, 0x00, 0x10, 0x5E, 0x94, 0x00, 0x02, 0x31, 0x00, 0x00, 0x10, 0x5D,
                    0x03, 0x00, 0x16, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x37, 0x00, 0x32, 0x00, 0x34, 0x00, 0x32,
                    0x00, 0x33, 0x00, 0x09, 0x00, 0x34, 0x00, 0x38, 0x00, 0x00, 0x10, 0x5D, 0x0A, 0x00, 0x16, 0x23,
                    0x00, 0x31, 0x00, 0x30, 0x00, 0x34, 0x00, 0x34, 0x00, 0x30, 0x00, 0x36, 0x00, 0x37, 0x00, 0x09,
                    0x00, 0x32, 0x00, 0x32, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                PacketHandler propertiesHandler = IncomingPacketHandlers.GetHandler( 0xD6 );

                propertiesHandler.OnReceive( new PacketReader( properties, properties.Length, false ) );

                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillLootTalismanSkillBonusMultipleSameName()
        {
            Cliloc.Initialize( () => new Dictionary<int, string>
            {
                { 1072394, "~1_NAME~ Bonus: ~2_val~%" },
                { 1072395, "~1_NAME~ Exceptional Bonus: ~2_val~%" },
                { 1072393, "Glassblowing" }
            } );

            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = -1
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Skill Bonus" ),
                Value = 12,
                Additional = "Glassblowing",
                Operator = AutolootOperator.Equal
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 || data[0] == 0x08 )
                {
                    are.Set();
                }
                else if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x40, 0x05, 0x08, 0x72, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );

                byte[] properties =
                {
                    0xD6, 0x00, 0x75, 0x00, 0x01, 0x40, 0x05, 0x08, 0x72, 0x00, 0x00, 0x03, 0x28, 0x5B, 0x0B, 0x00,
                    0x10, 0x57, 0xAF, 0x00, 0x00, 0x00, 0x10, 0x5E, 0x94, 0x00, 0x02, 0x31, 0x00, 0x00, 0x10, 0x5D,
                    0x04, 0x00, 0x16, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x37, 0x00, 0x32, 0x00, 0x34, 0x00, 0x31,
                    0x00, 0x39, 0x00, 0x09, 0x00, 0x34, 0x00, 0x38, 0x00, 0x00, 0x10, 0x5D, 0x0B, 0x00, 0x16, 0x23,
                    0x00, 0x31, 0x00, 0x30, 0x00, 0x37, 0x00, 0x32, 0x00, 0x33, 0x00, 0x39, 0x00, 0x33, 0x00, 0x09,
                    0x00, 0x31, 0x00, 0x32, 0x00, 0x00, 0x10, 0x5D, 0x0A, 0x00, 0x16, 0x23, 0x00, 0x31, 0x00, 0x30,
                    0x00, 0x37, 0x00, 0x32, 0x00, 0x33, 0x00, 0x39, 0x00, 0x33, 0x00, 0x09, 0x00, 0x31, 0x00, 0x36,
                    0x00, 0x00, 0x00, 0x00, 0x00
                };
                PacketHandler propertiesHandler = IncomingPacketHandlers.GetHandler( 0xD6 );

                propertiesHandler.OnReceive( new PacketReader( properties, properties.Length, false ) );

                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            lootEntry.Constraints.Clear();
            lootEntry.Constraints.Add( new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Skill Bonus" ),
                Value = 16,
                Additional = "Glassblowing",
                Operator = AutolootOperator.Equal
            } );

            vm.OnCorpseEvent( corpse.Serial );

            result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillLootSkillBonusOSILowercase()
        {
            Cliloc.Initialize( () =>
                new Dictionary<int, string> { { 1060451, "~1_skillname~ +~2_val~" }, { 1043181, "animal lore" } } );

            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = -1
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Skill Bonus" ),
                Value = 5,
                Additional = "Animal Lore",
                Operator = AutolootOperator.GreaterThan
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 || data[0] == 0x08 )
                {
                    are.Set();
                }
                else if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x46, 0x6B, 0xB4, 0x9E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );

                byte[] properties =
                {
                    0xD6, 0x00, 0xC5, 0x00, 0x01, 0x46, 0x6B, 0xB4, 0x9E, 0x00, 0x00, 0xF8, 0xBD, 0x34, 0x5A, 0x00,
                    0x11, 0x93, 0x0D, 0x00, 0x22, 0x23, 0x00, 0x31, 0x00, 0x31, 0x00, 0x35, 0x00, 0x31, 0x00, 0x36,
                    0x00, 0x38, 0x00, 0x38, 0x00, 0x09, 0x00, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x32, 0x00, 0x34,
                    0x00, 0x32, 0x00, 0x33, 0x00, 0x34, 0x00, 0x00, 0x10, 0x04, 0x2B, 0x00, 0x00, 0x00, 0x10, 0x5E,
                    0x94, 0x00, 0x02, 0x31, 0x00, 0x00, 0x10, 0x2E, 0x65, 0x00, 0x14, 0x23, 0x00, 0x31, 0x00, 0x30,
                    0x00, 0x34, 0x00, 0x33, 0x00, 0x31, 0x00, 0x38, 0x00, 0x31, 0x00, 0x09, 0x00, 0x35, 0x00, 0x00,
                    0x10, 0x2E, 0x63, 0x00, 0x16, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x34, 0x00, 0x32, 0x00, 0x33,
                    0x00, 0x37, 0x00, 0x36, 0x00, 0x09, 0x00, 0x31, 0x00, 0x30, 0x00, 0x00, 0x10, 0x2E, 0x50, 0x00,
                    0x04, 0x31, 0x00, 0x30, 0x00, 0x00, 0x10, 0x2E, 0x50, 0x00, 0x04, 0x31, 0x00, 0x30, 0x00, 0x00,
                    0x10, 0x2E, 0x57, 0x00, 0x02, 0x34, 0x00, 0x00, 0x10, 0x2E, 0x58, 0x00, 0x02, 0x34, 0x00, 0x00,
                    0x10, 0x2E, 0x3C, 0x00, 0x02, 0x34, 0x00, 0x00, 0x10, 0x2F, 0x1F, 0x00, 0x0E, 0x32, 0x00, 0x35,
                    0x00, 0x35, 0x00, 0x09, 0x00, 0x32, 0x00, 0x35, 0x00, 0x35, 0x00, 0x00, 0x11, 0x92, 0x05, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00
                };
                PacketHandler propertiesHandler = IncomingPacketHandlers.GetHandler( 0xD6 );

                propertiesHandler.OnReceive( new PacketReader( properties, properties.Length, false ) );

                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillLootSkillBonusNoName()
        {
            Cliloc.Initialize( () =>
                new Dictionary<int, string> { { 1060451, "~1_skillname~ +~2_val~" }, { 1043181, "animal lore" } } );

            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = -1
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == "Skill Bonus" ),
                Value = 5,
                Additional = string.Empty,
                Operator = AutolootOperator.GreaterThan
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 || data[0] == 0x08 )
                {
                    are.Set();
                }
                else if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x46, 0x6B, 0xB4, 0x9E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );

                byte[] properties =
                {
                    0xD6, 0x00, 0xC5, 0x00, 0x01, 0x46, 0x6B, 0xB4, 0x9E, 0x00, 0x00, 0xF8, 0xBD, 0x34, 0x5A, 0x00,
                    0x11, 0x93, 0x0D, 0x00, 0x22, 0x23, 0x00, 0x31, 0x00, 0x31, 0x00, 0x35, 0x00, 0x31, 0x00, 0x36,
                    0x00, 0x38, 0x00, 0x38, 0x00, 0x09, 0x00, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x32, 0x00, 0x34,
                    0x00, 0x32, 0x00, 0x33, 0x00, 0x34, 0x00, 0x00, 0x10, 0x04, 0x2B, 0x00, 0x00, 0x00, 0x10, 0x5E,
                    0x94, 0x00, 0x02, 0x31, 0x00, 0x00, 0x10, 0x2E, 0x65, 0x00, 0x14, 0x23, 0x00, 0x31, 0x00, 0x30,
                    0x00, 0x34, 0x00, 0x33, 0x00, 0x31, 0x00, 0x38, 0x00, 0x31, 0x00, 0x09, 0x00, 0x35, 0x00, 0x00,
                    0x10, 0x2E, 0x63, 0x00, 0x16, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x34, 0x00, 0x32, 0x00, 0x33,
                    0x00, 0x37, 0x00, 0x36, 0x00, 0x09, 0x00, 0x31, 0x00, 0x30, 0x00, 0x00, 0x10, 0x2E, 0x50, 0x00,
                    0x04, 0x31, 0x00, 0x30, 0x00, 0x00, 0x10, 0x2E, 0x50, 0x00, 0x04, 0x31, 0x00, 0x30, 0x00, 0x00,
                    0x10, 0x2E, 0x57, 0x00, 0x02, 0x34, 0x00, 0x00, 0x10, 0x2E, 0x58, 0x00, 0x02, 0x34, 0x00, 0x00,
                    0x10, 0x2E, 0x3C, 0x00, 0x02, 0x34, 0x00, 0x00, 0x10, 0x2F, 0x1F, 0x00, 0x0E, 0x32, 0x00, 0x35,
                    0x00, 0x35, 0x00, 0x09, 0x00, 0x32, 0x00, 0x35, 0x00, 0x35, 0x00, 0x00, 0x11, 0x92, 0x05, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00
                };
                PacketHandler propertiesHandler = IncomingPacketHandlers.GetHandler( 0xD6 );

                propertiesHandler.OnReceive( new PacketReader( properties, properties.Length, false ) );

                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillLootItemIDMultipleEqual()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x40000001, 0x01 ) { Container = new ItemCollection( 0x40000001 ) };
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );
            
            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = false,
                Autoloot = true,
                Constraints = new ObservableCollection<AutolootConstraintEntry>(),
                ID = -1
            };

            AutolootConstraintEntry autolootConstraint = new AutolootConstraintEntry
            {
                Property = vm.Constraints.FirstOrDefault( c => c.Name == Strings.ID__Multiple_ ),
                Values = new ObservableCollection<int>( new[] { 0x108A } ),
                Additional = string.Empty,
                Operator = AutolootOperator.Equal
            };
            lootEntry.Constraints.Add( autolootConstraint );

            vm.Items.Add( lootEntry );
            
            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            Engine.InternalPacketSentEvent += OnPacketSentEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x46, 0x6B, 0xB4, 0x9E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x64
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );

                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;
            Engine.InternalPacketSentEvent -= OnPacketSentEvent;
            Engine.Player = null;
            return;

            void OnPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 || data[0] == 0x08 )
                {
                    are.Set();
                }
                else if ( data[0] == 0xD6 || data[0] == 0x06 )
                {
                }
                else
                {
                    Assert.Fail();
                }
            }
        }
    }
}