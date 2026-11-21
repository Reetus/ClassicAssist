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

using System.Linq;
using ClassicAssist.UI.Controls;
using ClassicAssist.UI.ViewModels.Debug;
using ClassicAssist.UO.Network.PacketFilter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class PacketMatchExpressionTests
    {
        // 0x25 add to container: cmd, serial 0x4001ABCD, ...
        private static readonly byte[] _addToContainer = { 0x25, 0x40, 0x01, 0xAB, 0xCD, 0x0E, 0xED };

        // 0xF3 world item: cmd, 0x0001, type, serial 0x4001ABCD, graphic 0x0EED (gold), ...
        private static readonly byte[] _worldItem = { 0xF3, 0x00, 0x01, 0x00, 0x40, 0x01, 0xAB, 0xCD, 0x0E, 0xED, 0x00, 0x01, 0xF4, 0x01, 0xF4 };

        private static bool Matches( string filter, byte[] data )
        {
            return PacketMatchExpression.Parse( filter ).IsMatch( data );
        }

        [DataTestMethod]
        [DataRow( "" )]
        [DataRow( "   " )]
        [DataRow( null )]
        public void EmptyFilterMatchesEverything( string filter )
        {
            Assert.IsTrue( Matches( filter, _worldItem ) );
        }

        [DataTestMethod]
        [DataRow( "[0] = 0xF3", true )]
        [DataRow( "[0]=0xf3", true )]
        [DataRow( "[0] == 243", true )]
        [DataRow( "[0] = 0x25", false )]
        [DataRow( "[4..7] = 0x4001ABCD", true )]
        [DataRow( "[4..7] = 0x4001ABCE", false )]
        [DataRow( "[0] = 0xF3 && [4..7] = 0x4001abcd", true )]
        [DataRow( "[0] = 0xF3 and [8..9] = 0x0EED", true )]
        [DataRow( "[0] = 0xF3 && [8..9] = 0x0EEE", false )]
        [DataRow( "[5] = !0x01", false )]
        [DataRow( "[5]=!0xAA", true )]
        [DataRow( "[5] != 0xAA", true )]
        [DataRow( "[5] != 0x01", false )]
        [DataRow( "[0] = 0x25 || [0] = 0xF3", true )]
        [DataRow( "[0] = 0x25 or [0] = 0x1A", false )]
        [DataRow( "![0] = 0x73", true )]
        [DataRow( "not [0] = 0xF3", false )]
        [DataRow( "!([0] = 0x25 || [0] = 0xF3)", false )]
        [DataRow( "[4..7] > 0x40000000", true )]
        [DataRow( "[4..7] < 0x40000000", false )]
        [DataRow( "[1..2] >= 1 && [1..2] <= 1", true )]
        [DataRow( "len = 15", true )]
        [DataRow( "LEN > 20", false )]
        [DataRow( "contains 0x4001ABCD", true )]
        [DataRow( "contains 0x4001ABCE", false )]
        [DataRow( "contains 0x0EED", true )]
        [DataRow( "contains 0xF4", true )]
        public void MatchesWorldItem( string filter, bool expected )
        {
            Assert.AreEqual( expected, Matches( filter, _worldItem ), filter );
        }

        [TestMethod]
        public void AndBindsTighterThanOr()
        {
            // [0]=0xF3 || ( [0]=0x25 && [1]=0x01 ) is true for the world item; read left to right,
            // ( [0]=0xF3 || [0]=0x25 ) && [1]=0x01, it would be false.
            Assert.IsTrue( Matches( "[0] = 0xF3 || [0] = 0x25 && [1] = 0x01", _worldItem ) );
        }

        [TestMethod]
        public void FindsOneSerialAcrossPacketTypes()
        {
            // The serial sits at a different offset in each packet; contains finds it in both.
            Assert.IsTrue( Matches( "contains 0x4001ABCD", _addToContainer ) );
            Assert.IsTrue( Matches( "contains 0x4001ABCD", _worldItem ) );
            Assert.IsTrue( Matches( "[0] = 0x25 && [1..4] = 0x4001ABCD", _addToContainer ) );
        }

        [TestMethod]
        public void ByteTermNeverMatchesTooShortPacket()
        {
            byte[] shortPacket = { 0xF3, 0x00 };

            Assert.IsFalse( Matches( "[5] = 0xAA", shortPacket ) );
            Assert.IsFalse( Matches( "[5] != 0xAA", shortPacket ) );
            Assert.IsTrue( Matches( "!([5] = 0xAA)", shortPacket ) );
            Assert.IsFalse( Matches( "[1..2] = 0", shortPacket ) );
        }

        [TestMethod]
        public void HexContainsKeepsWrittenWidth()
        {
            byte[] data = { 0x01, 0x34 };

            Assert.IsTrue( Matches( "contains 0x34", data ) );
            Assert.IsFalse( Matches( "contains 0x0034", data ) );
            Assert.IsTrue( Matches( "contains 0x134", data ) );
        }

        [TestMethod]
        public void DecimalContainsUsesFewestBytes()
        {
            byte[] data = { 0x00, 0x01, 0x00 };

            Assert.IsTrue( Matches( "contains 256", data ) );
            Assert.IsFalse( Matches( "contains 65536", data ) );
        }

        [TestMethod]
        public void ContainsAcceptsLongSequences()
        {
            Assert.IsTrue( Matches( "contains 0x004001ABCD0EED0001F401F4", _worldItem ) );
        }

        [TestMethod]
        public void EightByteFieldCompares()
        {
            byte[] data = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE };

            Assert.IsTrue( Matches( "[0..7] = 0xFFFFFFFFFFFFFFFE", data ) );
        }

        [DataTestMethod]
        [DataRow( "[0]", "Expected '='" )]
        [DataRow( "[0] = ", "Expected a value" )]
        [DataRow( "[0 = 1", "Expected ']'" )]
        [DataRow( "[0] = 0x1FF", "does not fit in 1 byte" )]
        [DataRow( "[1..2] = 0x10000", "does not fit in 2 bytes" )]
        [DataRow( "[4..1] = 1", "ends before it starts" )]
        [DataRow( "[0..8] = 1", "at most 8" )]
        [DataRow( "[0] = 0x", "Expected hex digits" )]
        [DataRow( "[0] = 0xZZ", "Expected hex digits" )]
        [DataRow( "[0] = 12ab", "Invalid number" )]
        [DataRow( "[0] = 1 &&", "Expected '['" )]
        [DataRow( "[0] = 1 [1] = 2", "Expected '&&' or '||'" )]
        [DataRow( "([0] = 1", "Expected ')'" )]
        [DataRow( "foo = 1", "Unknown keyword 'foo'" )]
        [DataRow( "[0] = 1 & [1] = 2", "Unexpected '&'" )]
        [DataRow( "[0] = 0x10000000000000000", "larger than 8 bytes" )]
        [DataRow( "[70000] = 1", "past the largest packet size" )]
        public void RejectsInvalidFilter( string filter, string message )
        {
            Assert.IsFalse( PacketMatchExpression.TryParse( filter, out PacketMatchExpression expression, out string error ) );
            Assert.IsNull( expression );
            StringAssert.Contains( error, message );
        }

        [TestMethod]
        public void ErrorsReportColumn()
        {
            PacketMatchExpression.TryParse( "[0] = 0xF3 && [1] ? 2", out _, out string error );

            Assert.AreEqual( "Unexpected '?' at column 19", error );
        }

        [TestMethod]
        public void LeadingZerosDoNotCountTowardsWidth()
        {
            Assert.IsTrue( Matches( "[0] = 0x00F3", _worldItem ) );
        }
    }

    /// <summary>
    ///     The Debug window's filter is a view over the capture: it hides packets, never drops them, so
    ///     changing or clearing it shows what was captured before.
    /// </summary>
    [TestClass]
    public class DebugPacketsViewModelFilterTests
    {
        private DebugPacketsViewModel _viewModel;

        [TestInitialize]
        public void Initialize()
        {
            _viewModel = new DebugPacketsViewModel();
        }

        private void Capture( params byte[] data )
        {
            _viewModel.AddCapturedEntries( new[] { new PacketEntry { Data = data, Direction = PacketDirection.Incoming } } );
        }

        private byte[] Commands()
        {
            return _viewModel.Items.Select( e => e.Data[0] ).ToArray();
        }

        [TestMethod]
        public void FilterAppliesToNewAndExistingPackets()
        {
            Capture( 0x1A, 0x01 );
            Capture( 0x25, 0x02 );

            _viewModel.Filter = "[0] = 0x25 || [0] = 0x1D";

            CollectionAssert.AreEqual( new byte[] { 0x25 }, Commands() );

            Capture( 0x1D, 0x03 );
            Capture( 0x73, 0x04 );

            CollectionAssert.AreEqual( new byte[] { 0x25, 0x1D }, Commands() );

            _viewModel.Filter = string.Empty;

            CollectionAssert.AreEqual( new byte[] { 0x1A, 0x25, 0x1D, 0x73 }, Commands() );
            Assert.IsNull( _viewModel.FilterError );
        }

        [TestMethod]
        public void InvalidFilterKeepsLastValidOneAndReportsError()
        {
            Capture( 0x1A );
            Capture( 0x25 );

            _viewModel.Filter = "[0] = 0x25";
            _viewModel.Filter = "[0] = 0x";

            Assert.IsNotNull( _viewModel.FilterError );
            CollectionAssert.AreEqual( new byte[] { 0x25 }, Commands() );

            _viewModel.Filter = "[0] = 0x1A";

            Assert.IsNull( _viewModel.FilterError );
            CollectionAssert.AreEqual( new byte[] { 0x1A }, Commands() );
        }

        [TestMethod]
        public void ClearDropsHiddenPacketsToo()
        {
            Capture( 0x1A );
            _viewModel.Filter = "[0] = 0x25";

            _viewModel.ClearCommand.Execute( null );
            _viewModel.Filter = string.Empty;

            Assert.AreEqual( 0, _viewModel.Items.Count );
        }
    }
}
