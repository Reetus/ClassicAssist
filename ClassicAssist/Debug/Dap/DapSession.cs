using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClassicAssist.Data.Macros;
using IronPython.Runtime.Exceptions;

namespace ClassicAssist.Debug.Dap
{
    public sealed class DapSession
    {
        private readonly TcpClient _client;
        private readonly DebugManager _debugManager;
        private readonly VariableInspector _variableInspector = new VariableInspector();
        private readonly object _writeLock = new object();
        private bool _closed;
        private string _launchedArgs;
        private string _launchedProgram;
        private bool _pendingLaunch;
        private int _seq;
        private NetworkStream _stream;

        public DapSession( TcpClient client, DebugManager debugManager )
        {
            _client = client;
            _debugManager = debugManager;
            _debugManager.SendEvent += OnDebugEvent;
        }

        public async Task RunAsync( CancellationToken token )
        {
            _stream = _client.GetStream();

            try
            {
                await ReadLoop( token );
            }
            catch ( OperationCanceledException )
            {
            }
            catch ( Exception ) when ( !_closed )
            {
            }
            finally
            {
                Close();
            }
        }

        public void Close()
        {
            if ( _closed )
            {
                return;
            }

            _closed = true;
            _debugManager.SendEvent -= OnDebugEvent;
            _debugManager.IsActive = false;
            _debugManager.ResumeAll();

            try
            {
                _stream?.Close();
                _client.Close();
            }
            catch
            {
                // ignored
            }
        }

        private void OnDebugEvent( DapEvent evt )
        {
            SendMessage( evt );
        }

        public void SendMessage( DapMessage message )
        {
            if ( _closed || _stream == null )
            {
                return;
            }

            message.Seq = Interlocked.Increment( ref _seq );

            byte[] bytes = DapProtocol.Serialize( message );

            lock ( _writeLock )
            {
                try
                {
                    _stream.Write( bytes, 0, bytes.Length );
                    _stream.Flush();
                }
                catch ( Exception )
                {
                    // ignored
                }
            }
        }

        private DapResponse MakeResponse( DapRequest request, bool success = true, object body = null,
            string message = null )
        {
            return new DapResponse
            {
                RequestSeq = request.Seq,
                Command = request.Command,
                Success = success,
                Body = body,
                Message = message
            };
        }

        private async Task ReadLoop( CancellationToken token )
        {
            byte[] buffer = new byte[8192];
            MemoryStream pending = new MemoryStream();

            while ( !token.IsCancellationRequested && !_closed && _stream != null )
            {
                int read = await _stream.ReadAsync( buffer, 0, buffer.Length, token );

                if ( read == 0 )
                {
                    break;
                }

                pending.Write( buffer, 0, read );

                byte[] messageBytes;

                while ( TryExtractMessage( pending, out messageBytes ) )
                {
                    try
                    {
                        DapRequest request = DapProtocol.DeserializeRequest( messageBytes );

                        if ( request != null )
                        {
                            HandleRequest( request );
                        }
                    }
                    catch ( Exception )
                    {
                        // ignored — malformed message
                    }
                }
            }
        }

        private static bool TryExtractMessage( MemoryStream pending, out byte[] messageBytes )
        {
            messageBytes = null;
            byte[] data = pending.GetBuffer();
            int length = (int) pending.Length;

            int headerEnd = IndexOfHeaderTerminator( data, length );

            if ( headerEnd < 0 )
            {
                return false;
            }

            string headerStr = Encoding.ASCII.GetString( data, 0, headerEnd );
            int contentLength = -1;

            foreach ( string line in headerStr.Split( new[] { "\r\n" }, StringSplitOptions.None ) )
            {
                if ( line.StartsWith( "Content-Length:", StringComparison.OrdinalIgnoreCase ) )
                {
                    int len;

                    if ( int.TryParse( line.Substring( 15 ).Trim(), out len ) )
                    {
                        contentLength = len;
                    }
                }
            }

            if ( contentLength < 0 )
            {
                return false;
            }

            int bodyStart = headerEnd + 4;
            int totalLength = bodyStart + contentLength;

            if ( length < totalLength )
            {
                return false;
            }

            messageBytes = new byte[contentLength];
            Buffer.BlockCopy( data, bodyStart, messageBytes, 0, contentLength );

            // Compact the pending buffer
            int remaining = length - totalLength;

            if ( remaining > 0 )
            {
                Buffer.BlockCopy( data, totalLength, data, 0, remaining );
            }

            pending.SetLength( remaining );
            pending.Position = remaining;

            return true;
        }

        private static int IndexOfHeaderTerminator( byte[] data, int length )
        {
            for ( int i = 0; i + 3 < length; i++ )
            {
                if ( data[i] == (byte) '\r' && data[i + 1] == (byte) '\n' &&
                     data[i + 2] == (byte) '\r' && data[i + 3] == (byte) '\n' )
                {
                    return i;
                }
            }

            return -1;
        }

        private void HandleRequest( DapRequest request )
        {
            DapResponse response;

            switch ( request.Command )
            {
                case "initialize":
                    response = HandleInitialize( request );
                    break;
                case "attach":
                    response = HandleAttach( request );
                    break;
                case "launch":
                    response = HandleLaunch( request );
                    break;
                case "terminate":
                    response = HandleTerminate( request );
                    break;
                case "restart":
                    response = HandleRestart( request );
                    break;
                case "setBreakpoints":
                    response = HandleSetBreakpoints( request );
                    break;
                case "setExceptionBreakpoints":
                    response = HandleSetExceptionBreakpoints( request );
                    break;
                case "configurationDone":
                    response = HandleConfigurationDone( request );
                    break;
                case "threads":
                    response = HandleThreads( request );
                    break;
                case "stackTrace":
                    response = HandleStackTrace( request );
                    break;
                case "scopes":
                    response = HandleScopes( request );
                    break;
                case "variables":
                    response = HandleVariables( request );
                    break;
                case "setVariable":
                    response = HandleSetVariable( request );
                    break;
                case "evaluate":
                    response = HandleEvaluate( request );
                    break;
                case "completions":
                    response = HandleCompletions( request );
                    break;
                case "exceptionInfo":
                    response = HandleExceptionInfo( request );
                    break;
                case "continue":
                    response = HandleContinue( request );
                    break;
                case "next":
                    response = HandleNext( request );
                    break;
                case "stepIn":
                    response = HandleStepIn( request );
                    break;
                case "stepOut":
                    response = HandleStepOut( request );
                    break;
                case "pause":
                    response = HandlePause( request );
                    break;
                case "disconnect":
                    response = HandleDisconnect( request );
                    break;
                default:
                    response = MakeResponse( request, false, message: $"Unknown command: {request.Command}" );
                    break;
            }

            SendMessage( response );
        }

        private DapResponse HandleInitialize( DapRequest request )
        {
            DapResponse response = MakeResponse( request, body: new Capabilities() );

            // Send initialized event after the response
            Task.Run( () =>
            {
                Thread.Sleep( 10 );
                SendMessage( DapEvent.Initialized() );
            } );

            return response;
        }

        private DapResponse HandleAttach( DapRequest request )
        {
            _debugManager.IsActive = true;

            // Notify VSCode about macros that were already running before attach
            foreach ( Tuple<int, string> thread in _debugManager.GetThreads() )
            {
                OnDebugEvent( DapEvent.Thread( thread.Item1, "started" ) );
            }

            return MakeResponse( request );
        }

        private DapResponse HandleLaunch( DapRequest request )
        {
            LaunchArguments args = DapProtocol.DeserializeArguments<LaunchArguments>( request.Arguments );
            _debugManager.IsActive = true;

            if ( string.IsNullOrEmpty( args?.Program ) )
            {
                return MakeResponse( request, false, message: "Missing 'program' in launch configuration" );
            }

            // Store launch info — actual execution deferred to configurationDone
            // so VSCode has time to send setBreakpoints first
            _launchedProgram = args.Program;
            _launchedArgs = args.Args;
            _pendingLaunch = true;

            return MakeResponse( request );
        }

        private DapResponse HandleTerminate( DapRequest request )
        {
            MacroManager.GetInstance().StopAll();

            SendMessage( DapEvent.Terminated() );

            return MakeResponse( request );
        }

        private DapResponse HandleRestart( DapRequest request )
        {
            MacroManager manager = MacroManager.GetInstance();
            manager.StopAll();

            if ( !string.IsNullOrEmpty( _launchedProgram ) )
            {
                MacroEntry macro = FindMacroByProgram( _launchedProgram );

                if ( macro != null )
                {
                    manager.Execute( macro );
                }
            }

            return MakeResponse( request );
        }

        private DapResponse HandleConfigurationDone( DapRequest request )
        {
            if ( _pendingLaunch && !string.IsNullOrEmpty( _launchedProgram ) )
            {
                _pendingLaunch = false;

                MacroManager manager = MacroManager.GetInstance();
                MacroEntry macro = FindMacroByProgram( _launchedProgram );

                if ( macro != null )
                {
                    object[] macroArgs = null;

                    if ( !string.IsNullOrEmpty( _launchedArgs ) )
                    {
                        macroArgs = _launchedArgs.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                            .Select( a => a.Trim() )
                            .Cast<object>()
                            .ToArray();
                    }

                    manager.Execute( macro, macroArgs );
                }
            }

            return MakeResponse( request );
        }

        private static MacroEntry FindMacroByProgram( string program )
        {
            string normalized = DebugManager.NormalizePath( program );

            return MacroManager.GetInstance().Items.FirstOrDefault( m =>
                m.IsFileBacked && !string.IsNullOrEmpty( m.FilePath ) &&
                DebugManager.NormalizePath( m.FilePath ).Equals( normalized, StringComparison.OrdinalIgnoreCase ) );
        }

        private DapResponse HandleSetBreakpoints( DapRequest request )
        {
            SetBreakpointsArguments args = DapProtocol.DeserializeArguments<SetBreakpointsArguments>( request.Arguments );

            if ( args?.Source?.Path == null )
            {
                return MakeResponse( request, false, message: "Missing source path" );
            }

            BreakpointInfo[] infos = ( args.Breakpoints ?? new SourceBreakpoint[0] ).Select( b => new BreakpointInfo
            {
                Line = b.Line,
                Condition = b.Condition,
                LogMessage = b.LogMessage
            } ).ToArray();

            _debugManager.SetBreakpoints( args.Source.Path, infos );

            DapBreakpoint[] verified = infos.Select( ( bp, i ) => new DapBreakpoint
            {
                Id = i + 1,
                Verified = true,
                Line = bp.Line,
                Source = new DapSource { Path = args.Source.Path, Name = Path.GetFileName( args.Source.Path ) }
            } ).ToArray();

            return MakeResponse( request, body: new { breakpoints = verified } );
        }

        private DapResponse HandleThreads( DapRequest request )
        {
            DapThread[] threads = _debugManager.GetThreads()
                .Select( t => new DapThread { Id = t.Item1, Name = t.Item2 } )
                .ToArray();

            return MakeResponse( request, body: new { threads } );
        }

        private DapResponse HandleStackTrace( DapRequest request )
        {
            StackTraceArguments args = DapProtocol.DeserializeArguments<StackTraceArguments>( request.Arguments );

            if ( args == null )
            {
                return MakeResponse( request, false, message: "Missing arguments" );
            }

            MacroDebugState state = _debugManager.GetThreadState( args.ThreadId );

            if ( state?.CurrentFrame == null )
            {
                return MakeResponse( request,
                    body: new { stackFrames = new DapStackFrame[0], totalFrames = 0 } );
            }

            System.Collections.Generic.List<DapStackFrame> frames =
                new System.Collections.Generic.List<DapStackFrame>();
            TraceBackFrame frame = state.CurrentFrame;
            int frameId = args.ThreadId * 1000;

            while ( frame != null )
            {
                string fileName = frame.f_code.co_filename;

                frames.Add( new DapStackFrame
                {
                    Id = frameId++,
                    Name = frame.f_code.co_name ?? "<module>",
                    Line = (int) frame.f_lineno,
                    Column = 1,
                    Source = string.IsNullOrEmpty( fileName ) || fileName == "<string>"
                        ? null
                        : new DapSource { Path = fileName, Name = Path.GetFileName( fileName ) }
                } );

                try
                {
                    frame = (TraceBackFrame) frame.f_back;
                }
                catch
                {
                    break;
                }
            }

            // Store frames for scopes/variables lookup
            _variableInspector.StoreFrames( args.ThreadId, state.CurrentFrame );

            return MakeResponse( request,
                body: new { stackFrames = frames.ToArray(), totalFrames = frames.Count } );
        }

        private DapResponse HandleScopes( DapRequest request )
        {
            ScopesArguments args = DapProtocol.DeserializeArguments<ScopesArguments>( request.Arguments );

            if ( args == null )
            {
                return MakeResponse( request, false, message: "Missing arguments" );
            }

            DapScope[] scopes =
            {
                new DapScope
                {
                    Name = "Locals",
                    VariablesReference = _variableInspector.GetLocalsRef( args.FrameId ),
                    Expensive = false
                },
                new DapScope
                {
                    Name = "Globals",
                    VariablesReference = _variableInspector.GetGlobalsRef( args.FrameId ),
                    Expensive = true
                }
            };

            return MakeResponse( request, body: new { scopes } );
        }

        private DapResponse HandleVariables( DapRequest request )
        {
            VariablesArguments args = DapProtocol.DeserializeArguments<VariablesArguments>( request.Arguments );

            if ( args == null )
            {
                return MakeResponse( request, false, message: "Missing arguments" );
            }

            DapVariable[] variables = _variableInspector.GetVariables( args.VariablesReference );

            return MakeResponse( request, body: new { variables } );
        }

        private DapResponse HandleEvaluate( DapRequest request )
        {
            EvaluateArguments args = DapProtocol.DeserializeArguments<EvaluateArguments>( request.Arguments );

            if ( args == null || string.IsNullOrWhiteSpace( args.Expression ) )
            {
                return MakeResponse( request, false, message: "Missing expression" );
            }

            // Route !commands to game state inspector (works without paused debugger)
            if ( args.Expression.StartsWith( "!" ) )
            {
                Tuple<bool, string> result = GameStateInspector.Execute( args.Expression );

                return MakeResponse( request, body: new
                {
                    result = result.Item2,
                    type = (string) null,
                    variablesReference = 0
                } );
            }

            int frameId = args.FrameId ?? 0;

            try
            {
                Tuple<string, string, int> result = _variableInspector.Evaluate( frameId, args.Expression );

                return MakeResponse( request, body: new
                {
                    result = result.Item1,
                    type = result.Item2,
                    variablesReference = result.Item3
                } );
            }
            catch ( Exception e )
            {
                return MakeResponse( request, body: new
                {
                    result = e.Message,
                    type = (string) null,
                    variablesReference = 0
                } );
            }
        }

        private DapResponse HandleSetExceptionBreakpoints( DapRequest request )
        {
            SetExceptionBreakpointsArguments args =
                DapProtocol.DeserializeArguments<SetExceptionBreakpointsArguments>( request.Arguments );
            _debugManager.SetExceptionBreakpoints( args?.Filters ?? new string[0] );
            return MakeResponse( request );
        }

        private DapResponse HandleSetVariable( DapRequest request )
        {
            SetVariableArguments args = DapProtocol.DeserializeArguments<SetVariableArguments>( request.Arguments );

            if ( args == null )
            {
                return MakeResponse( request, false, message: "Missing arguments" );
            }

            try
            {
                Tuple<string, string> result =
                    _variableInspector.SetVariable( args.VariablesReference, args.Name, args.Value );

                return MakeResponse( request, body: new { value = result.Item1, type = result.Item2 } );
            }
            catch ( Exception e )
            {
                return MakeResponse( request, false, message: e.Message );
            }
        }

        private DapResponse HandleCompletions( DapRequest request )
        {
            CompletionsArguments args = DapProtocol.DeserializeArguments<CompletionsArguments>( request.Arguments );

            if ( args == null )
            {
                return MakeResponse( request, body: new { targets = new CompletionItem[0] } );
            }

            CompletionItem[] items = _variableInspector.GetCompletions( args.FrameId ?? 0, args.Text, args.Column );

            return MakeResponse( request, body: new { targets = items } );
        }

        private DapResponse HandleExceptionInfo( DapRequest request )
        {
            ExceptionInfoArguments args =
                DapProtocol.DeserializeArguments<ExceptionInfoArguments>( request.Arguments );

            if ( args == null )
            {
                return MakeResponse( request, false, message: "Missing arguments" );
            }

            MacroDebugState state = _debugManager.GetThreadState( args.ThreadId );

            string description = state?.StoppedReason == "exception" ? "Python exception" : "Unknown";

            return MakeResponse( request, body: new
            {
                exceptionId = "PythonException",
                description,
                breakMode = _debugManager.BreakOnAllExceptions ? "always" : "unhandled"
            } );
        }

        private DapResponse HandleContinue( DapRequest request )
        {
            ContinueArguments args = DapProtocol.DeserializeArguments<ContinueArguments>( request.Arguments );

            if ( args != null )
            {
                _debugManager.Continue( args.ThreadId );
            }

            return MakeResponse( request, body: new { allThreadsContinued = false } );
        }

        private DapResponse HandleNext( DapRequest request )
        {
            NextArguments args = DapProtocol.DeserializeArguments<NextArguments>( request.Arguments );

            if ( args != null )
            {
                _debugManager.StepOver( args.ThreadId );
            }

            return MakeResponse( request );
        }

        private DapResponse HandleStepIn( DapRequest request )
        {
            StepInArguments args = DapProtocol.DeserializeArguments<StepInArguments>( request.Arguments );

            if ( args != null )
            {
                _debugManager.StepInto( args.ThreadId );
            }

            return MakeResponse( request );
        }

        private DapResponse HandleStepOut( DapRequest request )
        {
            StepOutArguments args = DapProtocol.DeserializeArguments<StepOutArguments>( request.Arguments );

            if ( args != null )
            {
                _debugManager.StepOut( args.ThreadId );
            }

            return MakeResponse( request );
        }

        private DapResponse HandlePause( DapRequest request )
        {
            PauseArguments args = DapProtocol.DeserializeArguments<PauseArguments>( request.Arguments );

            if ( args != null )
            {
                _debugManager.ForcePause( args.ThreadId );
            }

            return MakeResponse( request );
        }

        private DapResponse HandleDisconnect( DapRequest request )
        {
            _debugManager.IsActive = false;
            _debugManager.ResumeAll();
            _debugManager.ClearAllBreakpoints();

            Task.Run( () =>
            {
                Thread.Sleep( 50 );
                Close();
            } );

            return MakeResponse( request );
        }
    }
}
