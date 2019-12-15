using System;
using ClassicAssist.Data.Macros.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class TimerTests
    {
        [TestMethod]
        public void WillAddOffset()
        {
            TimerCommands.CreateTimer( "shmoo" );
            TimerCommands.SetTimer( "shmoo", (int) TimeSpan.FromMinutes( 2 ).TotalMilliseconds );

            long timerValue = TimerCommands.Timer( "shmoo" );

            TimeSpan timespan = TimeSpan.FromMilliseconds( timerValue );

            Assert.AreEqual( 2, (int) timespan.TotalMinutes );

            TimerCommands.RemoveTimer( "shmoo" );
        }

        [TestMethod]
        public void WillCreateTimer()
        {
            TimerCommands.CreateTimer( "shmoo2" );

            Assert.IsTrue( TimerCommands.TimerExists( "shmoo2" ) );

            TimerCommands.RemoveTimer( "shmoo2" );
        }

        [TestMethod]
        public void WillRemoveTimer()
        {
            TimerCommands.CreateTimer( "shmoo3" );
            TimerCommands.RemoveTimer( "shmoo3" );

            Assert.IsFalse( TimerCommands.TimerExists( "shmoo3" ) );
        }

        [TestMethod]
        public void WillCreateTimerSetTimer()
        {
            TimerCommands.SetTimer( "shmoo4" );
            Assert.IsTrue( TimerCommands.TimerExists( "shmoo4" ) );
        }
    }
}