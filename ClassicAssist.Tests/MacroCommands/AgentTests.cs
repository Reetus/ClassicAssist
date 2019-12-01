using Assistant;
using ClassicAssist.Data.Counters;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
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
    }
}