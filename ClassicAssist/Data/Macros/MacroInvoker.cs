#region License

// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Objects;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using LiteDB.Engine;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ClassicAssist.Data.Macros
{
    public class MacroInvoker
    {
        public delegate void dMacroException( Exception e );

        public delegate void dMacroPaused( int lineNumber, AutoResetEvent autoResetEvent, Dictionary<string, object> frameVariables );

        public delegate void dMacroStartStop();

        private static readonly ScriptEngine _engine = Python.CreateEngine();
        private static Dictionary<string, object> _importCache;
        private readonly MemoryStream _memoryStream = new MemoryStream();
        private readonly AutoResetEvent _pauseEvent = new AutoResetEvent( false );
        private readonly SystemMessageTextWriter _textWriter = new SystemMessageTextWriter();
        private CancellationTokenSource _cancellationToken;
        private CompiledCode _compiled;
        private string _lastCompiledHash = string.Empty;
        private MacroEntry _macro;
        private ScriptScope _macroScope;

        public MacroInvoker()
        {
            ScriptRuntime runtime = _engine.Runtime;
            runtime.LoadAssembly( Assembly.GetExecutingAssembly() );

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

        public Stopwatch StopWatch { get; set; } = new Stopwatch();

        public Thread Thread { get; set; }

        public event dMacroStartStop StartedEvent;
        public event dMacroStartStop StoppedEvent;
        public event dMacroException ExceptionEvent;
        public event dMacroPaused PausedEvent;

        private static string GetScriptingImports()
        {
            string prepend = Assembly.GetExecutingAssembly().GetTypes().Where( t => t.Namespace != null && t.IsPublic && t.IsClass && t.Namespace.EndsWith( "Macros.Commands" ) )
                .Aggregate( string.Empty, ( current, t ) => current + $"from {t.FullName} import * \n" );

            foreach ( string assemblyName in AssistantOptions.Assemblies ?? Array.Empty<string>() )
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile( assemblyName );

                    prepend += assembly.GetTypes().Where( t => t.Namespace != null && t.IsPublic && t.IsClass && t.Namespace.EndsWith( "Macros.Commands" ) )
                        .Aggregate( string.Empty, ( current, t ) => current + $"from {t.FullName} import * \n" );
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

            foreach ( string assembly in AssistantOptions.Assemblies ?? new string[0] )
            {
                try
                {
                    _engine.Runtime.LoadAssembly( Assembly.LoadFile( assembly ) );
                }
                catch ( Exception )
                {
                    // ignored
                }
            }

            ScriptSource importSource = engine.CreateScriptSourceFromString( GetScriptingImports(), SourceCodeKind.Statements );

            CompiledCode importCompiled = importSource.Compile();
            ScriptScope importScope = engine.CreateScope( dictionary );
            importCompiled.Execute( importScope );

            foreach ( KeyValuePair<string, object> kvp in dictionary.Where( e => e.Key.EndsWith( "Message" ) || e.Key.EndsWith( "Msg" ) ).ToList() )
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

                    _macroScope.SetVariable( "args", parameters ?? Array.Empty<object>() );

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

        private static Dictionary<string, object> GetFrameVariables( TraceBackFrame frame )
        {
            Dictionary<string, object> variables = new Dictionary<string, object>();

            // Locals
            if ( frame.f_locals is PythonDictionary locals )
            {
                foreach ( KeyValuePair<object, object> kvp in locals )
                {
                    if ( !( kvp.Key is string key ) )
                    {
                        continue;
                    }

                    object value = kvp.Value;

                    // Skip built-in functions / methods
                    if ( value is BuiltinFunction || value is BuiltinMethodDescriptor || value is PythonType || value is PythonModule )
                    {
                        continue;
                    }

                    variables[key] = value;
                }
            }

            // Globals (optional)
            if ( frame.f_globals is PythonDictionary globals )
            {
                foreach ( KeyValuePair<object, object> kvp in globals )
                {
                    if ( kvp.Key is string key && !variables.ContainsKey( key ) ) // don't overwrite locals
                    {
                        object value = kvp.Value;

                        // Skip built-in functions / methods
                        if ( value is BuiltinFunction || value is BuiltinMethodDescriptor || value is PythonType || value is PythonModule )
                        {
                            continue;
                        }

                        variables[key] = value;
                    }
                }
            }

            return variables;
        }

        private TracebackDelegate OnTrace( TraceBackFrame frame, string result, object payload )
        {
            if ( result == "line" && ( _macro.Breakpoints != null && _macro.Breakpoints.Contains( (int) frame.f_lineno ) || _macro.IsPaused ) )
            {
                _pauseEvent.Reset();
                PausedEvent?.Invoke( (int) frame.f_lineno, _pauseEvent, GetFrameVariables( frame ) );
                _pauseEvent.WaitOne();
            }

            if ( !_cancellationToken.IsCancellationRequested )
            {
                return OnTrace;
            }

            _macroScope.Engine.Execute( "sys.exit()", _macroScope );
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
                Thread?.Interrupt();
                Thread?.Join( 100 );

                MacroManager.GetInstance().Replay = false;
                MacroManager.GetInstance().OnMacroStopped( _macro );
            }
            catch ( ThreadStateException e )
            {
                UO.Commands.SystemMessage( string.Format( Strings.Macro_error___0_, e.Message ) );
            }
        }

        public static string GetDisplayValue( object value, bool isClipboardCopy = false )
        {
            switch ( value )
            {
                case null:
                    return "None";
                case string s:
                    return $"\"{s}\"";
                case bool b:
                    return b ? "True" : "False";
                case double _:
                case int _:
                case long _:
                case float _:
                case decimal _:
                    return value.ToString();
                case Entity entity:
                    return isClipboardCopy ? $"0x{entity.Serial:x8}" : $"{entity.Name} (0x{entity.Serial:x8})";
            }

            switch ( value )
            {
                //case List pyList:
                //    if ( isClipboardCopy )
                //    {
                //        return "[" + string.Join( ", ", pyList.Select( e => GetDisplayValue( e ) ) ) + "]";
                //    }

                //    return "[" + string.Join( ", ", pyList.Take( 10 ).Select( e => GetDisplayValue( e ) ) ) + ( pyList.__len__() > 10 ? ", ..." : "" ) + "]";
                case PythonDictionary pyDict:
                    if ( isClipboardCopy )
                    {
                        return "{" + string.Join( ", ", pyDict.Select( kv => $"{GetDisplayValue( kv.Key )}: {GetDisplayValue( kv.Value )}" ) ) + "}";
                    }

                    return "{" + string.Join( ", ", pyDict.Take( 5 ).Select( kv => $"{GetDisplayValue( kv.Key )}: {GetDisplayValue( kv.Value )}" ) ) +
                           ( pyDict.Count > 5 ? ", ..." : "" ) + "}";
            }

            if ( value is IEnumerable enumerable && value.GetType() != typeof( string ) )
            {
                if ( isClipboardCopy )
                {
                    IEnumerable<string> allItems = enumerable.Cast<object>().Select( i => GetDisplayValue( i ) );
                    return "[" + string.Join( ", ", allItems ) + "]";
                }

                IEnumerable<string> items = enumerable.Cast<object>().Take( 10 ).Select( i => GetDisplayValue( i ) );
                return "[" + string.Join( ", ", items ) + "]";
            }

            // Fallback to repr() if available
            try
            {
                dynamic dyn = value;
                dynamic repr = dyn.__repr__();

                if ( repr is string repStr )
                {
                    return repStr;
                }
            }
            catch
            {
                /* ignore */
            }

            // Fallback: type name
            return value.ToString() ?? value.GetType().Name;
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