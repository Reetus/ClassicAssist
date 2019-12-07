using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Assistant;
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
            MacroEntry me = new MacroEntry { Macro = "Dummy(5,7)" };

            MacroInvoker mi = new MacroInvoker( me );

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

        [TestMethod]
        public void EnsureAllCommandsHaveAttribute()
        {
            IEnumerable<Type> types = Assembly.GetAssembly( typeof( Engine ) ).GetTypes().Where( t =>
                t.Namespace != null && t.Namespace.EndsWith( "Macros.Commands" ) && t.IsClass && t.IsPublic && !t.Name.Contains( "Dummy" ) );

            foreach ( Type type in types )
            {
                MethodInfo[] members = type.GetMethods( BindingFlags.Public | BindingFlags.Static );

                foreach ( MethodInfo member in members )
                {
                    CommandsDisplayAttribute attr = member.GetCustomAttribute<CommandsDisplayAttribute>();

                    if ( attr == null )
                    {
                        Assert.Fail( $"{type.Name}.{member.Name} has no CommandsDisplayAttribute." );
                    }
                }
            }
        }
    }
}