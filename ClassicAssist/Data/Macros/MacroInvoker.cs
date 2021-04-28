using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Shared.Resources;
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
        private readonly MemoryStream _memoryStream = new MemoryStream();
        private readonly SystemMessageTextWriter _textWriter = new SystemMessageTextWriter();
        private CancellationTokenSource _cancellationToken;
        private MacroEntry _macro;

        public MacroInvoker()
        {
            ScriptRuntime runtime = _engine.Runtime;
            runtime.LoadAssembly( Assembly.GetExecutingAssembly() );

            foreach ( string assembly in AssistantOptions.Assemblies ?? new string[0] )
            {
                try
                {
                    runtime.LoadAssembly( Assembly.LoadFile( assembly ) );
                }
                catch ( Exception )
                {
                    // ignored
                }
            }

            if ( _importCache == null )
            {
                _importCache = InitializeImports( _engine );
            }

            _engine.Runtime.IO.SetOutput( _memoryStream, _textWriter );
            _engine.Runtime.IO.SetErrorOutput( _memoryStream, _textWriter );

            string modulePath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Modules" );
            ICollection<string> searchPaths = _engine.GetSearchPaths();

            if ( searchPaths.Contains( modulePath ) )
            {
                return;
            }

            searchPaths.Add( modulePath );
            _engine.SetSearchPaths( searchPaths );
        }

        public Exception Exception { get; set; }
        public bool IsFaulted { get; set; }

        public bool IsRunning => Thread?.IsAlive ?? false;

        public Stopwatch StopWatch { get; set; } = new Stopwatch();

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

            foreach ( string assemblyName in AssistantOptions.Assemblies ?? new string[0] )
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile( assemblyName );

                    prepend += assembly.GetTypes()
                        .Where( t =>
                            t.Namespace != null && t.IsPublic && t.IsClass &&
                            t.Namespace.EndsWith( "Macros.Commands" ) ).Aggregate( string.Empty,
                            ( current, t ) => current + $"from {t.FullName} import * \n" );
                }
                catch ( Exception )
                {
                    // ignored
                }
            }

            prepend += "from System import Array";

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

        public static void ResetImportCache()
        {
            _importCache = InitializeImports( _engine );

            foreach ( string module in _engine.GetModuleFilenames() )
            {
                _engine.Execute( $"import {module};reload({module})" );
            }
        }

        public void Execute( MacroEntry macro )
        {
            _macro = macro;

            if ( Thread != null && Thread.IsAlive )
            {
                Stop();
            }

            MainCommands.SetQuietMode( Options.CurrentOptions.DefaultMacroQuietMode );

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

                    StopWatch.Reset();
                    StopWatch.Start();

                    do
                    {
                        _cancellationToken.Token.ThrowIfCancellationRequested();

                        source.Execute( macroScope );

                        StopWatch.Stop();

                        bool willLoop = _macro.Loop && !IsFaulted && !_cancellationToken.IsCancellationRequested;

                        if ( !willLoop )
                        {
                            break;
                        }

                        if ( Options.CurrentOptions.Debug )
                        {
                            UO.Commands.SystemMessage( string.Format( Strings.Loop_time___0_, StopWatch.Elapsed ) );
                        }

                        int diff = 50 - (int) StopWatch.ElapsedMilliseconds;

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
                    MacroManager.GetInstance().OnMacroStopped();
                }
            } ) { IsBackground = true };

            try
            {
                Thread.Start();
                MacroManager.GetInstance().OnMacroStarted();
            }
            catch ( ThreadStateException )
            {
                // TODO 
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

            try
            {
                StopWatch.Stop();

                int diff = 50 - (int) StopWatch.ElapsedMilliseconds;

                if ( diff > 0 )
                {
                    Thread.Sleep( diff );
                }

                Thread?.Abort();
                Thread?.Join( 100 );

                MacroManager.GetInstance().Replay = false;
                MacroManager.GetInstance().OnMacroStopped();
            }
            catch ( ThreadStateException e )
            {
                UO.Commands.SystemMessage( string.Format( Strings.Macro_error___0_, e.Message ) );
            }
        }
    }
}