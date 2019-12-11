using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using ClassicAssist.Data.Macros.Commands;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace ClassicAssist.Data.Macros
{
    public class MacroInvoker
    {
        public delegate void dMacroException( Exception e );

        public delegate void dMacroStartStop();

        private readonly ScriptEngine _engine;
        private readonly MacroEntry _macro;

        public MacroInvoker( MacroEntry macro )
        {
            _macro = macro;
            _engine = Python.CreateEngine();

            ScriptRuntime runtime = _engine.Runtime;
            runtime.LoadAssembly( Assembly.GetExecutingAssembly() );
        }

        public Exception Exception { get; set; }
        public bool IsFaulted { get; set; }

        public bool IsRunning => Thread.IsAlive;

        public Thread Thread { get; set; }

        public event dMacroStartStop StartedEvent;
        public event dMacroStartStop StoppedEvent;
        public event dMacroException ExceptionEvent;

        private static string GetScriptingImports()
        {
            string prepend = Assembly.GetExecutingAssembly().GetTypes()
                .Where( t =>
                    t.Namespace != null && t.IsPublic && t.IsClass && t.Namespace.EndsWith( "Macros.Commands" ) )
                .Aggregate( string.Empty, ( current, t ) => current + $"from {t.FullName} import * \n" );

            return prepend;
        }

        public void Execute()
        {
            string code = $"{GetScriptingImports()}\r\n{_macro.Macro}";

            ScriptSource source = _engine.CreateScriptSourceFromString( code, SourceCodeKind.Statements );

            Thread = new Thread( () =>
            {
                Thread = Thread.CurrentThread;

                try
                {
                    StartedEvent?.Invoke();

                    AliasCommands.SetDefaultAliases();

                    source.Execute();
                }
                catch ( ThreadAbortException )
                {
                    IsFaulted = true;
                }
                catch ( Exception e )
                {
                    IsFaulted = true;
                    Exception = e;

                    ExceptionEvent?.Invoke( e );
                }
                finally
                {
                    StoppedEvent?.Invoke();
                }
            } ) { IsBackground = true };

            Thread.Start();
        }

        public void Stop()
        {
            Thread?.Abort();
        }

        public void ExecuteSync()
        {
            string code = $"{GetScriptingImports()}\r\n{_macro.Macro}";

            ScriptSource source = _engine.CreateScriptSourceFromString( code, SourceCodeKind.Statements );

            try
            {
                StartedEvent?.Invoke();

                AliasCommands.SetDefaultAliases();

                source.Execute();
            }
            catch ( ThreadAbortException )
            {
                IsFaulted = true;
            }
            catch ( Exception e )
            {
                IsFaulted = true;
                Exception = e;

                ExceptionEvent?.Invoke( e );
            }
            finally
            {
                StoppedEvent?.Invoke();
            }
        }
    }
}