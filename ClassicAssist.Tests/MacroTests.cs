using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assistant;
using ClassicAssist.Data.Macros;
using ClassicAssist.Resources;
using ClassicAssist.UI.ViewModels;
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

            MacroManager mi = MacroManager.GetInstance();

            mi.Execute( me );
        }

        [TestMethod]
        public void WontThrowExceptionNewMacroNoAssistantOptions()
        {
            try
            {
                string path = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Assistant.json" );

                if ( File.Exists( path ) )
                {
                    File.Delete( path );
                }

                MacrosTabViewModel vm = new MacrosTabViewModel();

                vm.NewMacroCommand.Execute( null );
            }
            catch ( Exception )
            {
                Assert.Fail();
            }
        }

        //[TestMethod]
        //public void WillAbortRunning()
        //{
        //    MacroEntry me = new MacroEntry { Macro = "while true:\r\n\t" };

        //    MacroManager mi = MacroManager.GetInstance();

        //    mi.Execute( me );

        //    mi.Stop();

        //    bool result = mi.CurrentMacro.MacroInvoker.Thread.Join( 5000 );

        //    Assert.IsTrue( result );
        //}

        //[TestMethod]
        //public void WillRaiseStartedEvent()
        //{
        //    MacroEntry me = new MacroEntry();

        //    MacroManager mi = MacroManager.GetInstance();

        //    AutoResetEvent are = new AutoResetEvent( false );

        //    void OnStartedEvent()
        //    {
        //        are.Set();
        //    }

        //    mi.CurrentMacro.MacroInvoker.StartedEvent += OnStartedEvent;

        //    mi.Execute( me );

        //    bool result = are.WaitOne( 5000 );

        //    Assert.IsTrue( result );
        //}

        //[TestMethod]
        //public void WillRaiseStoppedEvent()
        //{
        //    MacroEntry me = new MacroEntry();

        //    MacroManager mi = MacroManager.GetInstance();

        //    AutoResetEvent are = new AutoResetEvent( false );

        //    void OnStoppedEvent()
        //    {
        //        are.Set();
        //    }

        //    mi.CurrentMacro.MacroInvoker.StoppedEvent += OnStoppedEvent;

        //    mi.Execute( me );

        //    bool result = are.WaitOne( 5000 );

        //    Assert.IsTrue( result );
        //}

        //[TestMethod]
        //public void WillExecuteRunDummy()
        //{
        //    MacroEntry me = new MacroEntry { Macro = "Dummy(5,7)" };

        //    MacroManager mi = MacroManager.GetInstance();

        //    mi.Execute( me );

        //    mi.CurrentMacro.MacroInvoker.Thread.Join();

        //    Assert.IsFalse( mi.CurrentMacro.MacroInvoker.IsFaulted );
        //}

        //[TestMethod]
        //public void WillExceptionEvent()
        //{
        //    MacroEntry me = new MacroEntry { Macro = "kjdkdsdksdfsdk" };

        //    MacroManager mi = MacroManager.GetInstance();

        //    AutoResetEvent are = new AutoResetEvent( false );

        //    void OnExceptionEvent( Exception e )
        //    {
        //        Assert.IsTrue( mi.CurrentMacro.MacroInvoker.IsFaulted );
        //        Assert.IsNotNull( mi.CurrentMacro.MacroInvoker.Exception );
        //        are.Set();
        //    }

        //    mi.CurrentMacro.MacroInvoker.ExceptionEvent += OnExceptionEvent;

        //    mi.Execute( me );

        //    bool result = are.WaitOne( 5000 );

        //    Assert.IsTrue( result );
        //}

        [TestMethod]
        public void EnsureAllCommandsHaveAttribute()
        {
            IEnumerable<Type> types = Assembly.GetAssembly( typeof( Engine ) ).GetTypes().Where( t =>
                t.Namespace != null && t.Namespace.EndsWith( "Macros.Commands" ) && t.IsClass && t.IsPublic &&
                !t.Name.Contains( "Dummy" ) );

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

        [TestMethod]
        public void EnsureAllCategoriesAreLocalizable()
        {
            IEnumerable<Type> types = Assembly.GetAssembly( typeof( Engine ) ).GetTypes().Where( t =>
                t.Namespace != null && t.Namespace.EndsWith( "Macros.Commands" ) && t.IsClass && t.IsPublic &&
                !t.Name.Contains( "Dummy" ) );

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

                    string resourceName = Strings.ResourceManager.GetString( attr.Category );

                    if ( string.IsNullOrEmpty( resourceName ) )
                    {
                        Assert.Fail(
                            $"{type.Name}.{member.Name}: Category \"{attr.Category}\" has no resource entry." );
                    }
                }
            }
        }
    }
}