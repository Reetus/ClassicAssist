using Newtonsoft.Json;

namespace ClassicAssist.DebugAdapter.Dap
{
    // --- Capabilities ---

    public sealed class Capabilities
    {
        [JsonProperty( "supportsConfigurationDoneRequest" )]
        public bool SupportsConfigurationDoneRequest { get; set; } = true;

        [JsonProperty( "supportsSingleThreadExecutionRequests" )]
        public bool SupportsSingleThreadExecutionRequests { get; set; } = true;

        [JsonProperty( "supportsStepBack" )]
        public bool SupportsStepBack { get; set; }

        [JsonProperty( "supportsTerminateRequest" )]
        public bool SupportsTerminateRequest { get; set; } = true;

        [JsonProperty( "supportsRestartRequest" )]
        public bool SupportsRestartRequest { get; set; } = true;

        [JsonProperty( "supportsEvaluateForHovers" )]
        public bool SupportsEvaluateForHovers { get; set; } = true;

        [JsonProperty( "supportsConditionalBreakpoints" )]
        public bool SupportsConditionalBreakpoints { get; set; } = true;

        [JsonProperty( "supportsLogPoints" )]
        public bool SupportsLogPoints { get; set; } = true;

        [JsonProperty( "supportsSetVariable" )]
        public bool SupportsSetVariable { get; set; } = true;

        [JsonProperty( "supportsCompletionsRequest" )]
        public bool SupportsCompletionsRequest { get; set; } = true;

        [JsonProperty( "supportsExceptionInfoRequest" )]
        public bool SupportsExceptionInfoRequest { get; set; } = true;

        // Only "all" is advertised: the IronPython trace "exception" event fires at every frame as
        // an exception propagates and cannot reliably distinguish caught from uncaught, so we don't
        // expose an "uncaught" filter we couldn't honour.
        [JsonProperty( "exceptionBreakpointFilters" )]
        public ExceptionBreakpointFilter[] ExceptionBreakpointFilters { get; set; } =
        {
            new ExceptionBreakpointFilter { Filter = "all", Label = "All Exceptions", Default = false }
        };
    }

    public sealed class ExceptionBreakpointFilter
    {
        [JsonProperty( "filter" )]
        public string Filter { get; set; } = "";

        [JsonProperty( "label" )]
        public string Label { get; set; } = "";

        [JsonProperty( "default" )]
        public bool Default { get; set; }
    }

    // --- Source ---

    public sealed class DapSource
    {
        [JsonProperty( "name", NullValueHandling = NullValueHandling.Ignore )]
        public string Name { get; set; }

        [JsonProperty( "path", NullValueHandling = NullValueHandling.Ignore )]
        public string Path { get; set; }
    }

    // --- Breakpoints ---

    public sealed class SourceBreakpoint
    {
        [JsonProperty( "line" )]
        public int Line { get; set; }

        [JsonProperty( "condition", NullValueHandling = NullValueHandling.Ignore )]
        public string Condition { get; set; }

        [JsonProperty( "logMessage", NullValueHandling = NullValueHandling.Ignore )]
        public string LogMessage { get; set; }

        [JsonProperty( "hitCondition", NullValueHandling = NullValueHandling.Ignore )]
        public string HitCondition { get; set; }
    }

    public sealed class SetBreakpointsArguments
    {
        [JsonProperty( "source" )]
        public DapSource Source { get; set; } = new DapSource();

        [JsonProperty( "breakpoints" )]
        public SourceBreakpoint[] Breakpoints { get; set; }
    }

    public sealed class DapBreakpoint
    {
        [JsonProperty( "id" )]
        public int Id { get; set; }

        [JsonProperty( "verified" )]
        public bool Verified { get; set; }

        [JsonProperty( "line" )]
        public int Line { get; set; }

        [JsonProperty( "source", NullValueHandling = NullValueHandling.Ignore )]
        public DapSource Source { get; set; }
    }

    // --- Threads ---

    public sealed class DapThread
    {
        [JsonProperty( "id" )]
        public int Id { get; set; }

        [JsonProperty( "name" )]
        public string Name { get; set; } = "";
    }

    // --- Stack Frames ---

    public sealed class StackTraceArguments
    {
        [JsonProperty( "threadId" )]
        public int ThreadId { get; set; }

        [JsonProperty( "startFrame" )]
        public int? StartFrame { get; set; }

        [JsonProperty( "levels" )]
        public int? Levels { get; set; }
    }

    public sealed class DapStackFrame
    {
        [JsonProperty( "id" )]
        public int Id { get; set; }

        [JsonProperty( "name" )]
        public string Name { get; set; } = "";

        [JsonProperty( "source", NullValueHandling = NullValueHandling.Ignore )]
        public DapSource Source { get; set; }

        [JsonProperty( "line" )]
        public int Line { get; set; }

        [JsonProperty( "column" )]
        public int Column { get; set; } = 1;
    }

    // --- Scopes ---

    public sealed class ScopesArguments
    {
        [JsonProperty( "frameId" )]
        public int FrameId { get; set; }
    }

    public sealed class DapScope
    {
        [JsonProperty( "name" )]
        public string Name { get; set; } = "";

        [JsonProperty( "variablesReference" )]
        public int VariablesReference { get; set; }

        [JsonProperty( "expensive" )]
        public bool Expensive { get; set; }
    }

    // --- Variables ---

    public sealed class VariablesArguments
    {
        [JsonProperty( "variablesReference" )]
        public int VariablesReference { get; set; }
    }

    public sealed class DapVariable
    {
        [JsonProperty( "name" )]
        public string Name { get; set; } = "";

        [JsonProperty( "value" )]
        public string Value { get; set; } = "";

        [JsonProperty( "type", NullValueHandling = NullValueHandling.Ignore )]
        public string Type { get; set; }

        [JsonProperty( "variablesReference" )]
        public int VariablesReference { get; set; }
    }

    // --- Continue / Step ---

    public sealed class ContinueArguments
    {
        [JsonProperty( "threadId" )]
        public int ThreadId { get; set; }

        [JsonProperty( "singleThread" )]
        public bool? SingleThread { get; set; }
    }

    public sealed class NextArguments
    {
        [JsonProperty( "threadId" )]
        public int ThreadId { get; set; }

        [JsonProperty( "singleThread" )]
        public bool? SingleThread { get; set; }

        [JsonProperty( "granularity", NullValueHandling = NullValueHandling.Ignore )]
        public string Granularity { get; set; }
    }

    public sealed class StepInArguments
    {
        [JsonProperty( "threadId" )]
        public int ThreadId { get; set; }

        [JsonProperty( "singleThread" )]
        public bool? SingleThread { get; set; }
    }

    public sealed class StepOutArguments
    {
        [JsonProperty( "threadId" )]
        public int ThreadId { get; set; }

        [JsonProperty( "singleThread" )]
        public bool? SingleThread { get; set; }
    }

    public sealed class PauseArguments
    {
        [JsonProperty( "threadId" )]
        public int ThreadId { get; set; }
    }

    public sealed class EvaluateArguments
    {
        [JsonProperty( "expression" )]
        public string Expression { get; set; } = "";

        [JsonProperty( "frameId" )]
        public int? FrameId { get; set; }

        [JsonProperty( "context", NullValueHandling = NullValueHandling.Ignore )]
        public string Context { get; set; }
    }

    public sealed class LaunchArguments
    {
        [JsonProperty( "program" )]
        public string Program { get; set; }

        [JsonProperty( "args", NullValueHandling = NullValueHandling.Ignore )]
        public string Args { get; set; }
    }

    public sealed class DisconnectArguments
    {
        [JsonProperty( "restart" )]
        public bool? Restart { get; set; }

        [JsonProperty( "terminateDebuggee" )]
        public bool? TerminateDebuggee { get; set; }
    }

    // --- Exception Breakpoints ---

    public sealed class SetExceptionBreakpointsArguments
    {
        [JsonProperty( "filters" )]
        public string[] Filters { get; set; } = new string[0];
    }

    // --- Set Variable ---

    public sealed class SetVariableArguments
    {
        [JsonProperty( "variablesReference" )]
        public int VariablesReference { get; set; }

        [JsonProperty( "name" )]
        public string Name { get; set; } = "";

        [JsonProperty( "value" )]
        public string Value { get; set; } = "";
    }

    // --- Completions ---

    public sealed class CompletionsArguments
    {
        [JsonProperty( "frameId" )]
        public int? FrameId { get; set; }

        [JsonProperty( "text" )]
        public string Text { get; set; } = "";

        [JsonProperty( "column" )]
        public int Column { get; set; }
    }

    public sealed class CompletionItem
    {
        [JsonProperty( "label" )]
        public string Label { get; set; } = "";

        [JsonProperty( "type", NullValueHandling = NullValueHandling.Ignore )]
        public string Type { get; set; }

        [JsonProperty( "text", NullValueHandling = NullValueHandling.Ignore )]
        public string Text { get; set; }
    }

    // --- Exception Info ---

    public sealed class ExceptionInfoArguments
    {
        [JsonProperty( "threadId" )]
        public int ThreadId { get; set; }
    }

    // --- Event Bodies ---

    public sealed class StoppedEventBody
    {
        [JsonProperty( "reason" )]
        public string Reason { get; set; } = "";

        [JsonProperty( "threadId" )]
        public int ThreadId { get; set; }

        [JsonProperty( "allThreadsStopped" )]
        public bool AllThreadsStopped { get; set; }
    }

    public sealed class ContinuedEventBody
    {
        [JsonProperty( "threadId" )]
        public int ThreadId { get; set; }

        [JsonProperty( "allThreadsContinued" )]
        public bool AllThreadsContinued { get; set; }
    }

    public sealed class ThreadEventBody
    {
        [JsonProperty( "reason" )]
        public string Reason { get; set; } = "";

        [JsonProperty( "threadId" )]
        public int ThreadId { get; set; }
    }

    public sealed class OutputEventBody
    {
        [JsonProperty( "category" )]
        public string Category { get; set; } = "console";

        [JsonProperty( "output" )]
        public string Output { get; set; } = "";
    }
}
