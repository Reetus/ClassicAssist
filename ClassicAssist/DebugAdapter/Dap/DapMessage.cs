using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.DebugAdapter.Dap
{
    public class DapMessage
    {
        [JsonProperty( "seq" )]
        public int Seq { get; set; }

        [JsonProperty( "type" )]
        public string Type { get; set; } = "";
    }

    public sealed class DapRequest : DapMessage
    {
        public DapRequest()
        {
            Type = "request";
        }

        [JsonProperty( "command" )]
        public string Command { get; set; } = "";

        [JsonProperty( "arguments", NullValueHandling = NullValueHandling.Ignore )]
        public JToken Arguments { get; set; }
    }

    public sealed class DapResponse : DapMessage
    {
        public DapResponse()
        {
            Type = "response";
        }

        [JsonProperty( "request_seq" )]
        public int RequestSeq { get; set; }

        [JsonProperty( "success" )]
        public bool Success { get; set; }

        [JsonProperty( "command" )]
        public string Command { get; set; } = "";

        [JsonProperty( "message", NullValueHandling = NullValueHandling.Ignore )]
        public string Message { get; set; }

        [JsonProperty( "body", NullValueHandling = NullValueHandling.Ignore )]
        public object Body { get; set; }
    }

    public sealed class DapEvent : DapMessage
    {
        public DapEvent()
        {
            Type = "event";
        }

        [JsonProperty( "event" )]
        public string Event { get; set; } = "";

        [JsonProperty( "body", NullValueHandling = NullValueHandling.Ignore )]
        public object Body { get; set; }

        public static DapEvent Stopped( int threadId, string reason, string file = null, int line = 0 )
        {
            return new DapEvent
            {
                Event = "stopped",
                Body = new StoppedEventBody
                {
                    Reason = reason,
                    ThreadId = threadId,
                    AllThreadsStopped = false
                }
            };
        }

        public static DapEvent Continued( int threadId )
        {
            return new DapEvent
            {
                Event = "continued",
                Body = new ContinuedEventBody { ThreadId = threadId, AllThreadsContinued = false }
            };
        }

        public static DapEvent Thread( int threadId, string reason )
        {
            return new DapEvent
            {
                Event = "thread",
                Body = new ThreadEventBody { ThreadId = threadId, Reason = reason }
            };
        }

        public static DapEvent Initialized()
        {
            return new DapEvent { Event = "initialized" };
        }

        public static DapEvent Terminated()
        {
            return new DapEvent { Event = "terminated" };
        }

        public static DapEvent Output( string text, string category = "console" )
        {
            return new DapEvent
            {
                Event = "output",
                Body = new OutputEventBody { Category = category, Output = text }
            };
        }
    }

    public static class DapProtocol
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public static byte[] Serialize( DapMessage message )
        {
            string json = JsonConvert.SerializeObject( message, message.GetType(), SerializerSettings );
            string header = $"Content-Length: {Encoding.UTF8.GetByteCount( json )}\r\n\r\n";
            byte[] headerBytes = Encoding.ASCII.GetBytes( header );
            byte[] bodyBytes = Encoding.UTF8.GetBytes( json );

            byte[] result = new byte[headerBytes.Length + bodyBytes.Length];
            headerBytes.CopyTo( result, 0 );
            bodyBytes.CopyTo( result, headerBytes.Length );

            return result;
        }

        public static DapRequest DeserializeRequest( byte[] json )
        {
            JObject root = JObject.Parse( Encoding.UTF8.GetString( json ) );

            DapRequest request = new DapRequest
            {
                Seq = root["seq"]?.ToObject<int>() ?? 0,
                Command = root["command"]?.ToObject<string>() ?? ""
            };

            JToken args = root["arguments"];

            if ( args != null && args.Type != JTokenType.Null )
            {
                request.Arguments = args;
            }

            return request;
        }

        public static T DeserializeArguments<T>( JToken arguments )
        {
            if ( arguments == null )
            {
                return default( T );
            }

            return arguments.ToObject<T>();
        }
    }
}
