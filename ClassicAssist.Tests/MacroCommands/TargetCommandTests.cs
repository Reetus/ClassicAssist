using System.Threading;
using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
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
    }
}