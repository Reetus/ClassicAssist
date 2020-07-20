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
using ClassicAssist.Shared;
using ClassicAssist.Data.Scavenger;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Agents
{
    [TestClass]
    public class ScavengerTests
    {
        [TestMethod]
        public void WillScavengerEnabled()
        {
            //TODO
            //ScavengerTabViewModel vm = new ScavengerTabViewModel { Enabled = true };
            //ScavengerManager manager = ScavengerManager.GetInstance();

            //if ( !Directory.Exists( @"C:\Program Files (x86)\Electronic Arts\Ultima Online Classic" ) )
            //{
            //    return;
            //}

            //TileData.Initialize( @"C:\Program Files (x86)\Electronic Arts\Ultima Online Classic" );
            //Engine.Player = new PlayerMobile( 0x0 ) { WeightMax = 500 };
            //Engine.Player.SetLayer( Layer.Backpack, 0x40000001 );
            //manager.Items.Add( new ScavengerEntry { Enabled = true, Graphic = 0xff, Hue = -1, Name = "Test" } );

            //Engine.Items.Add( new Item( 0x40000001 ) { Container = new ItemCollection( 0x4000001 ) } );
            //Engine.Items.Add( new Item( 0x40000000 ) { ID = 0xff } );

            //AutoResetEvent are = new AutoResetEvent( false );

            //void OnInternalPacketSentEvent( byte[] data, int length )
            //{
            //    if ( data[0] == 0x07 )
            //    {
            //        are.Set();
            //    }
            //}

            //Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            //vm.CheckArea();

            //bool result = are.WaitOne( 5000 );

            //if ( !result )
            //{
            //    Assert.Fail();
            //}

            //Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            //Engine.Player = null;
            //Engine.Items.Clear();
            //manager.Items.Clear();
        }

        [TestMethod]
        public void WontScavengerEntryDisabled()
        {
            //TODO
            //ScavengerTabViewModel vm = new ScavengerTabViewModel { Enabled = true };
            ScavengerManager manager = ScavengerManager.GetInstance();

            if ( !Directory.Exists( @"C:\Program Files (x86)\Electronic Arts\Ultima Online Classic" ) )
            {
                return;
            }

            TileData.Initialize( @"C:\Program Files (x86)\Electronic Arts\Ultima Online Classic" );
            Engine.Player = new PlayerMobile( 0x0 ) { WeightMax = 500 };
            Engine.Player.SetLayer( Layer.Backpack, 0x40000001 );
            manager.Items.Add( new ScavengerEntry { Enabled = false, Graphic = 0xff, Hue = -1, Name = "Test" } );

            Engine.Items.Add( new Item( 0x40000001 ) { Container = new ItemCollection( 0x4000001 ) } );
            Engine.Items.Add( new Item( 0x40000000 ) { ID = 0xff } );

            AutoResetEvent are = new AutoResetEvent( false );

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] == 0x07 )
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            //TODO
            //vm.CheckArea();

            bool result = are.WaitOne( 500 );

            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.Player = null;
            Engine.Items.Clear();
            manager.Items.Clear();
        }
    }
}