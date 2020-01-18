using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;

namespace ClassicAssist.Data.Regions
{
    public class Region
    {
        public RegionAttributes Attributes { get; set; }
        public int Height => Y2 - Y1;
        public int Map { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public int Width => X2 - X1;
        public int X1 { get; set; }
        public int X2 { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }

        public override bool Equals( object obj )
        {
            if ( !( obj is Region region ) )
            {
                return false;
            }

            return region.X1 == X1 && region.X2 == X2 && region.Y1 == Y1 && region.Y2 == Y2 && region.Map == Map &&
                   region.Priority == Priority;
        }

        protected bool Equals( Region other )
        {
            return X1 == other.X1 && Y1 == other.Y1 && X2 == other.X2 && Y2 == other.Y2 && Map == other.Map &&
                   Priority == other.Priority;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = ( hashCode * 397 ) ^ X1;
                hashCode = ( hashCode * 397 ) ^ Y1;
                hashCode = ( hashCode * 397 ) ^ X2;
                hashCode = ( hashCode * 397 ) ^ Y2;
                hashCode = ( hashCode * 397 ) ^ Map;
                hashCode = ( hashCode * 397 ) ^ Priority;
                hashCode = ( hashCode * 397 ) ^ (int) Attributes;

                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Name} - {Attributes}";
        }
    }

    public class RegionList
    {
        public List<Region> Regions { get; set; } = new List<Region>();
    }

    public static class Regions
    {
        private static readonly Region[] _defaultRegions =
        {
            new Region
            {
                X1 = 0,
                Y1 = 0,
                X2 = 7168,
                Y2 = 4096,
                Map = 0,
                Attributes = RegionAttributes.Wilderness,
                Name = "Felucca",
                Priority = -1
            },
            new Region
            {
                X1 = 0,
                Y1 = 0,
                X2 = 7168,
                Y2 = 4096,
                Map = 1,
                Attributes = RegionAttributes.Wilderness,
                Name = "Trammel",
                Priority = -1
            },
            new Region
            {
                X1 = 0,
                Y1 = 0,
                X2 = 2304,
                Y2 = 1600,
                Map = 2,
                Attributes = RegionAttributes.Wilderness,
                Name = "Ilshenar",
                Priority = -1
            },
            new Region
            {
                X1 = 0,
                Y1 = 0,
                X2 = 2560,
                Y2 = 2048,
                Map = 3,
                Attributes = RegionAttributes.Wilderness,
                Name = "Malas",
                Priority = -1
            },
            new Region
            {
                X1 = 0,
                Y1 = 0,
                X2 = 1448,
                Y2 = 1448,
                Map = 4,
                Attributes = RegionAttributes.Wilderness,
                Name = "Tokuno",
                Priority = -1
            },
            new Region
            {
                X1 = 0,
                Y1 = 0,
                X2 = 1280,
                Y2 = 4096,
                Map = 5,
                Attributes = RegionAttributes.Wilderness,
                Name = "TerMur",
                Priority = -1
            },
            new Region
            {
                X1 = 5271,
                Y1 = 1159,
                X2 = 5311,
                Y2 = 1191,
                Map = 0,
                Attributes = RegionAttributes.Jail,
                Name = "Jail",
                Priority = byte.MaxValue
            },
            new Region
            {
                X1 = 5271,
                Y1 = 1159,
                X2 = 5311,
                Y2 = 1191,
                Map = 1,
                Attributes = RegionAttributes.Jail,
                Name = "Jail",
                Priority = byte.MaxValue
            },
            new Region
            {
                X1 = 2720,
                Y1 = 2089,
                X2 = 2726,
                Y2 = 2094,
                Map = 0,
                Attributes = RegionAttributes.Jail,
                Name = "OSI Unattended Macroing Jail",
                Priority = byte.MaxValue
            },
            new Region
            {
                X1 = 2720,
                Y1 = 2089,
                X2 = 2726,
                Y2 = 2094,
                Map = 1,
                Attributes = RegionAttributes.Jail,
                Name = "OSI Unattended Macroing Jail",
                Priority = byte.MaxValue
            }
        };

        private static readonly Lazy<List<Region>> _regions = new Lazy<List<Region>>( LoadRegions );

        private static List<Region> LoadRegions()
        {
            JsonSerializer serializer = new JsonSerializer();

            using ( StreamReader sr = new StreamReader( Path.Combine( Engine.StartupPath, "Data", "Regions.json" ) ) )
            {
                using ( JsonTextReader reader = new JsonTextReader( sr ) )
                {
                    RegionList regionList = serializer.Deserialize<RegionList>( reader );

                    if ( regionList != null )
                    {
                        return regionList.Regions;
                    }
                }
            }

            return new List<Region>( _defaultRegions );
        }

        public static void Add( Region region )
        {
            if ( !_regions.Value.Contains( region ) )
            {
                _regions.Value.Add( region );
            }
        }

        public static bool Contains( string name )
        {
            return _regions.Value.FirstOrDefault( r => r.Name == name ) != null;
        }

        public static Region GetRegion( PlayerMobile player )
        {
            return GetRegion( player.X, player.Y, (int) player.Map );
        }

        public static Region GetRegion( int x, int y, int map )
        {
            IEnumerable<Region> matching =
                _regions.Value.Where( r => x >= r.X1 && y >= r.Y1 && x <= r.X2 && y <= r.Y2 && map == r.Map );

            List<Region> list = matching.ToList();

            list.Sort( ( r1, r2 ) =>
            {
                if ( r1.Priority > r2.Priority )
                {
                    return -1;
                }

                if ( r2.Priority > r1.Priority )
                {
                    return 1;
                }

                int area1 = r1.Width * r1.Height;
                int area2 = r2.Width * r2.Height;

                if ( area1 < area2 )
                {
                    return -1;
                }

                return area1 == area2 ? 0 : 1;
            } );

            return list.Count == 0 ? null : list[0];
        }

        public static Region GetRegion( this Entity entity )
        {
            PlayerMobile player = Engine.Player;

            return player == null ? null : GetRegion( entity.X, entity.Y, (int) player.Map );
        }

        public static void Remove( Region region )
        {
            if ( _regions.Value.Contains( region ) )
            {
                _regions.Value.Remove( region );
            }
        }
    }
}