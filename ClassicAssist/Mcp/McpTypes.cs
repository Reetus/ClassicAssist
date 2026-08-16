using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Mcp
{
    public sealed class JsonRpcRequest
    {
        [JsonProperty( "jsonrpc" )]
        public string JsonRpc { get; set; } = "2.0";

        [JsonProperty( "id", NullValueHandling = NullValueHandling.Ignore )]
        public object Id { get; set; }

        [JsonProperty( "method" )]
        public string Method { get; set; }

        [JsonProperty( "params", NullValueHandling = NullValueHandling.Ignore )]
        public JToken Params { get; set; }
    }

    public sealed class JsonRpcResponse
    {
        [JsonProperty( "jsonrpc" )]
        public string JsonRpc { get; set; } = "2.0";

        [JsonProperty( "id" )]
        public object Id { get; set; }

        [JsonProperty( "result", NullValueHandling = NullValueHandling.Ignore )]
        public object Result { get; set; }

        [JsonProperty( "error", NullValueHandling = NullValueHandling.Ignore )]
        public JsonRpcError Error { get; set; }
    }

    public sealed class JsonRpcError
    {
        [JsonProperty( "code" )]
        public int Code { get; set; }

        [JsonProperty( "message" )]
        public string Message { get; set; }

        [JsonProperty( "data", NullValueHandling = NullValueHandling.Ignore )]
        public object Data { get; set; }
    }

    public sealed class McpTool
    {
        [JsonProperty( "name" )]
        public string Name { get; set; }

        [JsonProperty( "description" )]
        public string Description { get; set; }

        [JsonProperty( "inputSchema" )]
        public JObject InputSchema { get; set; }
    }

    public sealed class CallToolResult
    {
        [JsonProperty( "content" )]
        public List<McpContent> Content { get; set; } = new List<McpContent>();

        [JsonProperty( "isError" )]
        public bool IsError { get; set; }
    }

    public sealed class McpContent
    {
        [JsonProperty( "type" )]
        public string Type { get; set; } = "text";

        [JsonProperty( "text" )]
        public string Text { get; set; }
    }

    public sealed class CallToolParams
    {
        [JsonProperty( "name" )]
        public string Name { get; set; }

        [JsonProperty( "arguments" )]
        public JObject Arguments { get; set; }
    }
}
