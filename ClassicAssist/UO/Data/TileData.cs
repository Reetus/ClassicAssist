using System;
using System.IO;
using System.Text;

namespace ClassicAssist.UO.Data
{
    public static class TileData
    {
        private static string _dataPath;
        private static readonly Lazy<LandTile[]> _landTiles = new Lazy<LandTile[]>( LoadLandTiles );
        private static readonly Lazy<StaticTile[]> _staticTiles = new Lazy<StaticTile[]>( LoadStaticTiles );

        private static bool _oldFormat;

        public static void Initialize( string dataPath )
        {
            _dataPath = dataPath;
        }

        private static StaticTile[] LoadStaticTiles()
        {
            if ( string.IsNullOrEmpty( _dataPath ) || !Directory.Exists( _dataPath ) )
            {
                return null;
            }

            string fileName = Path.Combine( _dataPath, "tiledata.mul" );

            if ( !File.Exists( fileName ) )
            {
                throw new FileNotFoundException( "File not found.", fileName );
            }

            byte[] fileBytes = File.ReadAllBytes( fileName );

            StaticTile[] staticTiles = new StaticTile[( fileBytes.Length - 428032 ) / 1188 * 32];

            MemoryStream ms = new MemoryStream( fileBytes, false );

            using ( BinaryReader reader = new BinaryReader( ms ) )
            {
                ms.Seek( 36, SeekOrigin.Begin );
                string name = Encoding.ASCII.GetString( reader.ReadBytes( 20 ) ).TrimEnd( '\0' );

                if ( name == "VOID!!!!!!" )
                {
                    _oldFormat = true;
                }

                if ( _oldFormat )
                {
                    const int offset = 428032;

                    ms.Seek( offset, SeekOrigin.Begin );

                    for ( int i = 0; i < 0x4000; ++i )
                    {
                        if ( ( i & 0x1F ) == 0 )
                        {
                            reader.ReadInt32(); // header
                        }

                        staticTiles[i].ID = (ushort) i;
                        staticTiles[i].Flags = (TileFlags) reader.ReadInt32();
                        staticTiles[i].Weight = reader.ReadByte();
                        staticTiles[i].Quality = reader.ReadByte();
                        reader.ReadInt16();
                        reader.ReadByte();
                        staticTiles[i].Quantity = reader.ReadByte();
                        reader.ReadInt16();
                        reader.ReadByte();
                        reader.ReadByte();
                        reader.ReadInt16();
                        staticTiles[i].Height = reader.ReadByte();
                        staticTiles[i].Name = Encoding.ASCII.GetString( reader.ReadBytes( 20 ) ).TrimEnd( '\0' );
                    }
                }
                else
                {
                    const int offset = 493568;

                    ms.Seek( offset, SeekOrigin.Begin );

                    for ( int i = 0; i < 0x10000; ++i )
                    {
                        if ( ( i & 0x1F ) == 0 )
                        {
                            reader.ReadInt32(); // header
                        }

                        staticTiles[i].ID = (ushort) i;
                        staticTiles[i].Flags = (TileFlags) reader.ReadInt64();
                        staticTiles[i].Weight = reader.ReadByte();
                        staticTiles[i].Quality = reader.ReadByte();
                        reader.ReadInt16();
                        reader.ReadByte();
                        staticTiles[i].Quantity = reader.ReadByte();
                        reader.ReadInt32();
                        reader.ReadByte();
                        /*int value = */
                        reader.ReadByte();
                        staticTiles[i].Height = reader.ReadByte();
                        staticTiles[i].Name = Encoding.ASCII.GetString( reader.ReadBytes( 20 ) ).TrimEnd( '\0' );
                    }
                }
            }

            ms.Close();
            return staticTiles;
        }

        private static LandTile[] LoadLandTiles()
        {
            string fileName = Path.Combine( _dataPath, "tiledata.mul" );

            if ( !File.Exists( fileName ) )
            {
                throw new FileNotFoundException( "File not found.", fileName );
            }

            byte[] fileBytes = File.ReadAllBytes( fileName );

            LandTile[] landTiles = new LandTile[16384];

            MemoryStream ms = new MemoryStream( fileBytes, false );

            using ( BinaryReader bin = new BinaryReader( ms ) )
            {
                ms.Seek( 36, SeekOrigin.Begin );

                string name = Encoding.ASCII.GetString( bin.ReadBytes( 20 ) ).TrimEnd( '\0' );

                if ( name == "VOID!!!!!!" )
                {
                    _oldFormat = true;
                }

                ms.Seek( 0, SeekOrigin.Begin );

                if ( _oldFormat )
                {
                    for ( int i = 0; i < 0x4000; ++i )
                    {
                        if ( i == 0 || i > 0 && ( i & 0x1f ) == 0 )
                        {
                            bin.ReadInt32(); // block header
                        }

                        landTiles[i].Flags = (TileFlags) bin.ReadInt32();
                        landTiles[i].ID = bin.ReadInt16();
                        landTiles[i].Name = Encoding.ASCII.GetString( bin.ReadBytes( 20 ) ).TrimEnd( '\0' );
                    }
                }
                else
                {
                    for ( int i = 0; i < 0x4000; ++i )
                    {
                        if ( i == 1 || i > 0 && ( i & 0x1f ) == 0 )
                        {
                            bin.ReadInt32(); // block header
                        }

                        landTiles[i].Flags = (TileFlags) bin.ReadInt64();
                        landTiles[i].ID = bin.ReadInt16();
                        landTiles[i].Name = Encoding.ASCII.GetString( bin.ReadBytes( 20 ) ).TrimEnd( '\0' );
                    }
                }
            }

            ms.Close();
            return landTiles;
        }

        public static LandTile GetLandTile( int index )
        {
            return _landTiles.Value[index];
        }

        public static StaticTile GetStaticTile( int index )
        {
            try
            {
                return _staticTiles.Value[index];
            }
            catch ( NullReferenceException )
            {
                return new StaticTile { Name = "unknown" };
            }
            catch ( IndexOutOfRangeException )
            {
                return _staticTiles.Value[0];
            }
        }

        public static Layer GetLayer( int id )
        {
            StaticTile td = GetStaticTile( id );

            if ( !td.Flags.HasFlag( TileFlags.Wearable ) )
            {
                return Layer.Invalid;
            }

            Layer layer = (Layer) td.Quality;

            return layer;
        }
    }
}