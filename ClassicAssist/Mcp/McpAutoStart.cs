using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Mcp
{
    /// <summary>
    ///     Restarts the MCP server across sessions. The MCP toggle stays session-only so several
    ///     instances don't fight over one port, but the port is remembered per character in the
    ///     profile ("McpAutoStart": {"Ports": {"&lt;serial&gt;": port}}).
    ///     <list type="bullet">
    ///         <item>
    ///             Before any character is logged in (login screen, server list) the server starts on
    ///             the first free port from the lowest one the profile has saved, so it is reachable
    ///             from the start.
    ///         </item>
    ///         <item>
    ///             When a character logs in, a server started that way moves to the character's own
    ///             port if that port is free (connected MCP clients reconnect), or is stopped if the
    ///             character has no entry. A server started by hand is left alone.
    ///         </item>
    ///         <item>
    ///             With no server running, the character's port is used, or the next free one when it
    ///             is taken.
    ///         </item>
    ///     </list>
    ///     Unticking the option drops the character from the list.
    ///     <para>
    ///         All of this needs the per-machine consent in <see cref="AssistantOptions.McpAutoStart" />
    ///         ("Start automatically" in the options tab), which also makes the server require a bearer
    ///         token: a server that starts on its own must only answer agents configured for it.
    ///     </para>
    /// </summary>
    public static class McpAutoStart
    {
        private const string KEY = "McpAutoStart";
        private const int PORT_TRIES = 50;

        private static JObject _ports = new JObject();
        private static int _serial;
        private static bool _initialized;

        // The running server was started here (not by the checkbox): only that one is moved or stopped.
        private static bool _startedHere;

        public static void Initialize()
        {
            if ( _initialized )
            {
                return;
            }

            _initialized = true;

            ApplyToken();

            Engine.PlayerInitializedEvent += OnPlayerInitialized;
        }

        /// <summary>
        ///     Gives or withdraws consent (the "Start automatically" option). Enabling creates the bearer
        ///     token every request then needs; disabling drops it, so enabling again issues a new one.
        /// </summary>
        public static void SetEnabled( bool enabled )
        {
            AssistantOptions.McpAutoStart = enabled;
            AssistantOptions.McpToken = enabled ? AssistantOptions.McpToken ?? NewToken() : null;
            McpServer.Token = AssistantOptions.McpToken;

            AssistantOptions.Save();
        }

        /// <summary>
        ///     Called by <see cref="Options.Load" /> after the view models have deserialized, so the
        ///     options tab reflects the server state before this changes it.
        /// </summary>
        public static void Deserialize( JObject json )
        {
            _ports = json?[KEY]?["Ports"] as JObject ?? new JObject();

            ApplyToken();
            Apply();
        }

        /// <summary>
        ///     Called by <see cref="Options.Save" /> before the profile is hashed and written.
        /// </summary>
        public static void Serialize( JObject obj )
        {
            int serial = Engine.Player?.Serial ?? _serial;

            // Several instances can share one profile file and each writes the whole of it: start from
            // the ports on disk so this save doesn't drop the other characters' entries.
            JObject ports = ReadPortsOnDisk( obj["Name"]?.ToObject<string>() ) ?? _ports;

            // Without consent the ports are kept as they are: neither recorded nor dropped.
            if ( serial != 0 && AssistantOptions.McpAutoStart )
            {
                if ( McpServer.IsRunning )
                {
                    ports[SerialKey( serial )] = McpServer.Port;
                }
                else
                {
                    ports.Remove( SerialKey( serial ) );
                }
            }

            _ports = ports;
            obj[KEY] = new JObject { ["Ports"] = ports };
        }

        private static void ApplyToken()
        {
            if ( AssistantOptions.McpAutoStart && string.IsNullOrEmpty( AssistantOptions.McpToken ) )
            {
                AssistantOptions.McpToken = NewToken();
                AssistantOptions.Save();
            }

            McpServer.Token = AssistantOptions.McpAutoStart ? AssistantOptions.McpToken : null;
        }

        private static string NewToken()
        {
            byte[] bytes = new byte[32];

            using ( RandomNumberGenerator rng = RandomNumberGenerator.Create() )
            {
                rng.GetBytes( bytes );
            }

            return BitConverter.ToString( bytes ).Replace( "-", string.Empty ).ToLowerInvariant();
        }

        private static void OnPlayerInitialized( PlayerMobile player )
        {
            // Raised off the UI thread; the server and the bound options are only touched on it.
            if ( Engine.Dispatcher == null )
            {
                Apply();
                return;
            }

            Engine.Dispatcher.BeginInvoke( (Action) Apply );
        }

        private static void Apply()
        {
            if ( !AssistantOptions.McpAutoStart )
            {
                return;
            }

            if ( !McpServer.IsRunning )
            {
                _startedHere = false;
            }

            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                // No character yet: any port this profile has used, so the server is up from the start.
                int? lowest = LowestSavedPort();

                if ( !McpServer.IsRunning && lowest != null )
                {
                    Start( lowest.Value );
                }

                return;
            }

            _serial = player.Serial;

            int? wanted = SavedPort( _serial );

            if ( !McpServer.IsRunning )
            {
                if ( wanted != null )
                {
                    Start( wanted.Value );
                }

                return;
            }

            if ( !_startedHere )
            {
                return;
            }

            if ( wanted == null )
            {
                // Started before the character was known, and this character doesn't use it.
                McpServer.Shutdown();
                _startedHere = false;
                Options.CurrentOptions.McpEnabled = false;
            }
            else if ( McpServer.Port != wanted.Value && IsFree( wanted.Value ) )
            {
                McpServer.Shutdown();
                Start( wanted.Value );
            }
        }

        private static void Start( int port )
        {
            for ( int candidate = port; candidate < port + PORT_TRIES && candidate <= 65535; candidate++ )
            {
                try
                {
                    // Binding a loopback listener is immediate: no wait on the UI thread.
                    McpServer.Initialize( candidate );
                }
                catch ( SocketException e ) when ( e.SocketErrorCode == SocketError.AddressAlreadyInUse )
                {
                    continue;
                }
                catch ( Exception )
                {
                    break;
                }

                _startedHere = true;
                Options.CurrentOptions.McpPort = McpServer.Port;
                Options.CurrentOptions.McpEnabled = true;

                return;
            }

            Options.CurrentOptions.McpEnabled = false;
        }

        private static bool IsFree( int port )
        {
            try
            {
                TcpListener listener = new TcpListener( IPAddress.Loopback, port );

                listener.Start();
                listener.Stop();

                return true;
            }
            catch ( SocketException )
            {
                return false;
            }
        }

        private static int? SavedPort( int serial )
        {
            JToken token = _ports[SerialKey( serial )];

            if ( token == null || token.Type != JTokenType.Integer )
            {
                return null;
            }

            int port = token.ToObject<int>();

            return ValidPort( port ) ? port : (int?) null;
        }

        private static int? LowestSavedPort()
        {
            int? lowest = null;

            foreach ( JProperty property in _ports.Properties() )
            {
                if ( property.Value.Type != JTokenType.Integer )
                {
                    continue;
                }

                int port = property.Value.ToObject<int>();

                if ( ValidPort( port ) )
                {
                    lowest = lowest == null ? port : Math.Min( lowest.Value, port );
                }
            }

            return lowest;
        }

        private static bool ValidPort( int port )
        {
            return port >= 1 && port <= 65535;
        }

        private static JObject ReadPortsOnDisk( string name )
        {
            try
            {
                string directory = Path.IsPathRooted( AssistantOptions.ProfileDirectory )
                    ? AssistantOptions.ProfileDirectory
                    : Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, AssistantOptions.ProfileDirectory );

                string path = Path.Combine( directory, name ?? Options.DEFAULT_SETTINGS_FILENAME );

                if ( !File.Exists( path ) )
                {
                    return null;
                }

                return JObject.Parse( File.ReadAllText( path ) )[KEY]?["Ports"] as JObject;
            }
            catch ( Exception )
            {
                return null;
            }
        }

        private static string SerialKey( int serial )
        {
            return $"0x{serial:X8}";
        }
    }
}
