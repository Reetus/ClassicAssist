using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClassicAssist.DebugAdapter.Dap;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Hosting;

namespace ClassicAssist.DebugAdapter
{
    public sealed class VariableInspector
    {
        private static readonly HashSet<string> FilteredGlobals = new HashSet<string>( StringComparer.Ordinal )
        {
            "__builtins__", "__name__", "__doc__", "__file__", "__package__"
        };

        // Maps frameId → (localsRef, globalsRef)
        private readonly ConcurrentDictionary<int, Tuple<int, int>> _frameRefs =
            new ConcurrentDictionary<int, Tuple<int, int>>();

        // Maps variablesReference → the object to enumerate
        private readonly ConcurrentDictionary<int, object> _refMap = new ConcurrentDictionary<int, object>();

        // Maps threadId → list of frames for stack trace
        private readonly ConcurrentDictionary<int, List<TraceBackFrame>> _threadFrames =
            new ConcurrentDictionary<int, List<TraceBackFrame>>();

        // Reference counter for expandable variables. Starts above frame-based refs.
        private int _nextRef = 10000;

        public void StoreFrames( int threadId, TraceBackFrame topFrame )
        {
            ClearThread( threadId );

            List<TraceBackFrame> frames = new List<TraceBackFrame>();
            TraceBackFrame frame = topFrame;

            while ( frame != null )
            {
                frames.Add( frame );

                try
                {
                    frame = (TraceBackFrame) frame.f_back;
                }
                catch
                {
                    break;
                }
            }

            _threadFrames[threadId] = frames;

            // Pre-allocate refs for each frame
            int baseFrameId = threadId * 1000;

            for ( int i = 0; i < frames.Count; i++ )
            {
                int frameId = baseFrameId + i;
                int localsRef = AllocRef( frames[i].f_locals );
                int globalsRef = AllocRef( frames[i].f_globals );
                _frameRefs[frameId] = Tuple.Create( localsRef, globalsRef );
            }
        }

        public int GetLocalsRef( int frameId )
        {
            Tuple<int, int> refs;
            return _frameRefs.TryGetValue( frameId, out refs ) ? refs.Item1 : 0;
        }

        public int GetGlobalsRef( int frameId )
        {
            Tuple<int, int> refs;
            return _frameRefs.TryGetValue( frameId, out refs ) ? refs.Item2 : 0;
        }

        public Tuple<string, string, int> Evaluate( int frameId, string expression )
        {
            // Find the frame for this frameId
            int threadId = frameId / 1000;
            int frameIndex = frameId % 1000;

            List<TraceBackFrame> frames;

            if ( !_threadFrames.TryGetValue( threadId, out frames ) || frameIndex >= frames.Count )
            {
                throw new InvalidOperationException( "No frame available for evaluation" );
            }

            TraceBackFrame frame = frames[frameIndex];
            ScriptEngine engine = Python.CreateEngine();

            // Build a scope with the frame's locals and globals
            ScriptScope scope = engine.CreateScope();

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

            object result = engine.Execute( expression, scope );
            string repr = SafeRepr( result );
            string type = result?.GetType().Name;
            int childRef = 0;

            if ( IsExpandable( result ) )
            {
                childRef = AllocRef( result );
            }

            return Tuple.Create( repr, type, childRef );
        }

        public DapVariable[] GetVariables( int variablesReference )
        {
            object obj;

            if ( !_refMap.TryGetValue( variablesReference, out obj ) || obj == null )
            {
                return new DapVariable[0];
            }

            try
            {
                if ( obj is PythonDictionary pythonDict )
                {
                    return EnumerateDict( pythonDict );
                }

                if ( obj is IDictionary dict )
                {
                    return EnumerateDict( dict );
                }

                if ( obj is IList list )
                {
                    return EnumerateList( list );
                }

                return new[]
                {
                    new DapVariable { Name = "(value)", Value = SafeRepr( obj ), Type = obj.GetType().Name }
                };
            }
            catch ( Exception e )
            {
                return new[] { new DapVariable { Name = "(error)", Value = e.Message } };
            }
        }

        private DapVariable[] EnumerateDict( PythonDictionary dict )
        {
            List<DapVariable> result = new List<DapVariable>( dict.Count );

            foreach ( KeyValuePair<object, object> kvp in dict )
            {
                string key = kvp.Key?.ToString() ?? "(null)";

                if ( FilteredGlobals.Contains( key ) )
                {
                    continue;
                }

                // Skip module and function objects from display
                if ( kvp.Value is PythonModule || kvp.Value is PythonFunction )
                {
                    continue;
                }

                result.Add( MakeVariable( key, kvp.Value ) );
            }

            result.Sort( ( a, b ) => string.Compare( a.Name, b.Name, StringComparison.Ordinal ) );
            return result.ToArray();
        }

        private DapVariable[] EnumerateDict( IDictionary dict )
        {
            List<DapVariable> result = new List<DapVariable>( dict.Count );

            foreach ( DictionaryEntry entry in dict )
            {
                string key = entry.Key?.ToString() ?? "(null)";

                if ( FilteredGlobals.Contains( key ) )
                {
                    continue;
                }

                result.Add( MakeVariable( key, entry.Value ) );
            }

            result.Sort( ( a, b ) => string.Compare( a.Name, b.Name, StringComparison.Ordinal ) );
            return result.ToArray();
        }

        private DapVariable[] EnumerateList( IList list )
        {
            DapVariable[] result = new DapVariable[list.Count];

            for ( int i = 0; i < list.Count; i++ )
            {
                result[i] = MakeVariable( $"[{i}]", list[i] );
            }

            return result;
        }

        private DapVariable MakeVariable( string name, object value )
        {
            int childRef = 0;

            if ( IsExpandable( value ) )
            {
                childRef = AllocRef( value );
            }

            return new DapVariable
            {
                Name = name,
                Value = SafeRepr( value ),
                Type = value?.GetType().Name ?? "None",
                VariablesReference = childRef
            };
        }

        private static bool IsExpandable( object value )
        {
            return value is PythonDictionary || value is IDictionary || value is IList || value is PythonTuple;
        }

        private int AllocRef( object value )
        {
            if ( value == null )
            {
                return 0;
            }

            int refId = Interlocked.Increment( ref _nextRef );
            _refMap[refId] = value;
            return refId;
        }

        private static string SafeRepr( object value )
        {
            if ( value == null )
            {
                return "None";
            }

            try
            {
                return PythonOps.Repr( DefaultContext.Default, value ) ?? value.ToString() ?? "";
            }
            catch
            {
                try
                {
                    return value.ToString() ?? "(error)";
                }
                catch
                {
                    return "(error)";
                }
            }
        }

        private void ClearThread( int threadId )
        {
            List<TraceBackFrame> removedFrames;

            if ( _threadFrames.TryRemove( threadId, out removedFrames ) )
            {
                // Clean up frame refs for this thread
                int baseFrameId = threadId * 1000;

                for ( int i = 0; i < 100; i++ )
                {
                    int frameId = baseFrameId + i;
                    Tuple<int, int> refs;

                    if ( _frameRefs.TryRemove( frameId, out refs ) )
                    {
                        object removed;
                        _refMap.TryRemove( refs.Item1, out removed );
                        _refMap.TryRemove( refs.Item2, out removed );
                    }
                }
            }
        }

        public Tuple<string, string> SetVariable( int variablesReference, string name, string valueExpression )
        {
            object container;

            if ( !_refMap.TryGetValue( variablesReference, out container ) )
            {
                throw new InvalidOperationException( "Variable container not found" );
            }

            // Evaluate the new value expression
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();

            // Provide existing variables as context for the expression
            if ( container is PythonDictionary contextDict )
            {
                foreach ( KeyValuePair<object, object> kvp in contextDict )
                {
                    string key = kvp.Key as string;

                    if ( key != null )
                    {
                        scope.SetVariable( key, kvp.Value );
                    }
                }
            }

            object newValue = engine.Execute( valueExpression, scope );

            // Write back into the container
            if ( container is PythonDictionary pd )
            {
                pd[name] = newValue;
            }
            else if ( container is IDictionary id )
            {
                id[name] = newValue;
            }
            else if ( container is IList list && name.StartsWith( "[" ) && name.EndsWith( "]" ) )
            {
                int idx;

                if ( int.TryParse( name.Substring( 1, name.Length - 2 ), out idx ) )
                {
                    list[idx] = newValue;
                }
                else
                {
                    throw new InvalidOperationException( "Cannot set variable in this container" );
                }
            }
            else
            {
                throw new InvalidOperationException( "Cannot set variable in this container" );
            }

            return Tuple.Create( SafeRepr( newValue ), newValue?.GetType().Name );
        }

        public CompletionItem[] GetCompletions( int frameId, string text, int column )
        {
            // Extract the word being typed (everything after the last non-identifier char)
            string prefix = text.Substring( 0, Math.Min( Math.Max( column - 1, 0 ), text.Length ) );
            int dotIndex = prefix.LastIndexOf( '.' );
            int wordStart = prefix.LastIndexOfAny( new[] { ' ', '(', '[', ',', '=', '+', '-', '*', '/', ':', '{' } );
            string word = prefix.Substring( Math.Max( dotIndex, wordStart ) + 1 );

            int threadId = frameId / 1000;
            int frameIndex = frameId % 1000;

            List<TraceBackFrame> frames;

            if ( !_threadFrames.TryGetValue( threadId, out frames ) || frameIndex >= frames.Count )
            {
                return new CompletionItem[0];
            }

            TraceBackFrame frame = frames[frameIndex];
            HashSet<string> names = new HashSet<string>( StringComparer.Ordinal );

            Action<object> collectNames = dict =>
            {
                PythonDictionary pd = dict as PythonDictionary;

                if ( pd == null )
                {
                    return;
                }

                foreach ( KeyValuePair<object, object> kvp in pd )
                {
                    string key = kvp.Key as string;

                    if ( key != null && !FilteredGlobals.Contains( key ) &&
                         key.StartsWith( word, StringComparison.OrdinalIgnoreCase ) &&
                         !( kvp.Value is PythonModule ) && !( kvp.Value is PythonFunction ) )
                    {
                        names.Add( key );
                    }
                }
            };

            collectNames( frame.f_locals );
            collectNames( frame.f_globals );

            return names.OrderBy( n => n )
                .Select( n => new CompletionItem { Label = n, Type = "variable" } )
                .ToArray();
        }

        public void Clear()
        {
            _refMap.Clear();
            _frameRefs.Clear();
            _threadFrames.Clear();
        }
    }
}
