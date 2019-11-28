using System;
using System.Threading;
using ClassicAssist.Data.Macros;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class MacroTests
    {
        [TestMethod]
        public void WillExecute()
        {
            MacroEntry me = new MacroEntry();

            MacroInvoker mi = new MacroInvoker( me );

            mi.Execute();
        }

        [TestMethod]
        public void WillAbortRunning()
        {
            MacroEntry me = new MacroEntry { Macro = "while true:\r\n\t" };

            MacroInvoker mi = new MacroInvoker( me );

            mi.Execute();

            mi.Stop();

            bool result = mi.Thread.Join( 5000 );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void WillRaiseStartedEvent()
        {
            MacroEntry me = new MacroEntry();

            MacroInvoker mi = new MacroInvoker( me );

            AutoResetEvent are = new AutoResetEvent( false );

            void OnStartedEvent()
            {
                are.Set();
            }

            mi.StartedEvent += OnStartedEvent;

            mi.Execute();

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void WillRaiseStoppedEvent()
        {
            MacroEntry me = new MacroEntry();

            MacroInvoker mi = new MacroInvoker( me );

            AutoResetEvent are = new AutoResetEvent( false );

            void OnStoppedEvent()
            {
                are.Set();
            }

            mi.StoppedEvent += OnStoppedEvent;

            mi.Execute();

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void WillExecuteRunDummy()
        {
            MacroEntry me = new MacroEntry() { Macro = "Dummy(5,7)"};

            MacroInvoker mi = new MacroInvoker(me);

            mi.Execute();

            mi.Thread.Join();

            Assert.IsFalse( mi.IsFaulted );
        }


        [TestMethod]
        public void WillExceptionEvent()
        {
            MacroEntry me = new MacroEntry { Macro = "kjdkdsdksdfsdk" };

            MacroInvoker mi = new MacroInvoker( me );

            AutoResetEvent are = new AutoResetEvent( false );

            void OnExceptionEvent( Exception e )
            {
                Assert.IsTrue( mi.IsFaulted );
                Assert.IsNotNull( mi.Exception );
                are.Set();
            }

            mi.ExceptionEvent += OnExceptionEvent;

            mi.Execute();

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );
        }
    }
}