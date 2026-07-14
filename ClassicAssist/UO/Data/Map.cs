using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Data
{
    public enum Map
    {
        Felucca,
        Trammel,
        Ilshenar,
        Malas,
        Tokuno,
        Ter_Mur
    }

    public static class MapInfo
    {
        private static string _dataPath;

        private static readonly UOPIndex[] _uopIndex = new UOPIndex[6];

        private static int[,] _defaultMapSize { get; } =
        {
            { 7168, 4096 }, { 7168, 4096 }, { 2304, 1600 }, { 2560, 2048 }, { 1448, 1448 }, { 1280, 4096 }
        };

        public static void Initialize( string dataPath )
        {
            _dataPath = dataPath;
        }

        public static LandTile GetLandTile( int map, int x, int y )
        {
            int blockIndex = x / 8 * ( _defaultMapSize[map, 1] / 8 ) + y / 8;
            int cellX = x % 8;
            int cellY = y % 8;
            string fileName = GetMapFilename( map );

            if ( string.IsNullOrEmpty( fileName ) )
            {
                return new LandTile();
            }

            using ( FileStream fileStream = File.OpenRead( fileName ) )
            {
                if ( fileName.EndsWith( ".uop", StringComparison.InvariantCultureIgnoreCase ) &&
                     _uopIndex[map] == null )
                {
                    _uopIndex[map] = new UOPIndex( File.OpenRead( fileName ) );
                }

                int offset = blockIndex * 196 + 4 + cellY * 24 + cellX * 3;

                if ( _uopIndex[map] != null )
                {
                    offset = _uopIndex[map].Lookup( offset );
                }

                fileStream.Seek( offset, SeekOrigin.Begin );

                using ( BinaryReader binaryReader = new BinaryReader( fileStream ) )
                {
                    int id = binaryReader.ReadUInt16();
                    LandTile landTile = TileData.GetLandTile( id );
                    landTile.ID = id;
                    landTile.X = x;
                    landTile.Y = y;
                    landTile.Z = binaryReader.ReadSByte();

                    return landTile;
                }
            }
        }

        public static (int, int) GetMapSurface( int map, int x, int y )
        {
            LandTile landTile = GetLandTile( map, x, y );

            StaticTile[] staticTiles = Statics.GetStatics( map, x, y );

            if ( staticTiles == null || staticTiles.Length == 0 )
            {
                return ( landTile.Z, 0 );
            }

            StaticTile staticTile = staticTiles
                .Where( i => i.Flags.HasFlag( TileFlags.Surface ) && Math.Abs( Engine.Player.Z - i.Z ) < 20 )
                .OrderByDescending( i => i.Z ).FirstOrDefault();

            if ( staticTile.ID == 0 && landTile.Flags.HasFlag( TileFlags.Impassable ) )
            {
                staticTile = staticTiles
                    .Where( i => i.Flags.HasFlag( TileFlags.Wet ) && Math.Abs( Engine.Player.Z - i.Z ) < 20 )
                    .OrderByDescending( i => i.Z ).FirstOrDefault();
            }

            if ( staticTile.ID == 0 )
            {
                return ( landTile.Z, 0 );
            }

            return ( staticTile.Z + staticTile.Height, staticTile.ID );
        }

        public static int GetAverageZ( int map, int x, int y )
        {
            GetAverageZ( map, x, y, out _, out int avg, out _ );

            return avg;
        }

        public static void GetAverageZ( int map, int x, int y, out int z, out int avg, out int top )
        {
            int zTop = GetLandTile( map, x, y ).Z;
            int zLeft = GetLandTile( map, x, y + 1 ).Z;
            int zRight = GetLandTile( map, x + 1, y ).Z;
            int zBottom = GetLandTile( map, x + 1, y + 1 ).Z;

            z = zTop;

            if ( zLeft < z )
            {
                z = zLeft;
            }

            if ( zRight < z )
            {
                z = zRight;
            }

            if ( zBottom < z )
            {
                z = zBottom;
            }

            top = zTop;

            if ( zLeft > top )
            {
                top = zLeft;
            }

            if ( zRight > top )
            {
                top = zRight;
            }

            if ( zBottom > top )
            {
                top = zBottom;
            }

            avg = Math.Abs( zTop - zBottom ) > Math.Abs( zLeft - zRight )
                ? FloorAverage( zLeft, zRight )
                : FloorAverage( zTop, zBottom );
        }

        private static int FloorAverage( int a, int b )
        {
            int v = a + b;

            if ( v < 0 )
            {
                --v;
            }

            return v / 2;
        }

        public static bool InRange( int p1X, int p1Y, int p2X, int p2Y, int range )
        {
            return p1X >= p2X - range && p1X <= p2X + range && p1Y >= p2Y - range && p1Y <= p2Y + range;
        }

        private static void GetItemsAtLocation( int x, int y, List<Item> results )
        {
            foreach ( Item item in Engine.Items )
            {
                if ( item.X == x && item.Y == y )
                {
                    results.Add( item );
                }
            }
        }

        /// <summary>
        ///     Determines whether an item of the given art id will fit on the ground at ( x, y ) using
        ///     the ModernUO Item.DropToWorld fit/stack algorithm, from the player's location. Returns the
        ///     resolved drop Z via <paramref name="dropZ" />. NOTE: the line of sight check performed by
        ///     the server is intentionally omitted here.
        /// </summary>
        public static bool ItemCanFit( int map, int x, int y, int itemId, out int dropZ )
        {
            dropZ = 0;

            if ( Engine.Player == null )
            {
                return false;
            }

            return ItemCanFit( map, x, y, itemId, Engine.Player.X, Engine.Player.Y, Engine.Player.Z, out dropZ );
        }

        public static bool ItemCanFit( int map, int x, int y, int itemId, int fromX, int fromY, int fromZ,
            out int dropZ )
        {
            dropZ = 0;

            if ( x < 0 || y < 0 )
            {
                return false;
            }

            if ( !InRange( fromX, fromY, x, y, 2 ) )
            {
                return false;
            }

            int maxZ = fromZ + 16;

            LandTile landTile = GetLandTile( map, x, y );
            GetAverageZ( map, x, y, out int landZ, out int landAvg, out _ );
            TileFlags landFlags = landTile.Flags;
            bool landImpassable = ( landFlags & TileFlags.Impassable ) != 0;

            int z = int.MinValue;

            // Passable, non-ignored land counts as a base surface.
            if ( !landTile.Ignored && !landImpassable && landAvg <= maxZ )
            {
                z = landAvg;
            }

            StaticTile[] statics = Statics.GetStatics( map, x, y ) ?? Array.Empty<StaticTile>();

            foreach ( StaticTile tile in statics )
            {
                if ( !tile.Flags.HasFlag( TileFlags.Surface ) )
                {
                    continue;
                }

                int calcHeight = tile.Flags.HasFlag( TileFlags.Bridge ) ? tile.Height / 2 : tile.Height;
                int top = tile.Z + calcHeight;

                if ( top > maxZ || top < z )
                {
                    continue;
                }

                z = top;
            }

            List<Item> items = new List<Item>();
            GetItemsAtLocation( x, y, items );

            foreach ( Item item in items )
            {
                StaticTile idata = TileData.GetStaticTile( item.ID );

                if ( !idata.Flags.HasFlag( TileFlags.Surface ) )
                {
                    continue;
                }

                int calcHeight = idata.Flags.HasFlag( TileFlags.Bridge ) ? idata.Height / 2 : idata.Height;
                int top = item.Z + calcHeight;

                if ( top <= maxZ && top >= z )
                {
                    z = top;
                }
            }

            if ( z == int.MinValue || z > maxZ )
            {
                return false;
            }

            int surfaceZ = z;

            // 20-bit vertical occupancy mask for the space directly above the surface.
            int openSlots = ( 1 << 20 ) - 1;

            foreach ( StaticTile tile in statics )
            {
                int calcHeight = tile.Flags.HasFlag( TileFlags.Bridge ) ? tile.Height / 2 : tile.Height;
                int checkZ = tile.Z;
                int checkTop = checkZ + calcHeight;

                if ( checkTop == checkZ && !tile.Flags.HasFlag( TileFlags.Surface ) )
                {
                    ++checkTop;
                }

                int zStart = Math.Max( checkZ - z, 0 );
                int zEnd = Math.Min( checkTop - z, 19 );

                if ( zStart >= 20 || zEnd < 0 )
                {
                    continue;
                }

                int bitCount = zEnd - zStart;
                openSlots &= ~( ( ( 1 << bitCount ) - 1 ) << zStart );
            }

            foreach ( Item item in items )
            {
                StaticTile idata = TileData.GetStaticTile( item.ID );
                int calcHeight = idata.Flags.HasFlag( TileFlags.Bridge ) ? idata.Height / 2 : idata.Height;
                int checkZ = item.Z;
                int checkTop = checkZ + calcHeight;

                if ( checkTop == checkZ && !idata.Flags.HasFlag( TileFlags.Surface ) )
                {
                    ++checkTop;
                }

                int zStart = Math.Max( checkZ - z, 0 );
                int zEnd = Math.Min( checkTop - z, 19 );

                if ( zStart >= 20 || zEnd < 0 )
                {
                    continue;
                }

                int bitCount = zEnd - zStart;
                openSlots &= ~( ( ( 1 << bitCount ) - 1 ) << zStart );
            }

            StaticTile itemData = TileData.GetStaticTile( itemId );
            int height = itemData.Height;

            if ( height == 0 )
            {
                ++height;
            }

            if ( height > 30 )
            {
                height = 30;
            }

            int match = ( 1 << height ) - 1;
            bool okay = false;

            for ( int i = 0; i < 20; ++i )
            {
                if ( i + height > 20 )
                {
                    match >>= 1;
                }

                okay = ( ( openSlots >> i ) & match ) == match;

                if ( okay )
                {
                    z += i;

                    break;
                }
            }

            if ( !okay )
            {
                return false;
            }

            // The resolved drop height must stay within reach of the origin. When the only gap
            // sits on top of a full stack ( e.g. z = 17 above a stack topping out at 16 ) the
            // server refuses the drop, so reject the tile and let the caller try another one.
            if ( z > maxZ )
            {
                return false;
            }

            height = itemData.Height;

            if ( height == 0 )
            {
                ++height;
            }

            // Final intersection re-checks (server post-placement validation).
            if ( landAvg > z && z + height > landZ )
            {
                return false;
            }

            if ( landImpassable && landAvg > surfaceZ && z + height > landZ )
            {
                return false;
            }

            foreach ( StaticTile tile in statics )
            {
                bool surface = tile.Flags.HasFlag( TileFlags.Surface );
                bool impassable = tile.Flags.HasFlag( TileFlags.Impassable );
                int calcHeight = tile.Flags.HasFlag( TileFlags.Bridge ) ? tile.Height / 2 : tile.Height;
                int checkZ = tile.Z;
                int checkTop = checkZ + calcHeight;

                if ( checkTop > z && z + height > checkZ )
                {
                    return false;
                }

                if ( ( surface || impassable ) && checkTop > surfaceZ && z + height > checkZ )
                {
                    return false;
                }
            }

            foreach ( Item item in items )
            {
                StaticTile idata = TileData.GetStaticTile( item.ID );
                int calcHeight = idata.Flags.HasFlag( TileFlags.Bridge ) ? idata.Height / 2 : idata.Height;

                if ( item.Z + calcHeight > z && z + height > item.Z )
                {
                    return false;
                }
            }

            // An item cannot be dropped on a tile occupied by a mobile.
            foreach ( Mobile mobile in Engine.Mobiles )
            {
                if ( mobile.X == x && mobile.Y == y && mobile.Z + 16 > z && z + height > mobile.Z )
                {
                    return false;
                }
            }

            // NOTE: the server also performs a line of sight check here; intentionally omitted.

            dropZ = z;

            return true;
        }

        private static string GetMapFilename( int map )
        {
            if ( string.IsNullOrEmpty( _dataPath ) )
            {
                return null;
            }

            string uopFilename = Path.Combine( _dataPath, $"map{map}LegacyMUL.uop" );

            if ( File.Exists( uopFilename ) )
            {
                return uopFilename;
            }

            string mulFilename = Path.Combine( _dataPath, $"map{map}.mul" );

            if ( File.Exists( mulFilename ) )
            {
                return mulFilename;
            }

            return null;
        }
    }
}