using System.Threading;
using IronPython.Runtime.Exceptions;

namespace ClassicAssist.DebugAdapter
{
    public enum StepMode
    {
        None,
        StepOver,
        StepInto,
        StepOut
    }

    public sealed class MacroDebugState
    {
        private readonly ManualResetEventSlim _breakEvent = new ManualResetEventSlim( true );

        public MacroDebugState( int threadId, string macroName )
        {
            ThreadId = threadId;
            MacroName = macroName;
        }

        public int ThreadId { get; }
        public string MacroName { get; }
        public StepMode StepMode { get; set; }
        public int StepDepth { get; set; }
        public TraceBackFrame CurrentFrame { get; set; }
        public string StoppedFile { get; set; }
        public int StoppedLine { get; set; }
        public string StoppedReason { get; set; }

        public bool IsPaused => !_breakEvent.IsSet;

        public void Pause()
        {
            _breakEvent.Reset();
        }

        public void Resume()
        {
            CurrentFrame = null;
            _breakEvent.Set();
        }

        public void Wait( CancellationToken token )
        {
            _breakEvent.Wait( token );
        }

        public void Dispose()
        {
            _breakEvent.Dispose();
        }
    }
}
