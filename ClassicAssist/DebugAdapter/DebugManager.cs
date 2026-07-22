using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ClassicAssist.Data.Macros;
using ClassicAssist.DebugAdapter.Dap;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Hosting;

namespace ClassicAssist.DebugAdapter
{
    public sealed class BreakpointInfo
    {
        public int Line { get; set; }
        public string Condition { get; set; }
        public string LogMessage { get; set; }
    }

    public sealed class DebugManager
    {
        private static DebugManager _instance;

        private readonly ConcurrentDictionary<string, Dictionary<int, BreakpointInfo>> _breakpoints =
            new ConcurrentDictionary<string, Dictionary<int, BreakpointInfo>>( StringComparer.OrdinalIgnoreCase );

        private readonly ConcurrentDictionary<int, MacroDebugState> _threadStates =
            new ConcurrentDictionary<int, MacroDebugState>();

        private readonly ConcurrentDictionary<int, MacroEntry> _threadMacros =
            new ConcurrentDictionary<int, MacroEntry>();

        private readonly object _breakpointLock = new object();

        public volatile bool IsActive;
        public volatile bool BreakOnAllExceptions;
        public volatile bool BreakOnUncaughtExceptions = true;

        public static DebugManager Instance => _instance;

        public event Action<DapEvent> SendEvent;

        public static DebugManager GetInstance()
        {
            return _instance ?? ( _instance = new DebugManager() );
        }

        public void SetBreakpoints( string filePath, BreakpointInfo[] breakpoints )
        {
            string key = NormalizePath( filePath );

            if ( breakpoints.Length == 0 )
            {
                Dictionary<int, BreakpointInfo> removed;
                _breakpoints.TryRemove( key, out removed );
                return;
            }

            lock ( _breakpointLock )
            {
                _breakpoints[key] = breakpoints.ToDictionary( b => b.Line );
            }
        }

        public void SetExceptionBreakpoints( string[] filters )
        {
            BreakOnAllExceptions = filters.Contains( "all" );
            BreakOnUncaughtExceptions = filters.Contains( "uncaught" );
        }

        /// <summary>
        ///     Called from OnTrace for "line" events. Returns true if execution should pause.
        /// </summary>
        public bool ShouldBreak( string file, int line, int threadId, TraceBackFrame frame )
        {
            if ( string.IsNullOrEmpty( file ) )
            {
                return false;
            }

            // Check stepping state first
            MacroDebugState state;

            if ( _threadStates.TryGetValue( threadId, out state ) )
            {
                switch ( state.StepMode )
                {
                    case StepMode.StepInto:
                        return true;
                    case StepMode.StepOver:
                        return GetCallDepth( frame ) <= state.StepDepth;
                    case StepMode.StepOut:
                        return GetCallDepth( frame ) < state.StepDepth;
                }
            }

            // Check breakpoints
            string key = NormalizePath( file );

            Dictionary<int, BreakpointInfo> bps;

            if ( !_breakpoints.TryGetValue( key, out bps ) )
            {
                return false;
            }

            BreakpointInfo bp;

            if ( !bps.TryGetValue( line, out bp ) )
            {
                return false;
            }

            // Log point — output message instead of breaking
            if ( !string.IsNullOrEmpty( bp.LogMessage ) )
            {
                string msg = InterpolateLogMessage( bp.LogMessage, frame );
                Action<DapEvent> handler = SendEvent;
                handler?.Invoke( DapEvent.Output( msg + "\n", "console" ) );
                return false;
            }

            // Conditional breakpoint — eval condition in frame scope
            if ( !string.IsNullOrEmpty( bp.Condition ) )
            {
                return EvalCondition( bp.Condition, frame );
            }

            return true;
        }

        /// <summary>
        ///     Called from OnTrace for "exception" events. Returns true if execution should pause.
        /// </summary>
        public bool ShouldBreakOnException( int threadId, object payload )
        {
            return BreakOnAllExceptions;
        }

        public void OnBreakpoint( int threadId, TraceBackFrame frame, string reason = "breakpoint" )
        {
            MacroDebugState state;

            if ( !_threadStates.TryGetValue( threadId, out state ) )
            {
                return;
            }

            state.CurrentFrame = frame;
            state.StoppedFile = frame.f_code.co_filename;
            state.StoppedLine = (int) frame.f_lineno;
            state.StoppedReason = reason;
            state.StepMode = StepMode.None;
            state.Pause();

            Action<DapEvent> handler = SendEvent;
            handler?.Invoke( DapEvent.Stopped( threadId, reason, state.StoppedFile, state.StoppedLine ) );
        }

        public void WaitForResume( int threadId, CancellationToken token )
        {
            MacroDebugState state;

            if ( _threadStates.TryGetValue( threadId, out state ) )
            {
                state.Wait( token );
            }
        }

        public void Continue( int threadId )
        {
            MacroDebugState state;

            if ( _threadStates.TryGetValue( threadId, out state ) )
            {
                state.StepMode = StepMode.None;
                state.Resume();

                Action<DapEvent> handler = SendEvent;
                handler?.Invoke( DapEvent.Continued( threadId ) );
            }
        }

        public void StepOver( int threadId )
        {
            MacroDebugState state;

            if ( _threadStates.TryGetValue( threadId, out state ) )
            {
                state.StepMode = StepMode.StepOver;
                state.StepDepth = GetCallDepth( state );
                state.Resume();
            }
        }

        public void StepInto( int threadId )
        {
            MacroDebugState state;

            if ( _threadStates.TryGetValue( threadId, out state ) )
            {
                state.StepMode = StepMode.StepInto;
                state.Resume();
            }
        }

        public void StepOut( int threadId )
        {
            MacroDebugState state;

            if ( _threadStates.TryGetValue( threadId, out state ) )
            {
                state.StepMode = StepMode.StepOut;
                state.StepDepth = GetCallDepth( state );
                state.Resume();
            }
        }

        public void ForcePause( int threadId )
        {
            MacroDebugState state;

            if ( _threadStates.TryGetValue( threadId, out state ) && !state.IsPaused )
            {
                state.StepMode = StepMode.StepInto;
            }
        }

        public void OnMacroStarted( MacroEntry macro )
        {
            Thread thread = macro.MacroInvoker?.Thread;

            if ( thread == null )
            {
                return;
            }

            int threadId = thread.ManagedThreadId;
            MacroDebugState state = new MacroDebugState( threadId, macro.Name );
            _threadStates[threadId] = state;
            _threadMacros[threadId] = macro;

            if ( IsActive )
            {
                Action<DapEvent> handler = SendEvent;
                handler?.Invoke( DapEvent.Thread( threadId, "started" ) );
            }
        }

        public void OnMacroStopped( MacroEntry macro )
        {
            Thread thread = macro.MacroInvoker?.Thread;

            if ( thread == null )
            {
                return;
            }

            int threadId = thread.ManagedThreadId;

            MacroDebugState state;

            if ( _threadStates.TryRemove( threadId, out state ) )
            {
                state.Resume();
                state.Dispose();
            }

            MacroEntry removed;
            _threadMacros.TryRemove( threadId, out removed );

            if ( IsActive )
            {
                Action<DapEvent> handler = SendEvent;
                handler?.Invoke( DapEvent.Thread( threadId, "exited" ) );
            }
        }

        public Tuple<int, string>[] GetThreads()
        {
            return _threadStates.Values
                .Select( s => Tuple.Create( s.ThreadId, s.MacroName ) )
                .ToArray();
        }

        public MacroDebugState GetThreadState( int threadId )
        {
            MacroDebugState state;
            return _threadStates.TryGetValue( threadId, out state ) ? state : null;
        }

        public void ResumeAll()
        {
            foreach ( MacroDebugState state in _threadStates.Values )
            {
                if ( state.IsPaused )
                {
                    state.StepMode = StepMode.None;
                    state.Resume();
                }
            }
        }

        public void ClearAllBreakpoints()
        {
            _breakpoints.Clear();
        }

        private static int GetCallDepth( MacroDebugState state )
        {
            return GetCallDepth( state.CurrentFrame );
        }

        private static int GetCallDepth( TraceBackFrame frame )
        {
            int depth = 0;
            TraceBackFrame f = frame;

            while ( f != null )
            {
                depth++;

                try
                {
                    f = (TraceBackFrame) f.f_back;
                }
                catch
                {
                    break;
                }
            }

            return depth;
        }

        private static void PopulateScope( ScriptScope scope, TraceBackFrame frame )
        {
            // Globals first, then locals so same-named locals shadow globals. Loading globals is
            // what lets conditions/logpoints reference module globals, imported macro commands and
            // args — not just locals — matching VariableInspector.Evaluate.
            if ( frame.f_globals is PythonDictionary globals )
            {
                foreach ( KeyValuePair<object, object> kvp in globals )
                {
                    string key = kvp.Key as string;

                    if ( key != null )
                    {
                        scope.SetVariable( key, kvp.Value );
                    }
                }
            }

            if ( frame.f_locals is PythonDictionary locals )
            {
                foreach ( KeyValuePair<object, object> kvp in locals )
                {
                    string key = kvp.Key as string;

                    if ( key != null )
                    {
                        scope.SetVariable( key, kvp.Value );
                    }
                }
            }
        }

        private static bool IsTruthy( object value )
        {
            if ( value == null )
            {
                return false;
            }

            if ( value is bool b )
            {
                return b;
            }

            if ( value is int i )
            {
                return i != 0;
            }

            return true;
        }

        private static bool EvalCondition( string condition, TraceBackFrame frame )
        {
            try
            {
                ScriptEngine engine = Python.CreateEngine();
                ScriptScope scope = engine.CreateScope();

                PopulateScope( scope, frame );

                object result = engine.Execute( condition, scope );
                return IsTruthy( result );
            }
            catch
            {
                return false;
            }
        }

        private static string InterpolateLogMessage( string message, TraceBackFrame frame )
        {
            // DAP log messages use {expression} for interpolation
            if ( !message.Contains( "{" ) )
            {
                return message;
            }

            try
            {
                ScriptEngine engine = Python.CreateEngine();
                ScriptScope scope = engine.CreateScope();

                PopulateScope( scope, frame );

                return Regex.Replace( message, @"\{(.+?)\}", m =>
                {
                    try
                    {
                        object val = engine.Execute( m.Groups[1].Value, scope );
                        return val?.ToString() ?? "None";
                    }
                    catch
                    {
                        return m.Value;
                    }
                } );
            }
            catch
            {
                return message;
            }
        }

        internal static string NormalizePath( string path )
        {
            return path.Replace( '\\', '/' ).TrimEnd( '/' );
        }
    }
}
