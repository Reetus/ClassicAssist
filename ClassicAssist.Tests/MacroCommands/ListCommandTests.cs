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

        [TestCleanup]
        public void Cleanup()
        {
            RemoveList( "shmoo" );
        }
    }
}