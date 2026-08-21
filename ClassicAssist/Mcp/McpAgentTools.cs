using System;
using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Data.Dress;
using ClassicAssist.Data.Friends;
using ClassicAssist.Data.Organizer;
using ClassicAssist.Data.Scavenger;
using ClassicAssist.Data.TrapPouch;
using ClassicAssist.Data.Vendors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Mcp
{
    public static class McpAgentTools
    {
        private static readonly string[] _agents = { "dress", "organizer", "friends", "vendorbuy", "scavenger", "trappouch", "autoloot" };

        public static IReadOnlyList<McpTool> GetTools()
        {
            return new List<McpTool>
            {
                new McpTool
                {
                    Name = "listAgents",
                    Description = "List the agents (dress, organizer, friends, vendor buy, scavenger, trap pouch, autoloot) with their entry counts.",
                    InputSchema = McpTools.ObjectSchema()
                },
                new McpTool
                {
                    Name = "getAgent",
                    Description = "Get the current entries of an agent so you can query them (e.g. check if a mobile serial is on the friends list). Agents: dress, organizer, friends, vendorbuy, scavenger, trappouch, autoloot.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["agent"] = McpTools.StringProperty( "The agent name (dress, organizer, friends, vendorbuy, scavenger, trappouch, autoloot)." ),
                            ["filter"] = McpTools.StringProperty( "Optional case-insensitive substring to match entry name or serial (e.g. '0x006ec0dc')." )
                        }, "agent" )
                }
            };
        }

        public static CallToolResult Invoke( string name, JObject args )
        {
            try
            {
                switch ( name )
                {
                    case "listAgents":
                        return McpTools.Text( ListAgents() );
                    case "getAgent":
                        return McpTools.Text( GetAgent( McpTools.RequireString( args, "agent" ), McpTools.GetString( args, "filter" ) ) );
                    default:
                        return null;
                }
            }
            catch ( Exception e )
            {
                return McpTools.Error( e.Message );
            }
        }

        private static string ListAgents()
        {
            JArray array = new JArray();

            foreach ( string agent in _agents )
            {
                int count = OnUi( () => GetEntryCount( agent ) );

                array.Add( new JObject { ["agent"] = agent, ["entryCount"] = count } );
            }

            JObject result = new JObject
            {
                ["agents"] = array
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetAgent( string agent, string filter )
        {
            agent = Normalize( agent );

            if ( !_agents.Contains( agent ) )
            {
                throw new InvalidOperationException(
                    $"Unknown agent '{agent}'. Expected one of: {string.Join( ", ", _agents )}." );
            }

            JArray entries = OnUi( () => GetEntries( agent, filter ) );

            JObject result = new JObject
            {
                ["agent"] = agent,
                ["entryCount"] = entries.Count,
                ["entries"] = entries
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static int GetEntryCount( string agent )
        {
            return GetEntries( agent, null ).Count;
        }

        private static JArray GetEntries( string agent, string filter )
        {
            JArray array = new JArray();

            bool Match( JObject entry )
            {
                if ( string.IsNullOrEmpty( filter ) )
                {
                    return true;
                }

                string name = entry["name"]?.ToObject<string>() ?? "";
                string serial = entry["serial"]?.ToObject<string>() ?? "";

                return name.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) >= 0 ||
                       serial.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) >= 0;
            }

            switch ( agent )
            {
                case "friends":
                {
                    foreach ( FriendEntry entry in Options.CurrentOptions.Friends ?? Enumerable.Empty<FriendEntry>() )
                    {
                        JObject o = new JObject
                        {
                            ["name"] = entry.Name,
                            ["serial"] = $"0x{entry.Serial:x8}"
                        };

                        if ( Match( o ) )
                        {
                            array.Add( o );
                        }
                    }

                    break;
                }
                case "dress":
                {
                    foreach ( DressAgentEntry entry in DressManager.GetInstance().Items ?? Enumerable.Empty<DressAgentEntry>() )
                    {
                        JArray items = new JArray();

                        foreach ( DressAgentItem item in entry.Items ?? new List<DressAgentItem>() )
                        {
                            items.Add( new JObject
                            {
                                ["name"] = item.Name,
                                ["serial"] = item.Serial != 0 ? $"0x{item.Serial:x8}" : null,
                                ["graphic"] = $"0x{item.ID:x4}",
                                ["layer"] = item.Layer.ToString()
                            } );
                        }

                        JObject o = new JObject
                        {
                            ["name"] = entry.Name,
                            ["undressContainer"] = entry.UndressContainer != 0 ? $"0x{entry.UndressContainer:x8}" : null,
                            ["itemCount"] = items.Count,
                            ["items"] = items
                        };

                        if ( Match( o ) )
                        {
                            array.Add( o );
                        }
                    }

                    break;
                }
                case "organizer":
                {
                    foreach ( OrganizerEntry entry in OrganizerManager.GetInstance().Items ?? Enumerable.Empty<OrganizerEntry>() )
                    {
                        JArray items = new JArray();

                        IEnumerable<OrganizerItem> organizerItems = entry.Items ?? Enumerable.Empty<OrganizerItem>();

                        foreach ( OrganizerItem item in organizerItems )
                        {
                            items.Add( new JObject
                            {
                                ["item"] = item.Item,
                                ["id"] = $"0x{item.ID:x4}",
                                ["hue"] = item.Hue,
                                ["amount"] = item.Amount
                            } );
                        }

                        JObject o = new JObject
                        {
                            ["name"] = entry.Name,
                            ["sourceContainer"] = entry.SourceContainer != 0 ? $"0x{entry.SourceContainer:x8}" : null,
                            ["destinationContainer"] = entry.DestinationContainer != 0 ? $"0x{entry.DestinationContainer:x8}" : null,
                            ["stack"] = entry.Stack,
                            ["returnExcess"] = entry.ReturnExcess,
                            ["itemCount"] = items.Count,
                            ["items"] = items
                        };

                        if ( Match( o ) )
                        {
                            array.Add( o );
                        }
                    }

                    break;
                }
                case "scavenger":
                {
                    foreach ( ScavengerEntry entry in ScavengerManager.GetInstance().Items ?? Enumerable.Empty<ScavengerEntry>() )
                    {
                        JObject o = new JObject
                        {
                            ["name"] = entry.Name,
                            ["enabled"] = entry.Enabled,
                            ["graphic"] = $"0x{entry.Graphic:x4}",
                            ["hue"] = entry.Hue
                        };

                        if ( Match( o ) )
                        {
                            array.Add( o );
                        }
                    }

                    break;
                }
                case "trappouch":
                {
                    foreach ( TrapPouchEntry entry in TrapPouchManager.GetInstance().Items ?? Enumerable.Empty<TrapPouchEntry>() )
                    {
                        JObject o = new JObject
                        {
                            ["name"] = entry.Name,
                            ["serial"] = $"0x{entry.Serial:x8}"
                        };

                        if ( Match( o ) )
                        {
                            array.Add( o );
                        }
                    }

                    break;
                }
                case "vendorbuy":
                {
                    foreach ( VendorBuyAgentEntry entry in VendorBuyManager.GetInstance().Items ?? Enumerable.Empty<VendorBuyAgentEntry>() )
                    {
                        JArray items = new JArray();

                        IEnumerable<VendorBuyAgentItem> buyItems = entry.Items ?? Enumerable.Empty<VendorBuyAgentItem>();

                        foreach ( VendorBuyAgentItem item in buyItems )
                        {
                            items.Add( new JObject
                            {
                                ["name"] = item.Name,
                                ["enabled"] = item.Enabled,
                                ["graphic"] = $"0x{item.Graphic:x4}",
                                ["hue"] = item.Hue,
                                ["amount"] = item.Amount
                            } );
                        }

                        JObject o = new JObject
                        {
                            ["name"] = entry.Name,
                            ["enabled"] = entry.Enabled,
                            ["itemCount"] = items.Count,
                            ["items"] = items
                        };

                        if ( Match( o ) )
                        {
                            array.Add( o );
                        }
                    }

                    break;
                }
                case "autoloot":
                {
                    foreach ( AutolootEntry entry in AutolootManager.GetInstance().GetEntries() ?? new List<AutolootEntry>() )
                    {
                        JObject o = new JObject
                        {
                            ["name"] = entry.Name,
                            ["enabled"] = entry.Enabled,
                            ["autoloot"] = entry.Autoloot,
                            ["constraintCount"] = entry.Constraints?.Count ?? 0
                        };

                        if ( Match( o ) )
                        {
                            array.Add( o );
                        }
                    }

                    break;
                }
            }

            return array;
        }

        private static string Normalize( string agent )
        {
            if ( string.IsNullOrEmpty( agent ) )
            {
                return agent;
            }

            agent = agent.ToLowerInvariant().Trim().Replace( "-", "" ).Replace( "_", "" ).Replace( " ", "" );

            switch ( agent )
            {
                case "vendorbuy":
                case "venderbuy":
                case "vendor_buy":
                    return "vendorbuy";
                case "trappouch":
                case "trappouches":
                case "trap_pouch":
                    return "trappouch";
                default:
                    return agent;
            }
        }

        private static T OnUi<T>( Func<T> func )
        {
            if ( Engine.Dispatcher == null )
            {
                return func();
            }

            return Engine.Dispatcher.Invoke( func );
        }
    }
}