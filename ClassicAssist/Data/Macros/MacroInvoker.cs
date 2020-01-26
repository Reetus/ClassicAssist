using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Resources;
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
        private static MacroInvoker _instance;
        private static readonly object _lock = new object();
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private CancellationTokenSource _cancellationToken;
        private MacroEntry _macro;

        private MacroInvoker()
        {
            ScriptRuntime runtime = _engine.Runtime;
            runtime.LoadAssembly( Assembly.GetExecutingAssembly() );

            if ( _importCache == null )
            {
                _importCache = InitializeImports( _engine );
            }
        }

        public Exception Exception { get; set; }
        public bool IsFaulted { get; set; }

        public bool IsRunning => Thread.IsAlive;

        public Thread Thread { get; set; }

        public static MacroInvoker GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new MacroInvoker();
                    return _instance;
                }
            }

            return _instance;
        }

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

        public void Execute( MacroEntry macro )
        {
            _macro = macro;

            if ( Thread != null && Thread.IsAlive )
            {
                Stop();
            }

            MainCommands.SetQuietMode( false );

            _cancellationToken = new CancellationTokenSource();

            if ( _importCache == null )
            {
                _importCache = InitializeImports( _engine );
            }

            ScriptSource source = _engine.CreateScriptSourceFromString( _macro.Macro, SourceCodeKind.Statements );

            Dictionary<string, object> importCache = new Dictionary<string, object>( _importCache );

            IsFaulted = false;

            Thread = new Thread( () =>
            {
                Thread = Thread.CurrentThread;

                try
                {
                    StartedEvent?.Invoke();

                    AliasCommands.SetDefaultAliases();

                    ScriptScope macroScope = _engine.CreateScope( importCache );

                    _stopWatch.Reset();
                    _stopWatch.Start();

                    do
                    {
                        _cancellationToken.Token.ThrowIfCancellationRequested();

                        source.Execute( macroScope );

                        _stopWatch.Stop();

                        bool willLoop = _macro.Loop && !IsFaulted && !_cancellationToken.IsCancellationRequested;

                        if ( !willLoop )
                        {
                            break;
                        }

                        if ( Options.CurrentOptions.Debug )
                        {
                            UO.Commands.SystemMessage( string.Format( Strings.Loop_time___0_, _stopWatch.Elapsed ) );
                        }

                        int diff = 50 - (int) _stopWatch.ElapsedMilliseconds;

                        if ( diff > 0 )
                        {
                            Thread.Sleep( diff );
                        }
                    }
                    while ( _macro.Loop && !IsFaulted );
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

            try
            {
                Thread.Start();
                Thread.Join();
            }
            catch ( ThreadStartException )
            {
                // TODO 
            }
        }

        public void Stop()
        {
            _cancellationToken?.Cancel();

            if ( Thread == null || !Thread.IsAlive )
            {
                return;
            }

            Thread?.Interrupt();
            Thread?.Abort();
            Thread?.Join();
        }
    }
}