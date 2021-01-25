using System.Threading;
using Assistant;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class CommandsTests
    {
        [TestMethod]
        public void ResendTargetToClientWillSendCorrectTargetType()
        {
            TargetType expected = TargetType.Object;
            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketReceivedEvent( byte[] data, int length )
            {
                TargetType type = (TargetType) data[1];

                // ReSharper disable once AccessToModifiedClosure
                if ( type != expected )
                {
                    Assert.Fail( "Incorrect TargetType" );
                }

                are.Set();
            }

            Engine.InternalPacketReceivedEvent += OnPacketReceivedEvent;

            Engine.TargetType = expected = TargetType.Object;

            Commands.ResendTargetToClient();

            are.WaitOne( 5000 );

            Engine.TargetType = expected = TargetType.Tile;

            Commands.ResendTargetToClient();

            are.WaitOne( 5000 );

            Engine.InternalPacketReceivedEvent -= OnPacketReceivedEvent;
        }
    }
}