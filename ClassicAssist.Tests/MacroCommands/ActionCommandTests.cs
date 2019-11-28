using System.Threading;
using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class ActionCommandTests
    {
        [TestMethod]
        public void AttackWillSendAttackPacket()
        {
            AutoResetEvent are = new AutoResetEvent( false );

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] != 0x05 )
                {
                    return;
                }

                int serial = data[1] << 24 | data[2] << 16 | data[3] << 8 | data[4];

                Assert.AreEqual( 0x00aabbcc, serial );

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            ActionCommands.Attack( 0x00aabbcc );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
        }

        [TestMethod]
        public void ClearHandsWillSendDragPacket()
        {
            AutoResetEvent are = new AutoResetEvent(false);

            void OnInternalPacketSentEvent(byte[] data, int length)
            {
                if (data[0] != 0x07)
                {
                    return;
                }

                int serial = data[1] << 24 | data[2] << 16 | data[3] << 8 | data[4];

                Assert.AreEqual(0x00aabbcc, serial);

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            Engine.Player = new PlayerMobile( 0x01 );
            Engine.Player.SetLayer( Layer.OneHanded, 0x00aabbcc );

            ActionCommands.ClearHands( "left" );

            bool result = are.WaitOne(5000);

            Assert.IsTrue(result);

            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void ClickObjectWillSendPacket()
        {
            AutoResetEvent are = new AutoResetEvent(false);

            void OnInternalPacketSentEvent(byte[] data, int length)
            {
                if (data[0] != 0x09)
                {
                    return;
                }

                int serial = data[1] << 24 | data[2] << 16 | data[3] << 8 | data[4];

                Assert.AreEqual(0x00aabbcc, serial);

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            AliasCommands.SetAlias( "thing", 0x00aabbcc);
            ActionCommands.ClickObject( "thing" );

            bool result = are.WaitOne(5000);

            Assert.IsTrue(result);

            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void MoveItemWillSendDragPacket()
        {
            AutoResetEvent are = new AutoResetEvent(false);

            void OnInternalPacketSentEvent(byte[] data, int length)
            {
                if (data[0] != 0x07)
                {
                    return;
                }

                int serial = data[1] << 24 | data[2] << 16 | data[3] << 8 | data[4];

                Assert.AreEqual(0x00aabbcc, serial);

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            Engine.Items.Add( new Item(0x00aabbcc) { Count = 50} );

            ActionCommands.MoveItem( 0x00aabbcc, 0xaabbdd );

            bool result = are.WaitOne(5000);

            Assert.IsTrue(result);

            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
        }
    }
}