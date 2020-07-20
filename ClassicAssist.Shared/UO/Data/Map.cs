using System;
using System.IO;
using System.Linq;
using ClassicAssist.Shared;

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