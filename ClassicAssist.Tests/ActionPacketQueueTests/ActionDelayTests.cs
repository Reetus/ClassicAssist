#region License

// Copyright (C) 2026 Reetus
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

using ClassicAssist.Data;
using ClassicAssist.UI.ViewModels.Agents;
using ClassicAssist.UO.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Tests.ActionPacketQueueTests
{
    [TestClass]
    public class ActionDelayTests
    {
        [TestMethod]
        public void WillUseGlobalActionDelayWhenNoOverride()
        {
            int previous = Options.CurrentOptions.ActionDelayMS;

            try
            {
                Options.CurrentOptions.ActionDelayMS = 750;

                ActionQueueItem queueItem = new ActionQueueItem( o => true );

                Assert.AreEqual( 750, ActionPacketQueue.GetActionDelay( queueItem ) );
            }
            finally
            {
                Options.CurrentOptions.ActionDelayMS = previous;
            }
        }

        [TestMethod]
        public void WillUseItemActionDelayOverride()
        {
            int previous = Options.CurrentOptions.ActionDelayMS;

            try
            {
                Options.CurrentOptions.ActionDelayMS = 750;

                ActionQueueItem queueItem = new ActionQueueItem( o => true ) { DelayMS = 1500 };

                Assert.AreEqual( 1500, ActionPacketQueue.GetActionDelay( queueItem ) );
            }
            finally
            {
                Options.CurrentOptions.ActionDelayMS = previous;
            }
        }

        [TestMethod]
        public void WillDefaultAutolootActionDelayToGeneralActionDelay()
        {
            AutolootViewModel vm = new AutolootViewModel();

            JObject json = new JObject { { "General", new JObject { { "ActionDelayMS", 650 } } }, { "Autoloot", new JObject() } };

            vm.Deserialize( json, new Options() );

            Assert.AreEqual( 650, vm.ActionDelayMS );
        }

        [TestMethod]
        public void WillDefaultAutolootActionDelayTo1000WhenNoConfig()
        {
            AutolootViewModel vm = new AutolootViewModel { ActionDelayMS = 5 };

            vm.Deserialize( new JObject(), new Options() );

            Assert.AreEqual( 1000, vm.ActionDelayMS );
        }

        [TestMethod]
        public void WillRoundTripAutolootActionDelay()
        {
            AutolootViewModel vm = new AutolootViewModel { ActionDelayMS = 2500 };

            JObject json = new JObject { { "General", new JObject { { "ActionDelayMS", 650 } } } };

            vm.Serialize( json, false );

            AutolootViewModel loaded = new AutolootViewModel();

            loaded.Deserialize( json, new Options() );

            Assert.AreEqual( 2500, loaded.ActionDelayMS );
        }
    }
}
