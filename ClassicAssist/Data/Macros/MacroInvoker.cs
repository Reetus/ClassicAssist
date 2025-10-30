using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Shared.Resources;
using IronPython.Hosting;
using IronPython.Runtime.Exceptions;
using Microsoft.Graph;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace ClassicAssist.Data.Macros
{
    public class MacroInvoker
    {
        public delegate void dMacroException( Exception e );

        public delegate void dMacroStartStop();

        private static ScriptEngine _engine = Python.CreateEngine();
        private static Dictionary<string, object> _importCache;
        private readonly SystemMessageTextWriter _textWriter = new SystemMessageTextWriter();
        private CancellationTokenSource _cancellationToken;
        private MacroEntry _macro;
        private ScriptScope _macroScope;
        private CompiledCode _compiled;
        private string _lastCompiledHash = string.Empty;

        public MacroInvoker()
        {
            ScriptRuntime runtime = _engine.Runtime;
            runtime.LoadAssembly( Assembly.GetExecutingAssembly() );

            if ( _importCache == null )
            {
                _importCache = InitializeImports( _engine );
            }

            runtime.IO.SetOutput( new TextStream( _textWriter ), Encoding.Unicode );
            runtime.IO.SetErrorOutput( new TextStream( _textWriter), Encoding.Unicode );
            
            _engine = Python.GetEngine( runtime );

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

            foreach ( string assemblyName in AssistantOptions.Assemblies ?? Array.Empty<string>() )
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

            prepend += "from System import Array\n";
            prepend += "import sys";

            return prepend;
        }

        public static Dictionary<string, object> InitializeImports( ScriptEngine engine )
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            foreach( string assembly in AssistantOptions.Assemblies ?? new string[0] )
            {
                try
                {
                    _engine.Runtime.LoadAssembly( Assembly.LoadFile( assembly ) );
                }
                catch( Exception )
                {
                    // ignored
                }
            }

            ScriptSource importSource =
                engine.CreateScriptSourceFromString( GetScriptingImports(), SourceCodeKind.Statements );

            CompiledCode importCompiled = importSource.Compile();
            ScriptScope importScope = engine.CreateScope( dictionary );
            importCompiled.Execute( importScope );

            foreach ( KeyValuePair<string, object> kvp in dictionary.Where( e =>
                e.Key.EndsWith( "Message" ) || e.Key.EndsWith( "Msg" ) ).ToList() )
            {
                string funcName = Regex.Replace( kvp.Key, "(Message|Msg)$", "" );

                if ( string.IsNullOrEmpty( funcName ) )
                {
                    continue;
                }

                foreach ( string suffix in new[] { "Msg", "Message" } )
                {
                    string fullName = $"{funcName}{suffix}";

                    if ( !dictionary.ContainsKey( fullName ) )
                    {
                        dictionary.Add( fullName, kvp.Value );
                    }
                }
            }

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

        public void Execute( MacroEntry macro, object[] parameters = null )
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

                    _macroScope = _engine.CreateScope( importCache );
                    _macroScope.SetVariable( "Events", new Events() );
                    _engine.SetTrace( OnTrace );

                    _macroScope.SetVariable( "args", parameters ?? Array.Empty<object>());

                    StopWatch.Reset();
                    StopWatch.Start();

                    if ( _compiled == null || !_lastCompiledHash.Equals( _macro.Hash ) )
                    {
                        _compiled = source.Compile();
                        _lastCompiledHash = _macro.Hash;
                    }

                    do
                    {
                        _cancellationToken.Token.ThrowIfCancellationRequested();

                        _compiled.Execute( _macroScope );

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
                catch ( SystemExitException )
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
                    MacroManager.GetInstance().OnMacroStopped( macro );
                }
            } ) { IsBackground = true };

            try
            {
                Thread.Start();
                MacroManager.GetInstance().OnMacroStarted( macro );
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

        private TracebackDelegate OnTrace( TraceBackFrame frame, string result, object payload )
        {
            if ( !_cancellationToken.IsCancellationRequested )
            {
                return OnTrace;
            }

            try
            {
                _macroScope.Engine.Execute("sys.exit()", _macroScope);
            }
            catch (SystemExitException)
            {
                throw new TaskCanceledException();
            }
            
            return null;
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

                if ( _macroScope.ContainsVariable( "Events" ) )
                {
                    try
                    {
                        Events events = _macroScope.GetVariable<Events>( "Events" );
                        events.InvokeShutdown();
                    }
                    catch ( Exception e )
                    {
                        UO.Commands.SystemMessage( string.Format( Strings.Macro_error___0_, e.Message ) );
                    }
                }

                //Thread?.Abort();
#if !NET
                Thread?.Interrupt();
#endif
                
                Thread?.Join( 100 );

                MacroManager.GetInstance().Replay = false;
                MacroManager.GetInstance().OnMacroStopped( _macro );
            }
            catch ( ThreadStateException e )
            {
                UO.Commands.SystemMessage( string.Format( Strings.Macro_error___0_, e.Message ) );
            }
        }
    }

    public class Events
    {
        public void InvokeShutdown()
        {
            Shutdown?.Invoke( this, EventArgs.Empty );
        }

        public event EventHandler Shutdown;
    }
}