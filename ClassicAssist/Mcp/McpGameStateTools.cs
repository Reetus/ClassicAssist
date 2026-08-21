using System;
using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using ClassicAssist.UO.Objects.Gumps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Mcp
{
    public static class McpGameStateTools
    {
        public static IReadOnlyList<McpTool> GetTools()
        {
            return new List<McpTool>
            {
                new McpTool
                {
                    Name = "getPlayer",
                    Description = "Get the current player's properties (name, serial, hits, mana, stamina, stats, position, gold, weight, etc.).",
                    InputSchema = McpTools.ObjectSchema()
                },
                new McpTool
                {
                    Name = "getBackpack",
                    Description = "Get the contents of the player's backpack.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["filter"] = McpTools.StringProperty( "Optional case-insensitive substring to filter item names." )
                        } )
                },
                new McpTool
                {
                    Name = "getTileInfo",
                    Description = "Get the land tile and statics at a map coordinate (like the Object Inspector).",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["x"] = McpTools.IntegerProperty( "The X coordinate." ),
                            ["y"] = McpTools.IntegerProperty( "The Y coordinate." ),
                            ["map"] = McpTools.IntegerProperty( "Optional map index (0-5), defaults to the player's map." )
                        }, "x", "y" )
                },
                new McpTool
                {
                    Name = "listGumps",
                    Description = "List all currently open gumps.",
                    InputSchema = McpTools.ObjectSchema()
                },
                new McpTool
                {
                    Name = "getGumpLayout",
                    Description = "Get the layout, strings and elements of a gump by id or serial.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["id"] = McpTools.StringProperty( "Optional gump id (decimal or 0x hex)." ),
                            ["serial"] = McpTools.StringProperty( "Optional gump serial (decimal or 0x hex)." )
                        } )
                },
                new McpTool
                {
                    Name = "getItemInfo",
                    Description = "Get detailed info and the list of cliloc properties for a mobile or item by serial.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["serial"] = McpTools.StringProperty( "The entity serial (decimal or 0x hex)." ) }, "serial" )
                },
                new McpTool
                {
                    Name = "getCliloc",
                    Description = "Get the localized string for a cliloc id (e.g. 1062724 = Sacred Journey).",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["cliloc"] = McpTools.IntegerProperty( "The cliloc id (decimal)." ) }, "cliloc" )
                },
                new McpTool
                {
                    Name = "getMobiles",
                    Description = "Get the list of mobiles within a certain distance of the player.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["range"] = McpTools.IntegerProperty( "Maximum distance in tiles from the player (default 10)." ) } )
                },
                new McpTool
                {
                    Name = "getItems",
                    Description = "Get the list of items within a certain distance of the player.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["range"] = McpTools.IntegerProperty( "Maximum distance in tiles from the player (default 10)." ) } )
                },
                new McpTool
                {
                    Name = "getContainer",
                    Description = "Get the contents of a container (e.g. bank box or backpack) by serial.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["serial"] = McpTools.StringProperty( "The container serial (decimal or 0x hex)." ) }, "serial" )
                },
                new McpTool
                {
                    Name = "getJournal",
                    Description = "Get recent game journal entries (system messages, speech, macro output).",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["filter"] = McpTools.StringProperty( "Optional case-insensitive substring to filter entry text." ),
                            ["count"] = McpTools.IntegerProperty( "Maximum number of most recent entries to return (default 20)." )
                        } )
                }
            };
        }

        public static CallToolResult Invoke( string name, JObject args )
        {
            try
            {
                switch ( name )
                {
                    case "getPlayer":
                        return McpTools.Text( GetPlayer() );
                    case "getBackpack":
                        return McpTools.Text( GetBackpack( McpTools.GetString( args, "filter" ) ) );
                    case "getTileInfo":
                        return McpTools.Text( GetTileInfo(
                            McpTools.RequireInt( args, "x" ),
                            McpTools.RequireInt( args, "y" ),
                            McpTools.GetInt( args, "map" ) ) );
                    case "listGumps":
                        return McpTools.Text( ListGumps() );
                    case "getGumpLayout":
                        return McpTools.Text( GetGumpLayout(
                            McpTools.GetString( args, "id" ),
                            McpTools.GetString( args, "serial" ) ) );
                    case "getItemInfo":
                        return McpTools.Text( GetItemInfo( McpTools.RequireString( args, "serial" ) ) );
                    case "getCliloc":
                        return McpTools.Text( GetCliloc( McpTools.RequireInt( args, "cliloc" ) ) );
                    case "getMobiles":
                        return McpTools.Text( GetMobiles( McpTools.GetInt( args, "range" ) ?? 10 ) );
                    case "getItems":
                        return McpTools.Text( GetItems( McpTools.GetInt( args, "range" ) ?? 10 ) );
                    case "getContainer":
                        return McpTools.Text( GetContainer( McpTools.RequireString( args, "serial" ) ) );
                    case "getJournal":
                        return McpTools.Text( GetJournal( McpTools.GetString( args, "filter" ), McpTools.GetInt( args, "count" ) ) );
                    default:
                        return null;
                }
            }
            catch ( Exception e )
            {
                return McpTools.Error( e.Message );
            }
        }

        private static string GetPlayer()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                throw new InvalidOperationException( "Not connected - no player information available." );
            }

            JObject result = new JObject
            {
                ["name"] = player.Name,
                ["serial"] = $"0x{player.Serial:x8}",
                ["hits"] = player.Hits,
                ["hitsMax"] = player.HitsMax,
                ["mana"] = player.Mana,
                ["manaMax"] = player.ManaMax,
                ["stamina"] = player.Stamina,
                ["staminaMax"] = player.StaminaMax,
                ["strength"] = player.Strength,
                ["dexterity"] = player.Dex,
                ["intelligence"] = player.Int,
                ["gold"] = player.Gold,
                ["weight"] = player.Weight,
                ["weightMax"] = player.WeightMax,
                ["followers"] = player.Followers,
                ["followersMax"] = player.FollowersMax,
                ["tithingPoints"] = player.TithingPoints,
                ["luck"] = player.Luck,
                ["x"] = player.X,
                ["y"] = player.Y,
                ["z"] = player.Z,
                ["map"] = player.Map.ToString(),
                ["hue"] = player.Hue,
                ["notoriety"] = player.Notoriety.ToString()
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetBackpack( string filter )
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                throw new InvalidOperationException( "Not connected - no player information available." );
            }

            Item backpack = player.Backpack;

            if ( backpack == null )
            {
                throw new InvalidOperationException( "Backpack not found." );
            }

            ItemCollection container = backpack.Container;

            if ( container == null )
            {
                throw new InvalidOperationException( "Backpack is not open - no contents available." );
            }

            IEnumerable<Item> items = container.GetItems() ?? Array.Empty<Item>();

            if ( !string.IsNullOrEmpty( filter ) )
            {
                items = items.Where( i => i.Name?.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) >= 0 );
            }

            JArray array = new JArray();

            foreach ( Item item in items )
            {
                array.Add( new JObject
                {
                    ["name"] = item.Name,
                    ["serial"] = $"0x{item.Serial:x8}",
                    ["graphic"] = $"0x{item.ID:x4}",
                    ["hue"] = item.Hue,
                    ["count"] = item.Count
                } );
            }

            JObject result = new JObject
            {
                ["backpackSerial"] = $"0x{backpack.Serial:x8}",
                ["itemCount"] = array.Count,
                ["items"] = array
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetTileInfo( int x, int y, int? map )
        {
            int mapIndex = map ?? ( Engine.Player != null ? (int) Engine.Player.Map : 0 );

            if ( mapIndex < 0 || mapIndex > 5 )
            {
                throw new InvalidOperationException( "Map must be between 0 and 5." );
            }

            LandTile landTile = MapInfo.GetLandTile( mapIndex, x, y );
            StaticTile[] statics = Statics.GetStatics( mapIndex, x, y ) ?? Array.Empty<StaticTile>();

            JObject land = new JObject
            {
                ["id"] = $"0x{landTile.ID:x4}",
                ["name"] = landTile.Name,
                ["z"] = landTile.Z,
                ["flags"] = landTile.Flags.ToString()
            };

            JArray staticsArray = new JArray();

            foreach ( StaticTile tile in statics )
            {
                staticsArray.Add( new JObject
                {
                    ["id"] = $"0x{tile.ID:x4}",
                    ["name"] = tile.Name,
                    ["z"] = tile.Z,
                    ["hue"] = tile.Hue,
                    ["flags"] = tile.Flags.ToString(),
                    ["weight"] = tile.Weight,
                    ["height"] = tile.Height
                } );
            }

            JObject result = new JObject
            {
                ["x"] = x,
                ["y"] = y,
                ["map"] = ( (Map) mapIndex ).ToString(),
                ["land"] = land,
                ["statics"] = staticsArray
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string ListGumps()
        {
            JArray array = new JArray();

            foreach ( Gump gump in GetGumps() )
            {
                array.Add( new JObject
                {
                    ["id"] = $"0x{gump.ID:x8}",
                    ["serial"] = $"0x{gump.Serial:x8}",
                    ["x"] = gump.X,
                    ["y"] = gump.Y
                } );
            }

            return JsonConvert.SerializeObject( array, Formatting.Indented );
        }

        private static string GetGumpLayout( string id, string serial )
        {
            Gump gump = null;

            if ( !string.IsNullOrEmpty( id ) && McpTools.TryParseInt( id, out int idValue ) )
            {
                Engine.Gumps.GetGump( (uint) idValue, out gump );
            }
            else if ( !string.IsNullOrEmpty( serial ) && McpTools.TryParseInt( serial, out int serialValue ) )
            {
                Engine.Gumps.FindGump( serialValue, out gump );
            }
            else
            {
                Gump[] gumps = GetGumps();

                if ( gumps.Length == 1 )
                {
                    gump = gumps[0];
                }
            }

            if ( gump == null )
            {
                throw new InvalidOperationException( "Gump not found. Use listGumps to find an id or serial." );
            }

            JArray elements = new JArray();

            GumpElement[] gumpElements;

            try
            {
                gumpElements = gump.GumpElements ?? Array.Empty<GumpElement>();
            }
            catch
            {
                gumpElements = Array.Empty<GumpElement>();
            }

            foreach ( GumpElement element in gumpElements )
            {
                elements.Add( new JObject
                {
                    ["x"] = element.X,
                    ["y"] = element.Y,
                    ["type"] = element.Type.ToString(),
                    ["cliloc"] = element.Cliloc,
                    ["elementId"] = element.ElementID,
                    ["text"] = element.Text
                } );
            }

            JObject result = new JObject
            {
                ["id"] = $"0x{gump.ID:x8}",
                ["serial"] = $"0x{gump.Serial:x8}",
                ["x"] = gump.X,
                ["y"] = gump.Y,
                ["pageCount"] = gump.Pages?.Length ?? 0,
                ["layout"] = gump.Layout ?? string.Empty,
                ["strings"] = new JArray( gump.Strings ?? Array.Empty<string>() ),
                ["elements"] = elements
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetCliloc( int cliloc )
        {
            string value = Cliloc.GetProperty( cliloc );

            JObject result = new JObject
            {
                ["cliloc"] = cliloc,
                ["text"] = value
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetItemInfo( string serialStr )
        {
            if ( !McpTools.TryParseInt( serialStr, out int serial ) )
            {
                throw new InvalidOperationException( $"Invalid serial: {serialStr}" );
            }

            Entity entity = FindEntity( serial );

            if ( entity == null )
            {
                throw new InvalidOperationException( $"Entity 0x{serial:x8} not found." );
            }

            JObject result = new JObject
            {
                ["name"] = entity.Name,
                ["serial"] = $"0x{entity.Serial:x8}",
                ["graphic"] = $"0x{entity.ID:x4}",
                ["hue"] = entity.Hue,
                ["x"] = entity.X,
                ["y"] = entity.Y,
                ["z"] = entity.Z,
                ["type"] = entity.GetType().Name
            };

            if ( entity is Mobile mobile )
            {
                result["hits"] = mobile.Hits;
                result["hitsMax"] = mobile.HitsMax;
                result["notoriety"] = mobile.Notoriety.ToString();
            }
            else if ( entity is Item item )
            {
                result["count"] = item.Count;

                if ( item.Owner != 0 )
                {
                    result["owner"] = $"0x{item.Owner:x8}";
                }

                result["layer"] = item.Layer.ToString();
            }

            JArray properties = new JArray();

            if ( entity.Properties != null )
            {
                foreach ( Property property in entity.Properties )
                {
                    properties.Add( new JObject
                    {
                        ["cliloc"] = property.Cliloc,
                        ["text"] = property.Text,
                        ["arguments"] = property.Arguments != null ? new JArray( property.Arguments ) : null
                    } );
                }
            }

            result["properties"] = properties;

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static Entity FindEntity( int serial )
        {
            if ( UOMath.IsMobile( serial ) )
            {
                Mobile mobile = Engine.Mobiles.GetMobile( serial );

                if ( mobile != null )
                {
                    return mobile;
                }

                return Engine.Player != null && Engine.Player.Serial == serial ? Engine.Player : null;
            }

            return Engine.Items.GetItem( serial );
        }

        private static string GetMobiles( int range )
        {
            if ( Engine.Player == null )
            {
                throw new InvalidOperationException( "Not connected - no player information available." );
            }

            IEnumerable<Mobile> mobiles = Engine.Mobiles.GetMobiles() ?? Array.Empty<Mobile>();

            JArray array = new JArray();

            foreach ( Mobile mobile in mobiles.Where( m => m.Distance <= range ) )
            {
                array.Add( new JObject
                {
                    ["name"] = mobile.Name,
                    ["serial"] = $"0x{mobile.Serial:x8}",
                    ["graphic"] = $"0x{mobile.ID:x4}",
                    ["hue"] = mobile.Hue,
                    ["x"] = mobile.X,
                    ["y"] = mobile.Y,
                    ["z"] = mobile.Z,
                    ["distance"] = mobile.Distance,
                    ["hits"] = mobile.Hits,
                    ["hitsMax"] = mobile.HitsMax,
                    ["notoriety"] = mobile.Notoriety.ToString()
                } );
            }

            JObject result = new JObject
            {
                ["range"] = range,
                ["mobileCount"] = array.Count,
                ["mobiles"] = array
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetItems( int range )
        {
            if ( Engine.Player == null )
            {
                throw new InvalidOperationException( "Not connected - no player information available." );
            }

            IEnumerable<Item> items = Engine.Items.GetItems() ?? Array.Empty<Item>();

            JArray array = new JArray();

            foreach ( Item item in items.Where( i => i.Distance <= range ) )
            {
                array.Add( new JObject
                {
                    ["name"] = item.Name,
                    ["serial"] = $"0x{item.Serial:x8}",
                    ["graphic"] = $"0x{item.ID:x4}",
                    ["hue"] = item.Hue,
                    ["x"] = item.X,
                    ["y"] = item.Y,
                    ["z"] = item.Z,
                    ["distance"] = item.Distance,
                    ["count"] = item.Count,
                    ["isContainer"] = item.IsContainer
                } );
            }

            JObject result = new JObject
            {
                ["range"] = range,
                ["itemCount"] = array.Count,
                ["items"] = array
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetContainer( string serialStr )
        {
            if ( !McpTools.TryParseInt( serialStr, out int serial ) )
            {
                throw new InvalidOperationException( $"Invalid serial: {serialStr}" );
            }

            Item container = Engine.Items.GetItem( serial );

            if ( container?.Container == null )
            {
                throw new InvalidOperationException( $"Entity 0x{serial:x8} is not an open container." );
            }

            JArray array = new JArray();

            foreach ( Item item in container.Container.GetItems() )
            {
                array.Add( new JObject
                {
                    ["name"] = item.Name,
                    ["serial"] = $"0x{item.Serial:x8}",
                    ["graphic"] = $"0x{item.ID:x4}",
                    ["hue"] = item.Hue,
                    ["count"] = item.Count,
                    ["x"] = item.X,
                    ["y"] = item.Y
                } );
            }

            JObject result = new JObject
            {
                ["containerSerial"] = $"0x{container.Serial:x8}",
                ["itemCount"] = array.Count,
                ["items"] = array
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetJournal( string filter, int? count )
        {
            JournalEntry[] buffer = Engine.Journal.GetEntireBuffer() ?? Array.Empty<JournalEntry>();

            List<JournalEntry> entries = new List<JournalEntry>( buffer );

            if ( !string.IsNullOrEmpty( filter ) )
            {
                entries = entries.Where( e => e.Text?.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) >= 0 )
                    .ToList();
            }

            int max = count ?? 20;

            if ( entries.Count > max )
            {
                entries = entries.Skip( entries.Count - max ).ToList();
            }

            JArray array = new JArray();

            foreach ( JournalEntry entry in entries )
            {
                array.Add( new JObject
                {
                    ["text"] = entry.Text,
                    ["author"] = entry.Name,
                    ["serial"] = entry.Serial != 0 ? $"0x{entry.Serial:x8}" : null,
                    ["cliloc"] = entry.Cliloc,
                    ["speechType"] = entry.SpeechType.ToString()
                } );
            }

            JObject result = new JObject
            {
                ["entryCount"] = array.Count,
                ["entries"] = array
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static Gump[] GetGumps()
        {
            if ( Engine.Gumps.GetGumps( out Gump[] gumps ) )
            {
                return gumps;
            }

            return Array.Empty<Gump>();
        }
    }
}
