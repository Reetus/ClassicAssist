using System;
using System.Collections.Generic;
using System.IO;
using ClassicAssist.Shared;
using ClassicAssist.Data.Dress;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Tests.Agents
{
    [TestClass]
    public class DressAgentTests
    {
        private Item _backpack;
        private int[] _packetLengths;
        private PlayerMobile _player;

        [TestInitialize]
        public void Initialize()
        {
            _packetLengths = new int[byte.MaxValue];

            string json = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, "packetlengths.json" ) );

            JObject jsonObj = JObject.Parse( json );

            foreach ( JProperty token in jsonObj["PacketLengths"].Children<JProperty>() )
            {
                int key = int.Parse( token.Name );
                int val = token.Value.ToObject<int>();

                if ( val == 0x8000 )
                {
                    val = 0;
                }

                _packetLengths[key] = val;
            }

            _player = new PlayerMobile( 0x01 ) { Equipment = new ItemCollection( 0x01 ) };
            _backpack = new Item( 0x40000002 ) { Container = new ItemCollection( 0x40000002 ), Owner = 0x01 };
            _player.SetLayer( Layer.Backpack, _backpack.Serial );

            Engine.Items.Add( _backpack );

            Engine.Player = _player;

            IncomingPacketHandlers.Initialize();
            OutgoingPacketHandlers.Initialize();
            Engine.ClientVersion = new Version( 7, 0, 45, 1 );

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;
        }

        private void OnInternalPacketSentEvent( byte[] data, int length )
        {
            PacketHandler handler = OutgoingPacketHandlers.GetHandler( data[0] );

            handler?.OnReceive( new PacketReader( data, data.Length, _packetLengths[data[0]] != 0 ) );

            if ( data[0] == 0x08 )
            {
                int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                int containerSerial = ( data[11] << 24 ) | ( data[12] << 16 ) | ( data[13] << 8 ) | data[14];

                //container cont update

                PacketWriter pw = new PacketWriter( 21 );
                pw.Write( (byte) 0x25 );
                pw.Write( serial );
                pw.Seek( 10, SeekOrigin.Current );
                pw.Write( containerSerial );
                pw.Write( (short) 0 );

                byte[] packet = pw.ToArray();

                handler = IncomingPacketHandlers.GetHandler( packet[0] );

                handler?.OnReceive( new PacketReader( packet, packet.Length, true ) );
            }

            if ( data[0] == 0x13 )
            {
                int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                Layer layer = (Layer) data[5];
                int containerSerial = ( data[6] << 24 ) | ( data[7] << 16 ) | ( data[8] << 8 ) | data[9];

                Mobile mobile = Engine.Mobiles.GetMobile( containerSerial );

                int itemInLayer = mobile?.GetLayer( layer ) ?? 0;

                if ( itemInLayer != 0 )
                {
                    // item already in layer / liftrej
                    return;
                }

                // equip update

                PacketWriter pw = new PacketWriter( 15 );
                pw.Write( (byte) 0x2E );
                pw.Write( serial );
                pw.Write( (short) 0 );
                pw.Write( (byte) 0 );
                pw.Write( (byte) layer );
                pw.Write( containerSerial );
                pw.Write( (short) 0 );

                byte[] packet = pw.ToArray();

                handler = IncomingPacketHandlers.GetHandler( packet[0] );

                handler?.OnReceive( new PacketReader( packet, packet.Length, true ) );
            }
        }

        [TestMethod]
        public void WillDressUndressItem()
        {
            Item dressItem = new Item( 0x40000003 );
            Engine.Items.Add( dressItem );

            DressAgentEntry dae = new DressAgentEntry
            {
                Name = "Test",
                Items = new List<DressAgentItem>
                {
                    new DressAgentItem { Layer = Layer.Talisman, Serial = dressItem.Serial }
                }
            };

            dae.Dress().Wait();

            Assert.AreEqual( dressItem.Serial, _player.GetLayer( dressItem.Layer ) );

            dae.Undress().Wait();

            Assert.AreEqual( 0, _player.GetLayer( dressItem.Layer ) );
            Assert.AreEqual( 1, _player.Backpack.Container.GetTotalItemCount() );
            Assert.IsNotNull( _player.Backpack.Container.GetItem( dressItem.Serial ) );
        }

        [TestMethod]
        public void WillDressUndressItemContainer()
        {
            Item dressItem = new Item( 0x40000003 );
            Item undressContainer = new Item( 0x40000004 ) { Container = new ItemCollection( 0x40000004 ) };

            Engine.Items.Add( dressItem );
            Engine.Items.Add( undressContainer );

            DressAgentEntry dae = new DressAgentEntry
            {
                Name = "Test",
                Items = new List<DressAgentItem>
                {
                    new DressAgentItem { Layer = Layer.Talisman, Serial = dressItem.Serial }
                },
                UndressContainer = undressContainer.Serial
            };

            dae.Dress().Wait();

            Assert.AreEqual( dressItem.Serial, _player.GetLayer( dressItem.Layer ) );

            dae.Undress().Wait();

            Assert.AreEqual( 0, _player.GetLayer( dressItem.Layer ) );
            Assert.AreEqual( 1, undressContainer.Container.GetTotalItemCount() );
            Assert.IsNotNull( undressContainer.Container.GetItem( dressItem.Serial ) );

            Engine.Items.Remove( undressContainer );
        }

        [TestMethod]
        public void WillUndressAll()
        {
            Item dressItem = new Item( 0x40000003 );
            Engine.Items.Add( dressItem );

            DressAgentEntry dae = new DressAgentEntry
            {
                Name = "Test",
                Items = new List<DressAgentItem>
                {
                    new DressAgentItem { Layer = Layer.Talisman, Serial = dressItem.Serial }
                }
            };

            dae.Dress().Wait();

            Assert.AreEqual( dressItem.Serial, _player.GetLayer( dressItem.Layer ) );

            DressManager.GetInstance().UndressAll().Wait();

            Assert.AreEqual( 0, _player.GetLayer( dressItem.Layer ) );
            Assert.AreEqual( 1, _player.Backpack.Container.GetTotalItemCount() );
            Assert.IsNotNull( _player.Backpack.Container.GetItem( dressItem.Serial ) );
        }

        [TestMethod]
        public void WontDressNoMoveConflicting()
        {
            Item dressItem = new Item( 0x40000003 );
            Engine.Items.Add( dressItem );

            Item conflictingItem = new Item( 0x40000004 ) { Owner = _player.Serial };
            Engine.Items.Add( conflictingItem );
            _player.SetLayer( Layer.Talisman, conflictingItem.Serial );

            DressAgentEntry dae = new DressAgentEntry
            {
                Name = "Test",
                Items = new List<DressAgentItem>
                {
                    new DressAgentItem { Layer = Layer.Talisman, Serial = dressItem.Serial }
                }
            };

            Assert.AreEqual( conflictingItem.Serial, _player.GetLayer( Layer.Talisman ) );

            dae.Dress( false ).Wait();

            Assert.AreEqual( conflictingItem.Serial, _player.GetLayer( Layer.Talisman ) );

            Engine.Items.Remove( conflictingItem );
        }

        [TestMethod]
        public void WillDressMoveConflicting()
        {
            Item dressItem = new Item( 0x40000003 );
            Engine.Items.Add( dressItem );

            Item conflictingItem = new Item( 0x40000004 ) { Owner = _player.Serial };
            Engine.Items.Add( conflictingItem );
            _player.SetLayer( Layer.Talisman, conflictingItem.Serial );

            DressAgentEntry dae = new DressAgentEntry
            {
                Name = "Test",
                Items = new List<DressAgentItem>
                {
                    new DressAgentItem { Layer = Layer.Talisman, Serial = dressItem.Serial }
                }
            };

            Assert.AreEqual( conflictingItem.Serial, _player.GetLayer( Layer.Talisman ) );

            dae.Dress().Wait();

            Assert.AreEqual( dressItem.Serial, _player.GetLayer( Layer.Talisman ) );

            Engine.Items.Remove( conflictingItem );
        }

        [TestCleanup]
        public void Cleanup()
        {
            ActionPacketQueue.Clear();
            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.Player = null;
            Engine.Items.Clear();
        }
    }
}