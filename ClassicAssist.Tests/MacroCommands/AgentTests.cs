using System;
using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Data.Counters;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Organizer;
using ClassicAssist.UI.ViewModels.Agents;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class AgentTests
    {
        [TestMethod]
        public void CounterWillCountType()
        {
            PlayerMobile player = new PlayerMobile( 0x01 );
            Item backpack = new Item( 0x02 ) { Layer = Layer.Backpack, Container = new ItemCollection( 0x02 ) };
            player.SetLayer( Layer.Backpack, backpack.Serial );
            Engine.Items.Add( backpack );
            Engine.Player = player;

            CountersManager manager = CountersManager.GetInstance();
            manager.Items.Add( new CountersAgentEntry { Name = "test", Color = -1, Graphic = 0xff } );

            backpack.Container.Add( new Item( 0x03 ) { ID = 0xff, Owner = 0x02, Count = 1 } );

            manager.RecountAll = () =>
            {
                foreach ( CountersAgentEntry entry in manager.Items )
                {
                    entry.Recount();
                }
            };

            manager.RecountAll();

            int count = AgentCommands.Counter( "test" );

            Assert.AreEqual( 1, count );
        }

        [TestMethod]
        public void WillOrganizeLimitAmount()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WillOrganizeLimitAmount",
                AppDomain.CurrentDomain.Evidence,
                AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () =>
            {
                Dictionary<int, int> serialAmount = new Dictionary<int, int>();

                void OnInternalPacketSentEvent( byte[] data, int length )
                {
                    if ( data[0] == 0x07 )
                    {
                        int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                        int amount = ( data[5] << 8 ) | data[6];

                        if ( serialAmount.ContainsKey( serial ) )
                        {
                            serialAmount.Remove( serial );
                        }

                        serialAmount.Add( serial, amount );
                    }

                    if ( data[0] == 0x08 )
                    {
                        int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                        int containerSerial = ( data[11] << 24 ) | ( data[12] << 16 ) | ( data[13] << 8 ) | data[14];

                        Item sourceItem = Engine.Items.GetItem( serial );
                        Item containerItem = Engine.Items.GetItem( containerSerial );

                        if ( serialAmount.ContainsKey( serial ) )
                        {
                            sourceItem.Count = serialAmount[serial];
                        }

                        containerItem.Container.Add( sourceItem );
                    }
                }

                Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

                OrganizerEntry entry = new OrganizerEntry();

                entry.Items.Add( new OrganizerItem { ID = 0xff, Item = "Shmoo", Amount = 10 } );

                Item sourceContainer = new Item( 0x40000004 ) { Container = new ItemCollection( 0x40000004 ) };
                sourceContainer.Container.Add(
                    new Item( 0x40000005, 0x40000004 ) { ID = 0xff, Count = 5, Owner = 0x40000004 } );
                sourceContainer.Container.Add(
                    new Item( 0x40000006, 0x40000004 ) { ID = 0xff, Count = 100, Owner = 0x40000004 } );

                Engine.Items.Add( sourceContainer );

                Item destinationContainer = new Item( 0x40000007 ) { Container = new ItemCollection( 0x40000007 ) };

                Engine.Items.Add( destinationContainer );

                entry.SourceContainer = sourceContainer.Serial;
                entry.DestinationContainer = destinationContainer.Serial;

                Engine.PacketWaitEntries = new PacketWaitEntries();
                OrganizerTabViewModel vm = new OrganizerTabViewModel();

                vm.Organize( entry ).Wait();

                Item[] destinationItems = destinationContainer.Container.GetItems();

                Assert.AreEqual( entry.Items[0].Amount,
                    destinationItems.Sum( i => i.ID == entry.Items[0].ID ? i.Count : 0 ) );

                Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
                Engine.Items.Clear();
                Engine.PacketWaitEntries = null;
            } );
        }

        [TestMethod]
        public void WillOrganizeNoLimit()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WillOrganizeNoLimit",
                AppDomain.CurrentDomain.Evidence,
                AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () =>
            {
                Dictionary<int, int> serialAmount = new Dictionary<int, int>();

                void OnInternalPacketSentEvent( byte[] data, int length )
                {
                    if ( data[0] == 0x07 )
                    {
                        int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                        int amount = ( data[5] << 8 ) | data[6];

                        if ( serialAmount.ContainsKey( serial ) )
                        {
                            serialAmount.Remove( serial );
                        }

                        serialAmount.Add( serial, amount );
                    }

                    if ( data[0] == 0x08 )
                    {
                        int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                        int containerSerial = ( data[11] << 24 ) | ( data[12] << 16 ) | ( data[13] << 8 ) | data[14];

                        Item sourceItem = Engine.Items.GetItem( serial );
                        Item containerItem = Engine.Items.GetItem( containerSerial );

                        if ( serialAmount.ContainsKey( serial ) )
                        {
                            sourceItem.Count = serialAmount[serial];
                        }

                        containerItem.Container.Add( sourceItem );
                    }
                }

                Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

                OrganizerEntry entry = new OrganizerEntry();

                entry.Items.Add( new OrganizerItem { ID = 0xff, Item = "Shmoo", Amount = -1 } );

                int amnt = 13013;

                Item sourceContainer = new Item( 0x40000004 ) { Container = new ItemCollection( 0x40000004 ) };
                sourceContainer.Container.Add(
                    new Item( 0x40000005, 0x40000004 ) { ID = 0xff, Count = amnt, Owner = 0x40000004 } );

                Engine.Items.Add( sourceContainer );

                Item destinationContainer = new Item( 0x40000007 ) { Container = new ItemCollection( 0x40000007 ) };

                Engine.Items.Add( destinationContainer );

                entry.SourceContainer = sourceContainer.Serial;
                entry.DestinationContainer = destinationContainer.Serial;

                OrganizerTabViewModel vm = new OrganizerTabViewModel();
                Engine.PacketWaitEntries = new PacketWaitEntries();

                vm.Organize( entry ).Wait();

                Item[] destinationItems = destinationContainer.Container.GetItems();

                Assert.AreEqual( amnt, destinationItems.Sum( i => i.Count ) );

                Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
                Engine.Items.Clear();
                Engine.PacketWaitEntries = null;
            } );
        }

        [TestMethod]
        public void WillOrganizeNoLimitMultiple()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WillOrganizeNoLimitMultiple",
                AppDomain.CurrentDomain.Evidence,
                AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () =>
            {
                Dictionary<int, int> serialAmount = new Dictionary<int, int>();

                void OnInternalPacketSentEvent( byte[] data, int length )
                {
                    if ( data[0] == 0x07 )
                    {
                        int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                        int amount = ( data[5] << 8 ) | data[6];

                        if ( serialAmount.ContainsKey( serial ) )
                        {
                            serialAmount.Remove( serial );
                        }

                        serialAmount.Add( serial, amount );
                    }

                    if ( data[0] == 0x08 )
                    {
                        int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                        int containerSerial = ( data[11] << 24 ) | ( data[12] << 16 ) | ( data[13] << 8 ) | data[14];

                        Item sourceItem = Engine.Items.GetItem( serial );
                        Item containerItem = Engine.Items.GetItem( containerSerial );

                        if ( serialAmount.ContainsKey( serial ) )
                        {
                            sourceItem.Count = serialAmount[serial];
                        }

                        containerItem.Container.Add( sourceItem );
                    }
                }

                Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

                OrganizerEntry entry = new OrganizerEntry();

                entry.Items.Add( new OrganizerItem { ID = 0xff, Item = "Shmoo", Amount = -1 } );
                entry.Items.Add( new OrganizerItem { ID = 0xfe, Item = "Shmoo", Amount = -1 } );

                int amnt = 13013;

                Item sourceContainer = new Item( 0x40000004 ) { Container = new ItemCollection( 0x40000004 ) };
                sourceContainer.Container.Add(
                    new Item( 0x40000005, 0x40000004 ) { ID = 0xff, Count = amnt, Owner = 0x40000004 } );
                sourceContainer.Container.Add(
                    new Item( 0x40000006, 0x40000004 ) { ID = 0xfe, Count = amnt, Owner = 0x40000004 } );

                Engine.Items.Add( sourceContainer );

                Item destinationContainer = new Item( 0x40000007 ) { Container = new ItemCollection( 0x40000007 ) };

                Engine.Items.Add( destinationContainer );

                entry.SourceContainer = sourceContainer.Serial;
                entry.DestinationContainer = destinationContainer.Serial;

                OrganizerTabViewModel vm = new OrganizerTabViewModel();
                Engine.PacketWaitEntries = new PacketWaitEntries();

                vm.Organize( entry ).Wait();

                Item[] destinationItems = destinationContainer.Container.GetItems();

                Assert.AreEqual( 2, destinationContainer.Container.GetTotalItemCount() );
                Assert.AreEqual( amnt * 2, destinationItems.Sum( i => i.Count ) );

                Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
                Engine.Items.Clear();
                Engine.PacketWaitEntries = null;
            } );
        }
    }
}