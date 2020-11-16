using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ClassicAssist.Data.Macros.Commands.ListCommands;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class ListCommandTests
    {
        [TestInitialize]
        public void Initialize()
        {
            CreateList( "shmoo" );
            PushList( "shmoo", 0 );
            PushList( "shmoo", 1 );
        }

        [TestMethod]
        public void ListExistsWillReturnTrue()
        {
            Assert.IsTrue( ListExists( "shmoo" ) );
        }

        [TestMethod]
        public void GetListWillReturnArray()
        {
            Assert.IsTrue( GetList( "shmoo" ) is int[] _ );
        }

        [TestMethod]
        public void ListCountWillReturnCorrect()
        {
            Assert.AreEqual( 2, List( "shmoo" ) );
        }

        [TestMethod]
        public void WillPopListFront()
        {
            CreateList( "poplist" );
            PushList( "poplist", 0 );
            PushList( "poplist", 1 );

            PopList( "poplist", "front" );

            int[] arr = GetList( "poplist" );

            Assert.AreEqual( 1, arr.Length );
            Assert.AreEqual( 1, arr[0] );
        }

        [TestMethod]
        public void WillPopListBack()
        {
            CreateList( "poplist" );
            PushList( "poplist", 0 );
            PushList( "poplist", 1 );

            PopList( "poplist" );

            int[] arr = GetList( "poplist" );

            Assert.AreEqual( 1, arr.Length );
            Assert.AreEqual( 0, arr[0] );
        }

        [TestMethod]
        public void WillPopListValue()
        {
            CreateList( "poplist" );
            PushList( "poplist", 0 );
            PushList( "poplist", 0 );
            PushList( "poplist", 0 );
            PushList( "poplist", 0 );
            PushList( "poplist", 1 );

            PopList( "poplist", "0" );

            int[] arr = GetList( "poplist" );

            Assert.AreEqual( 1, arr.Length );
            Assert.AreEqual( 1, arr[0] );
        }

        [TestCleanup]
        public void Cleanup()
        {
            RemoveList( "shmoo" );
        }
    }
}