using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class TargetCommandTests
    {
        [TestMethod]
        public void WillTargetTileRelativeSelf()
        {
            Engine.Player = new PlayerMobile( 0x01 ) { X = 100, Y = 100, Direction = Direction.East };

            AutoResetEvent are = new AutoResetEvent( false );

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] != 0x6C )
                {
                    Assert.Fail();
                }

                int x = ( data[11] << 8 ) | data[12];
                //int y = ( data[13] << 8 ) | data[14];

                if ( x != 101 )
                {
                    Assert.Fail();
                }

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            AliasCommands.SetAlias( "self", 0x01 );

            TargetCommands.TargetTileRelative( "self", 1 );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillTargetTileRelativeSelfRunning()
        {
            Engine.Player = new PlayerMobile( 0x01 )
            {
                X = 100, Y = 100, Direction = (Direction) ( (int) Direction.East | 0x80 )
            };

            AutoResetEvent are = new AutoResetEvent( false );

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] != 0x6C )
                {
                    Assert.Fail();
                }

                int x = ( data[11] << 8 ) | data[12];
                //int y = ( data[ 13 ] << 8 ) | data[ 14 ];

                if ( x != 101 )
                {
                    Assert.Fail();
                }

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            AliasCommands.SetAlias( "self", 0x01 );

            TargetCommands.TargetTileRelative( "self", 1 );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillSetLastTargetSerialOnInternalTargetSent()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            Assert.AreEqual( 0, Engine.Player.LastTargetSerial );

            TargetCommands.Target( 0x00aabbcc );

            Assert.AreEqual( 0x00aabbcc, Engine.Player.LastTargetSerial );

            Engine.Player = null;
        }

        [TestMethod]
        public void WillKeepLastBetweenCasts()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnWaitEntryAddedEvent( PacketWaitEntry entry )
            {
                PacketWriter target = new PacketWriter( 0x19 );
                target.Write( (byte) 0x6C );
                target.Fill();

                Engine.SendPacketToClient( target );
                Engine.PacketWaitEntries.CheckWait( target.ToArray(), PacketDirection.Incoming );
            }

            Engine.PacketWaitEntries.WaitEntryAddedEvent += OnWaitEntryAddedEvent;

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] != 0x6C )
                {
                    return;
                }

                int serial = ( data[7] << 24 ) | ( data[8] << 16 ) | ( data[9] << 8 ) | data[10];

                if ( serial != 0x00aabbcc )
                {
                    Assert.Fail();
                }

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            Task.Run( () => SpellCommands.Cast( "Explosion", 0x00aabbcc ) );

            bool result = are.WaitOne( 60000 );

            Assert.AreEqual( 0x00aabbcc, AliasCommands.GetAlias( "last" ) );

            Assert.IsTrue( result );

            Task.Run( () => SpellCommands.Cast( "Explosion", "last" ) );

            result = are.WaitOne( 60000 );

            Assert.AreEqual( 0x00aabbcc, AliasCommands.GetAlias( "last" ) );

            Assert.IsTrue( result );

            Assert.AreEqual( 0x00aabbcc, Engine.Player.LastTargetSerial );

            Engine.PacketWaitEntries.WaitEntryAddedEvent -= OnWaitEntryAddedEvent;
            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.Player = null;
        }
    }
}