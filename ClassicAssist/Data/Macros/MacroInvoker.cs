using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

        private static readonly ScriptEngine _engine = Python.CreateEngine();
        private static Dictionary<string, object> _importCache;
        private readonly MacroEntry _macro;

        public MacroInvoker( MacroEntry macro )
        {
            _macro = macro;

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

        public static Dictionary<string, object> InitializeImports( ScriptEngine engine )
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            ScriptSource importSource =
                engine.CreateScriptSourceFromString( GetScriptingImports(), SourceCodeKind.Statements );

            CompiledCode importCompiled = importSource.Compile();
            ScriptScope importScope = engine.CreateScope( dictionary );
            importCompiled.Execute( importScope );

            return dictionary;
        }

        public void Execute()
        {
            if ( _importCache == null )
            {
                _importCache = InitializeImports( _engine );
            }

            ScriptSource source = _engine.CreateScriptSourceFromString( _macro.Macro, SourceCodeKind.Statements );

            Dictionary<string, object> importCache = new Dictionary<string, object>( _importCache );

            Thread = new Thread( () =>
            {
                Thread = Thread.CurrentThread;

                try
                {
                    StartedEvent?.Invoke();

                    AliasCommands.SetDefaultAliases();

                    ScriptScope macroScope = _engine.CreateScope( importCache );

                    source.Execute( macroScope );
                }
                catch ( TaskCanceledException )
                {
                    IsFaulted = true;
                }
                catch ( ThreadInterruptedException )
                {
                    IsFaulted = true;
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
            if ( Thread == null || !Thread.IsAlive )
            {
                return;
            }

            Thread?.Interrupt();
            Thread?.Abort();
        }

        public void ExecuteSync( bool loop, CancellationToken token )
        {
            if ( _importCache == null )
            {
                _importCache = InitializeImports( _engine );
            }

            ScriptSource source = _engine.CreateScriptSourceFromString( _macro.Macro, SourceCodeKind.Statements );

            Dictionary<string, object> importCache = new Dictionary<string, object>( _importCache );

            try
            {
                StartedEvent?.Invoke();

                AliasCommands.SetDefaultAliases();

                ScriptScope macroScope = _engine.CreateScope( importCache );

                Stopwatch sw = new Stopwatch();

                do
                {
                    sw.Start();

                    source.Execute( macroScope );

                    sw.Stop();

                    token.ThrowIfCancellationRequested();

                    if ( sw.ElapsedMilliseconds < 50 )
                    {
                        int diff = 50 - (int) sw.ElapsedMilliseconds;
                        Thread.Sleep( diff > 0 ? diff : 1 );
                    }

                    if ( Options.CurrentOptions.Debug )
                    {
                        UO.Commands.SystemMessage( sw.ElapsedMilliseconds.ToString() );
                    }

                    sw.Reset();
                }
                while ( loop && !IsFaulted );
            }
            catch ( TaskCanceledException )
            {
                IsFaulted = true;
            }
            catch ( ThreadInterruptedException )
            {
                IsFaulted = true;
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