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
using ClassicAssist.UO.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class TileDataFormatTest
    {
        private const string oldFormatPath = @"D:\Games\Ultima Online\";
        private static readonly string newFormatPath = @"C:\Program Files (x86)\Electronic Arts\Ultima Online Classic";

        [TestMethod]
        public void WillReadOldFormatStatic()
        {
            if ( !Directory.Exists( oldFormatPath ) )
            {
                return;
            }

            TileData.Initialize( oldFormatPath );

            StaticTile td = TileData.GetStaticTile( 0x1bf8 );

            Assert.AreEqual( "silver ingot", td.Name );
            Assert.IsTrue( td.Flags.HasFlag( TileFlags.Generic ) );
        }

        [TestMethod]
        public void WillReadNewFormatStatic()
        {
            if ( !Directory.Exists( newFormatPath ) )
            {
                return;
            }

            TileData.Initialize( newFormatPath );

            StaticTile td = TileData.GetStaticTile( 0x1bf8 );

            Assert.AreEqual( "silver ingot", td.Name );
            Assert.IsTrue( td.Flags.HasFlag( TileFlags.Generic ) );
        }
    }
}