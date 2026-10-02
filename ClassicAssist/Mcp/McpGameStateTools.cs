using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.BuffIcons;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Skills;
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
                    Description = "Get the contents of the player's backpack. The backpack must already be open; use invokeCommand('UseObject') then invokeCommand('WaitForContents') to open it.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["filter"] = McpTools.StringProperty( "Optional case-insensitive substring to filter item names." ),
                            ["limit"] = McpTools.IntegerProperty( "Maximum number of items to return (default 200)." ),
                            ["offset"] = McpTools.IntegerProperty( "Number of items to skip (default 0)." )
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
                        new JObject
                        {
                            ["range"] = McpTools.IntegerProperty( "Maximum distance in tiles from the player (default 10)." ),
                            ["limit"] = McpTools.IntegerProperty( "Maximum number of mobiles to return (default 200)." ),
                            ["offset"] = McpTools.IntegerProperty( "Number of mobiles to skip (default 0)." )
                        } )
                },
                new McpTool
                {
                    Name = "getItems",
                    Description = "Get the list of items within a certain distance of the player.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["range"] = McpTools.IntegerProperty( "Maximum distance in tiles from the player (default 10)." ),
                            ["limit"] = McpTools.IntegerProperty( "Maximum number of items to return (default 200)." ),
                            ["offset"] = McpTools.IntegerProperty( "Number of items to skip (default 0)." )
                        } )
                },
                new McpTool
                {
                    Name = "getContainer",
                    Description = "Get the contents of a container (e.g. bank box or backpack) by serial. The container must already be open; use invokeCommand('UseObject') then invokeCommand('WaitForContents') to open it.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["serial"] = McpTools.StringProperty( "The container serial (decimal or 0x hex)." ),
                            ["limit"] = McpTools.IntegerProperty( "Maximum number of items to return (default 200)." ),
                            ["offset"] = McpTools.IntegerProperty( "Number of items to skip (default 0)." )
                        }, "serial" )
                },
                new McpTool
                {
                    Name = "getJournal",
                    Description = "Get recent game journal entries (system messages, speech, macro output).",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["filter"] = McpTools.StringProperty( "Optional case-insensitive substring to filter entry text." ),
                            ["count"] = McpTools.IntegerProperty( "Maximum number of most recent entries to return (default 20)." ),
                            ["offset"] = McpTools.IntegerProperty( "Number of (filtered) entries to skip from the most recent end (default 0)." )
                        } )
                },
                new McpTool
                {
                    Name = "getBuffs",
                    Description = "Get the currently active buff/debuff icons with their remaining duration.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["filter"] = McpTools.StringProperty( "Optional case-insensitive substring to filter buff names." ) } )
                },
                new McpTool
                {
                    Name = "getSkills",
                    Description = "Get the player's skills (value, base, cap, delta and lock status).",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["filter"] = McpTools.StringProperty( "Optional case-insensitive substring to filter skill names." ) } )
                },
                new McpTool
                {
                    Name = "getSkill",
                    Description = "Get a single skill by name (case-insensitive substring match).",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["name"] = McpTools.StringProperty( "The skill name." ) }, "name" )
                },
                new McpTool
                {
                    Name = "getTarget",
                    Description = "Get the current target cursor state (pending target serial/type/flags) and the last/enemy/friend target serials.",
                    InputSchema = McpTools.ObjectSchema()
                },
                new McpTool
                {
                    Name = "getHotkeys",
                    Description = "List the configured hotkey entries (name, type, bound shortcut and whether an action is attached).",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["filter"] = McpTools.StringProperty( "Optional case-insensitive substring to filter hotkey names." ) } )
                },
                new McpTool
                {
                    Name = "executeHotkey",
                    Description = "Execute a configured hotkey entry by name (case-insensitive; first match with an action). Sends client input, so use with care.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject { ["name"] = McpTools.StringProperty( "The hotkey entry name." ) }, "name" )
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
                        return McpTools.Text( GetBackpack( McpTools.GetString( args, "filter" ),
                            McpTools.GetInt( args, "limit" ), McpTools.GetInt( args, "offset" ) ) );
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
                        return McpTools.Text( GetMobiles( McpTools.GetInt( args, "range" ) ?? 10,
                            McpTools.GetInt( args, "limit" ), McpTools.GetInt( args, "offset" ) ) );
                    case "getItems":
                        return McpTools.Text( GetItems( McpTools.GetInt( args, "range" ) ?? 10,
                            McpTools.GetInt( args, "limit" ), McpTools.GetInt( args, "offset" ) ) );
                    case "getContainer":
                        return McpTools.Text( GetContainer( McpTools.RequireString( args, "serial" ),
                            McpTools.GetInt( args, "limit" ), McpTools.GetInt( args, "offset" ) ) );
                    case "getJournal":
                        return McpTools.Text( GetJournal( McpTools.GetString( args, "filter" ), McpTools.GetInt( args, "count" ),
                            McpTools.GetInt( args, "offset" ) ) );
                    case "getBuffs":
                        return McpTools.Text( GetBuffs( McpTools.GetString( args, "filter" ) ) );
                    case "getSkills":
                        return McpTools.Text( GetSkills( McpTools.GetString( args, "filter" ) ) );
                    case "getSkill":
                        return McpTools.Text( GetSkill( McpTools.RequireString( args, "name" ) ) );
                    case "getTarget":
                        return McpTools.Text( GetTarget() );
                    case "getHotkeys":
                        return McpTools.Text( GetHotkeys( McpTools.GetString( args, "filter" ) ) );
                    case "executeHotkey":
                        return McpTools.Text( ExecuteHotkey( McpTools.RequireString( args, "name" ) ) );
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

        private static string GetBackpack( string filter, int? limit, int? offset )
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
                throw new InvalidOperationException(
                    "Backpack is not open - no contents available. Use invokeCommand('UseObject') then invokeCommand('WaitForContents') first." );
            }

            IEnumerable<Item> items = container.GetItems() ?? Array.Empty<Item>();

            if ( !string.IsNullOrEmpty( filter ) )
            {
                items = items.Where( i => i.Name?.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) >= 0 );
            }

            (List<Item> page, int total, int offsetValue, int limitValue) = McpTools.Paginate( items, limit, offset );

            JArray array = new JArray();

            foreach ( Item item in page )
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

            McpTools.WithPageInfo( result, total, offsetValue, limitValue, array.Count );

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

        private static string GetMobiles( int range, int? limit, int? offset )
        {
            if ( Engine.Player == null )
            {
                throw new InvalidOperationException( "Not connected - no player information available." );
            }

            IEnumerable<Mobile> mobiles = Engine.Mobiles.GetMobiles() ?? Array.Empty<Mobile>();

            (List<Mobile> page, int total, int offsetValue, int limitValue) =
                McpTools.Paginate( mobiles.Where( m => m.Distance <= range ), limit, offset );

            JArray array = new JArray();

            foreach ( Mobile mobile in page )
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

            McpTools.WithPageInfo( result, total, offsetValue, limitValue, array.Count );

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetItems( int range, int? limit, int? offset )
        {
            if ( Engine.Player == null )
            {
                throw new InvalidOperationException( "Not connected - no player information available." );
            }

            IEnumerable<Item> items = Engine.Items.GetItems() ?? Array.Empty<Item>();

            (List<Item> page, int total, int offsetValue, int limitValue) =
                McpTools.Paginate( items.Where( i => i.Distance <= range ), limit, offset );

            JArray array = new JArray();

            foreach ( Item item in page )
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

            McpTools.WithPageInfo( result, total, offsetValue, limitValue, array.Count );

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetContainer( string serialStr, int? limit, int? offset )
        {
            if ( !McpTools.TryParseInt( serialStr, out int serial ) )
            {
                throw new InvalidOperationException( $"Invalid serial: {serialStr}" );
            }

            Item container = Engine.Items.GetItem( serial );

            if ( container?.Container == null )
            {
                throw new InvalidOperationException(
                    $"Entity 0x{serial:x8} is not an open container. Use invokeCommand('UseObject') then invokeCommand('WaitForContents') first." );
            }

            IEnumerable<Item> items = container.Container.GetItems() ?? Array.Empty<Item>();

            (List<Item> page, int total, int offsetValue, int limitValue) = McpTools.Paginate( items, limit, offset );

            JArray array = new JArray();

            foreach ( Item item in page )
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

            McpTools.WithPageInfo( result, total, offsetValue, limitValue, array.Count );

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetJournal( string filter, int? count, int? offset )
        {
            JournalEntry[] buffer = Engine.Journal.GetEntireBuffer() ?? Array.Empty<JournalEntry>();

            List<JournalEntry> entries = new List<JournalEntry>( buffer );

            if ( !string.IsNullOrEmpty( filter ) )
            {
                entries = entries.Where( e => e.Text?.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) >= 0 )
                    .ToList();
            }

            int total = entries.Count;
            int take = Math.Max( 0, count ?? 20 );
            int skip = Math.Max( 0, offset ?? 0 );

            int end = Math.Max( 0, total - skip );
            int start = Math.Max( 0, end - take );
            List<JournalEntry> page = end > start ? entries.GetRange( start, end - start ) : new List<JournalEntry>();

            JArray array = new JArray();

            foreach ( JournalEntry entry in page )
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

            McpTools.WithPageInfo( result, total, skip, take, array.Count );

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetBuffs( string filter )
        {
            BuffIconManager manager = BuffIconManager.GetInstance();

            string[] names = manager.GetEnabledNames() ?? Array.Empty<string>();

            JArray array = new JArray();

            foreach ( string name in names )
            {
                if ( string.IsNullOrEmpty( name ) )
                {
                    continue;
                }

                if ( !string.IsNullOrEmpty( filter ) && name.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) < 0 )
                {
                    continue;
                }

                BuffIconData data = manager.GetDataByName( name );

                array.Add( new JObject
                {
                    ["name"] = name,
                    ["id"] = data?.ID ?? -1,
                    ["remainingMs"] = (long) manager.BuffTime( name )
                } );
            }

            JObject result = new JObject
            {
                ["buffCount"] = array.Count,
                ["buffs"] = array
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetSkills( string filter )
        {
            JArray array = OnUi( () =>
            {
                ObservableCollection<SkillEntry> items = SkillManager.GetInstance().Items;

                IEnumerable<SkillEntry> entries = items ?? Enumerable.Empty<SkillEntry>();

                if ( !string.IsNullOrEmpty( filter ) )
                {
                    entries = entries.Where( s =>
                        s.Skill.Name?.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) >= 0 );
                }

                return new JArray( entries.OrderBy( s => s.Skill.Name ).Select( SkillToJObject ) );
            } );

            JObject result = new JObject
            {
                ["skillCount"] = array.Count,
                ["skills"] = array
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetSkill( string name )
        {
            JObject result = OnUi( () =>
            {
                ObservableCollection<SkillEntry> items = SkillManager.GetInstance().Items;

                SkillEntry entry = ( items ?? Enumerable.Empty<SkillEntry>() ).FirstOrDefault( s =>
                    s.Skill.Name?.IndexOf( name, StringComparison.OrdinalIgnoreCase ) >= 0 );

                return entry == null ? null : SkillToJObject( entry );
            } );

            if ( result == null )
            {
                throw new InvalidOperationException( $"Skill '{name}' not found." );
            }

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static JObject SkillToJObject( SkillEntry entry )
        {
            return new JObject
            {
                ["name"] = entry.Skill.Name,
                ["id"] = entry.Skill.ID,
                ["value"] = entry.Value,
                ["base"] = entry.Base,
                ["cap"] = entry.Cap,
                ["delta"] = entry.Delta,
                ["lock"] = entry.LockStatus.ToString()
            };
        }

        private static string GetTarget()
        {
            PlayerMobile player = Engine.Player;

            JObject result = new JObject
            {
                ["exists"] = Engine.TargetExists,
                ["serial"] = Engine.TargetExists ? $"0x{Engine.TargetSerial:x8}" : null,
                ["type"] = Engine.TargetType.ToString(),
                ["flags"] = Engine.TargetFlags.ToString(),
                ["lastTargetSerial"] =
                    player != null && player.LastTargetSerial != 0 ? $"0x{player.LastTargetSerial:x8}" : null,
                ["enemyTargetSerial"] =
                    player != null && player.EnemyTargetSerial != 0 ? $"0x{player.EnemyTargetSerial:x8}" : null,
                ["friendTargetSerial"] =
                    player != null && player.FriendTargetSerial != 0 ? $"0x{player.FriendTargetSerial:x8}" : null
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string GetHotkeys( string filter )
        {
            JArray array = OnUi( () =>
            {
                JArray results = new JArray();

                foreach ( HotkeyEntry entry in EnumerateHotkeys() )
                {
                    if ( !string.IsNullOrEmpty( filter ) &&
                         ( entry.Name?.IndexOf( filter, StringComparison.OrdinalIgnoreCase ) ?? -1 ) < 0 )
                    {
                        continue;
                    }

                    results.Add( HotkeyToJObject( entry ) );
                }

                return results;
            } );

            JObject result = new JObject
            {
                ["hotkeyCount"] = array.Count,
                ["hotkeys"] = array
            };

            return JsonConvert.SerializeObject( result, Formatting.Indented );
        }

        private static string ExecuteHotkey( string name )
        {
            HotkeyEntry target = OnUi( () => EnumerateHotkeys().FirstOrDefault( e =>
                e.Action != null && string.Equals( e.Name, name, StringComparison.OrdinalIgnoreCase ) ) );

            if ( target == null )
            {
                throw new InvalidOperationException( $"Hotkey '{name}' not found or has no action." );
            }

            AliasCommands.SetDefaultAliases();

            Task.Run( () => target.Action( target, null ) );

            return $"Executed hotkey '{target.Name}'.";
        }

        private static IEnumerable<HotkeyEntry> EnumerateHotkeys()
        {
            ObservableCollection<HotkeyCommand> categories = HotkeyManager.GetInstance().Items;

            if ( categories == null )
            {
                yield break;
            }

            foreach ( HotkeyCommand category in categories )
            {
                if ( category.Children != null && category.Children.Count > 0 )
                {
                    foreach ( HotkeyEntry child in category.Children )
                    {
                        yield return child;
                    }
                }
                else
                {
                    yield return category;
                }
            }
        }

        private static JObject HotkeyToJObject( HotkeyEntry entry )
        {
            return new JObject
            {
                ["name"] = entry.Name,
                ["type"] = entry.GetType().Name,
                ["key"] = entry.Hotkey?.Key.ToString(),
                ["modifier"] = entry.Hotkey?.Modifier.ToString(),
                ["mouse"] = entry.Hotkey?.Mouse.ToString(),
                ["shortcut"] = entry.Hotkey?.ToString(),
                ["isGlobal"] = entry.IsGlobal,
                ["passToUO"] = entry.PassToUO,
                ["hasAction"] = entry.Action != null
            };
        }

        private static Gump[] GetGumps()
        {
            if ( Engine.Gumps.GetGumps( out Gump[] gumps ) )
            {
                return gumps;
            }

            return Array.Empty<Gump>();
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
